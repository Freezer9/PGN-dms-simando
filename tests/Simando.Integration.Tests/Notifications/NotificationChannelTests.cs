using Microsoft.EntityFrameworkCore;
using Shouldly;
using Simando.Domain.Attachments;
using Simando.Domain.Directory;
using Simando.Domain.Geography;
using Simando.Domain.MasterData;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Notifications;
using Simando.Infrastructure.Persistence;
using Simando.Infrastructure.Workflow;
using Simando.Integration.Tests;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Notifications;

// Same Testcontainers/seed pattern as WorkflowServiceTests.cs, but unlike
// that file this one also inserts real RoleAssignment rows — WorkflowService
// takes EffectivePermissions/actorRoles as explicit parameters (bypassing
// RoleAssignment entirely) for the permission checks, but notification
// fan-out for role-resolved steps (Area Head/Regional Admin/Division Head)
// queries RoleAssignment directly to find who holds the role, so those rows
// have to actually exist here.
public class NotificationChannelTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("imresamu/postgis:18-3.6-alpine")
        .WithDatabase("simando")
        .WithUsername("simando")
        .WithPassword("simando")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = NewContext();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    [Fact(DisplayName = "StartAsync notifies the Area Head")]
    public async Task StartAsync_NotifiesAreaHead()
    {
        var seed = await SeedAsync();
        var workflow = NewWorkflowService();

        await workflow.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea });

        var notifications = await NotificationsForAsync(seed.Company.Id);
        notifications.ShouldContain(n => n.RecipientUserId == seed.AreaHeadUserId);
    }

    [Fact(DisplayName = "Area Head Setuju notifies the Regional Admin")]
    public async Task AreaHeadSetuju_NotifiesRegionalAdmin()
    {
        var seed = await SeedAsync();
        var workflow = NewWorkflowService();
        var instanceId = (await workflow.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea })).WorkflowInstanceId!.Value;
        await ActAreaHeadSetujuAsync(workflow, seed, instanceId);

        var notifications = await NotificationsForAsync(seed.Company.Id);
        notifications.ShouldContain(n => n.RecipientUserId == seed.RegionalAdminUserId);
    }

    [Fact(DisplayName = "ChooseReviewersAsync notifies both chosen reviewers")]
    public async Task ChooseReviewers_NotifiesBothReviewers()
    {
        var seed = await SeedAsync();
        var workflow = NewWorkflowService();
        var instanceId = (await workflow.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea })).WorkflowInstanceId!.Value;
        await ActAreaHeadSetujuAsync(workflow, seed, instanceId);

        await workflow.ChooseReviewersAsync(
            instanceId, [seed.Reviewer1UserId, seed.Reviewer2UserId],
            seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });

        var notifications = await NotificationsForAsync(seed.Company.Id);
        notifications.ShouldContain(n => n.RecipientUserId == seed.Reviewer1UserId);
        notifications.ShouldContain(n => n.RecipientUserId == seed.Reviewer2UserId);
    }

    [Fact(DisplayName = "Area Head Revisi (back to Draft) notifies the creator")]
    public async Task AreaHeadRevisi_NotifiesCreator()
    {
        var seed = await SeedAsync();
        var workflow = NewWorkflowService();
        var instanceId = (await workflow.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea })).WorkflowInstanceId!.Value;
        var areaHeadStep = await StepAsync(instanceId, WorkflowStepKind.AreaHead);

        var result = await workflow.ActAsync(
            areaHeadStep.Id, WorkflowAction.Revisi, "tidak lengkap",
            seed.AreaHeadUserId, seed.AreaHeadPermissions, new HashSet<Role> { Role.AreaHead });
        result.Succeeded.ShouldBeTrue();

        var notifications = await NotificationsForAsync(seed.Company.Id);
        notifications.ShouldContain(n => n.RecipientUserId == seed.CreatorId);
    }

    [Fact(DisplayName = "Tolak from mid-chain notifies the Regional Admin, not the creator")]
    public async Task Tolak_NotifiesRegionalAdmin_NotCreator()
    {
        var seed = await SeedAsync();
        var workflow = NewWorkflowService();
        var instanceId = (await workflow.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea })).WorkflowInstanceId!.Value;
        await ActAreaHeadSetujuAsync(workflow, seed, instanceId);
        await workflow.ChooseReviewersAsync(
            instanceId, [seed.Reviewer1UserId, seed.Reviewer2UserId],
            seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });

        var regionalAdminStep = await StepAsync(instanceId, WorkflowStepKind.RegionalAdmin);
        await workflow.ActAsync(regionalAdminStep.Id, WorkflowAction.Setuju, null, seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });

        var reviewer1Step = await StepAsync(instanceId, WorkflowStepKind.Reviewer1);

        await ClearNotificationsAsync();

        var tolak = await workflow.ActAsync(reviewer1Step.Id, WorkflowAction.Tolak, "data salah", seed.Reviewer1UserId, seed.ReviewerPermissions, new HashSet<Role> { Role.Reviewer });
        tolak.Succeeded.ShouldBeTrue();

        var notifications = await NotificationsForAsync(seed.Company.Id);
        notifications.ShouldContain(n => n.RecipientUserId == seed.RegionalAdminUserId);
        notifications.ShouldNotContain(n => n.RecipientUserId == seed.CreatorId);
    }

    [Fact(DisplayName = "Full chain reaching IssuedNol notifies the creator")]
    public async Task FullChain_ReachesIssuedNol_NotifiesCreator()
    {
        var seed = await SeedAsync();
        var workflow = NewWorkflowService();
        var instanceId = (await workflow.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea })).WorkflowInstanceId!.Value;
        await ActAreaHeadSetujuAsync(workflow, seed, instanceId);
        await workflow.ChooseReviewersAsync(
            instanceId, [seed.Reviewer1UserId, seed.Reviewer2UserId],
            seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });

        var regionalAdminStep = await StepAsync(instanceId, WorkflowStepKind.RegionalAdmin);
        await workflow.ActAsync(regionalAdminStep.Id, WorkflowAction.Setuju, null, seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });

        var reviewer1Step = await StepAsync(instanceId, WorkflowStepKind.Reviewer1);
        await workflow.ActAsync(reviewer1Step.Id, WorkflowAction.Setuju, null, seed.Reviewer1UserId, seed.ReviewerPermissions, new HashSet<Role> { Role.Reviewer });

        var reviewer2Step = await StepAsync(instanceId, WorkflowStepKind.Reviewer2);
        await workflow.ActAsync(reviewer2Step.Id, WorkflowAction.Setuju, null, seed.Reviewer2UserId, seed.ReviewerPermissions, new HashSet<Role> { Role.Reviewer });

        var divisionHeadStep = await StepAsync(instanceId, WorkflowStepKind.DivisionHead);
        await ClearNotificationsAsync();

        var final = await workflow.ActAsync(divisionHeadStep.Id, WorkflowAction.Setuju, null, seed.DivisionHeadUserId, seed.DivisionHeadPermissions, new HashSet<Role> { Role.DivisionHead });
        final.Succeeded.ShouldBeTrue();
        final.NewStatus.ShouldBe(RecordStatus.IssuedNol);

        var notifications = await NotificationsForAsync(seed.Company.Id);
        notifications.ShouldContain(n => n.RecipientUserId == seed.CreatorId);
    }

    [Fact(DisplayName = "StartAsync in an Area with no Area Head role assignment notifies nobody, doesn't crash")]
    public async Task StartAsync_NoAreaHeadAssigned_NotifiesNobody()
    {
        var seed = await SeedAsync();
        var workflow = NewWorkflowService();

        var (emptyAreaCompany, emptyAreaCreatorPermissions) = await SeedCompanyWithNoRoleAssignmentsAsync(seed.CreatorId);

        await workflow.StartAsync(emptyAreaCompany.Id, seed.CreatorId, emptyAreaCreatorPermissions, new HashSet<Role> { Role.SalesArea });

        var notifications = await NotificationsForAsync(emptyAreaCompany.Id);
        notifications.ShouldBeEmpty();
    }

    private async Task ActAreaHeadSetujuAsync(WorkflowService workflow, SeedData seed, Guid instanceId)
    {
        var areaHeadStep = await StepAsync(instanceId, WorkflowStepKind.AreaHead);
        var result = await workflow.ActAsync(
            areaHeadStep.Id, WorkflowAction.Setuju, null,
            seed.AreaHeadUserId, seed.AreaHeadPermissions, new HashSet<Role> { Role.AreaHead });
        result.Succeeded.ShouldBeTrue();
    }

    private WorkflowService NewWorkflowService() => new(
        new SingleContextFactory(_container.GetConnectionString()),
        new InAppNotificationChannel(new SingleContextFactory(_container.GetConnectionString())));

    private async Task<WorkflowStep> StepAsync(Guid instanceId, WorkflowStepKind kind)
    {
        await using var db = NewContext();
        return await db.WorkflowSteps.SingleAsync(s => s.WorkflowInstanceId == instanceId && s.Kind == kind);
    }

    private async Task<List<Domain.Notifications.Notification>> NotificationsForAsync(Guid companyId)
    {
        await using var db = NewContext();
        return await db.Notifications.Where(n => n.CompanyId == companyId).ToListAsync();
    }

    // Some tests assert on notifications from one specific transition only —
    // clearing prior transitions' rows keeps those assertions from having to
    // also account for earlier steps' notifications.
    private async Task ClearNotificationsAsync()
    {
        await using var db = NewContext();
        await db.Notifications.ExecuteDeleteAsync();
    }

    private sealed record SeedData(
        Company Company,
        Guid CreatorId,
        Guid AreaHeadUserId,
        Guid RegionalAdminUserId,
        Guid Reviewer1UserId,
        Guid Reviewer2UserId,
        Guid DivisionHeadUserId,
        EffectivePermissions CreatorPermissions,
        EffectivePermissions AreaHeadPermissions,
        EffectivePermissions RegionalAdminPermissions,
        EffectivePermissions ReviewerPermissions,
        EffectivePermissions DivisionHeadPermissions);

    private Guid _villageId;
    private Guid _industryTypeId;

    private async Task<SeedData> SeedAsync()
    {
        await using var db = NewContext();

        var provinceId = Guid.NewGuid();
        var regencyId = Guid.NewGuid();
        var districtId = Guid.NewGuid();
        var villageId = Guid.NewGuid();
        var industryTypeId = Guid.NewGuid();
        var regionId = Guid.NewGuid();
        var areaId = Guid.NewGuid();

        db.Provinces.Add(new Province { Id = provinceId, BpsCode = "11", Name = "Test Province" });
        db.Regencies.Add(new Regency { Id = regencyId, ProvinceId = provinceId, BpsCode = "1101", Type = RegencyType.Kabupaten, Name = "Test Regency" });
        db.Districts.Add(new District { Id = districtId, RegencyId = regencyId, BpsCode = "110101", Name = "Test District" });
        db.Villages.Add(new Village { Id = villageId, DistrictId = districtId, BpsCode = "1101012001", Type = VillageType.Desa, Name = "Test Village" });
        db.IndustryTypes.Add(new IndustryType { Id = industryTypeId, Name = "Test Industry" });
        db.Regions.Add(new Region { Id = regionId, Code = "SOR-TEST", Name = "Test Region", Active = true });
        db.Areas.Add(new Area { Id = areaId, RegionId = regionId, Code = "TST", Name = "Test Area", Active = true });

        var creator = NewUser("creator");
        var areaHead = NewUser("area.head");
        var regionalAdmin = NewUser("regional.admin");
        var reviewer1 = NewUser("reviewer.1");
        var reviewer2 = NewUser("reviewer.2");
        var divisionHead = NewUser("division.head");
        db.Users.AddRange(creator, areaHead, regionalAdmin, reviewer1, reviewer2, divisionHead);

        db.RoleAssignments.AddRange(
            NewAssignment(areaHead.Id, Role.AreaHead, areaId: areaId, regionId: null),
            NewAssignment(regionalAdmin.Id, Role.RegionalAdmin, areaId: null, regionId: regionId),
            NewAssignment(divisionHead.Id, Role.DivisionHead, areaId: null, regionId: regionId));

        await db.SaveChangesAsync();

        var company = new Company
        {
            Id = Guid.NewGuid(),
            Nomor = "1-11-1101",
            NamaPerusahaan = "PT Test",
            VillageId = villageId,
            Alamat = "Jl. Test",
            IndustryTypeId = industryTypeId,
            AreaId = areaId,
            CurrentStage = 7,
            Status = RecordStatus.Draft,
            CreatedBy = creator.Id,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.Companies.Add(company);
        db.Attachments.AddRange(
            new Attachment { Id = Guid.NewGuid(), CompanyId = company.Id, AttachableType = "Company", AttachableId = company.Id, Kind = AttachmentKind.Kk0, Filename = "kk0.pdf", MimeType = "application/pdf", SizeBytes = 100, Checksum = "abc", StorageProvider = StorageProvider.S3, StorageKey = "kk0", UploadedBy = creator.Id, UploadedAt = DateTimeOffset.UtcNow, Version = 1 },
            new Attachment { Id = Guid.NewGuid(), CompanyId = company.Id, AttachableType = "Company", AttachableId = company.Id, Kind = AttachmentKind.A1, Filename = "a1.pdf", MimeType = "application/pdf", SizeBytes = 100, Checksum = "abc", StorageProvider = StorageProvider.S3, StorageKey = "a1", UploadedBy = creator.Id, UploadedAt = DateTimeOffset.UtcNow, Version = 1 },
            new Attachment { Id = Guid.NewGuid(), CompanyId = company.Id, AttachableType = "Company", AttachableId = company.Id, Kind = AttachmentKind.CapexPreGr3, Filename = "capex.pdf", MimeType = "application/pdf", SizeBytes = 100, Checksum = "abc", StorageProvider = StorageProvider.S3, StorageKey = "capex", UploadedBy = creator.Id, UploadedAt = DateTimeOffset.UtcNow, Version = 1 }
        );
        await db.SaveChangesAsync();

        _villageId = villageId;
        _industryTypeId = industryTypeId;

        return new SeedData(
            company, creator.Id, areaHead.Id, regionalAdmin.Id, reviewer1.Id, reviewer2.Id, divisionHead.Id,
            PermissionsFor(Role.SalesArea, AccessScope.Area, areaId, null),
            PermissionsFor(Role.AreaHead, AccessScope.Area, areaId, null),
            PermissionsFor(Role.RegionalAdmin, AccessScope.Region, null, regionId),
            PermissionsFor(Role.Reviewer, AccessScope.Region, null, regionId),
            PermissionsFor(Role.DivisionHead, AccessScope.Region, null, regionId));
    }

    // A company in a brand-new Region/Area with zero RoleAssignment rows —
    // reuses SeedAsync's village/industry type so it doesn't need the whole
    // geography chain re-seeded. Returns creator permissions re-scoped to the
    // new Area too, since the same physical creator user now needs to submit
    // into a different Area than SeedData.CreatorPermissions covers.
    private async Task<(Company Company, EffectivePermissions CreatorPermissions)> SeedCompanyWithNoRoleAssignmentsAsync(Guid creatorId)
    {
        await using var db = NewContext();

        var emptyRegionId = Guid.NewGuid();
        var emptyAreaId = Guid.NewGuid();
        db.Regions.Add(new Region { Id = emptyRegionId, Code = "SOR-EMPTY", Name = "Empty Region", Active = true });
        db.Areas.Add(new Area { Id = emptyAreaId, RegionId = emptyRegionId, Code = "EMP", Name = "Empty Area", Active = true });

        var company = new Company
        {
            Id = Guid.NewGuid(),
            Nomor = "3-11-1101",
            NamaPerusahaan = "PT No Area Head",
            VillageId = _villageId,
            Alamat = "Jl. Empty",
            IndustryTypeId = _industryTypeId,
            AreaId = emptyAreaId,
            CurrentStage = 7,
            Status = RecordStatus.Draft,
            CreatedBy = creatorId,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.Companies.Add(company);
        db.Attachments.AddRange(
            new Attachment { Id = Guid.NewGuid(), CompanyId = company.Id, AttachableType = "Company", AttachableId = company.Id, Kind = AttachmentKind.Kk0, Filename = "kk0.pdf", MimeType = "application/pdf", SizeBytes = 100, Checksum = "abc", StorageProvider = StorageProvider.S3, StorageKey = "kk0", UploadedBy = creatorId, UploadedAt = DateTimeOffset.UtcNow, Version = 1 },
            new Attachment { Id = Guid.NewGuid(), CompanyId = company.Id, AttachableType = "Company", AttachableId = company.Id, Kind = AttachmentKind.A1, Filename = "a1.pdf", MimeType = "application/pdf", SizeBytes = 100, Checksum = "abc", StorageProvider = StorageProvider.S3, StorageKey = "a1", UploadedBy = creatorId, UploadedAt = DateTimeOffset.UtcNow, Version = 1 },
            new Attachment { Id = Guid.NewGuid(), CompanyId = company.Id, AttachableType = "Company", AttachableId = company.Id, Kind = AttachmentKind.CapexPreGr3, Filename = "capex.pdf", MimeType = "application/pdf", SizeBytes = 100, Checksum = "abc", StorageProvider = StorageProvider.S3, StorageKey = "capex", UploadedBy = creatorId, UploadedAt = DateTimeOffset.UtcNow, Version = 1 }
        );
        await db.SaveChangesAsync();

        return (company, PermissionsFor(Role.SalesArea, AccessScope.Area, emptyAreaId, null));
    }

    private static ApplicationUser NewUser(string userName) =>
        new() { Id = Guid.NewGuid(), UserName = userName, FullName = userName };

    private static RoleAssignment NewAssignment(Guid userId, Role role, Guid? areaId, Guid? regionId) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Role = role,
            AreaId = areaId,
            RegionId = regionId,
            Active = true,
            AssignedAt = DateTimeOffset.UtcNow,
        };

    private static EffectivePermissions PermissionsFor(Role role, AccessScope scope, Guid? areaId, Guid? regionId) =>
        new(scope, areaId, regionId, RoleCapabilities.For(role).Capabilities);

    private SimandoDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<SimandoDbContext>()
            .UseNpgsql(_container.GetConnectionString(), npgsql => npgsql.UseNetTopologySuite())
            .UseSnakeCaseNamingConvention()
            .Options;

        return new SimandoDbContext(options, new UnrestrictedCurrentUser());
    }

    private sealed class SingleContextFactory(string connectionString) : IDbContextFactory<SimandoDbContext>
    {
        public SimandoDbContext CreateDbContext() => Build();

        public Task<SimandoDbContext> CreateDbContextAsync(CancellationToken ct = default) => Task.FromResult(Build());

        private SimandoDbContext Build()
        {
            var options = new DbContextOptionsBuilder<SimandoDbContext>()
                .UseNpgsql(connectionString, npgsql => npgsql.UseNetTopologySuite())
                .UseSnakeCaseNamingConvention()
                .Options;

            return new SimandoDbContext(options, new UnrestrictedCurrentUser());
        }
    }
}
