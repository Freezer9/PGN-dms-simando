using Microsoft.EntityFrameworkCore;
using Shouldly;
using Simando.Domain.Attachments;
using Simando.Domain.Audit;
using Simando.Domain.Directory;
using Simando.Domain.Geography;
using Simando.Domain.MasterData;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Notifications;
using Simando.Infrastructure.Persistence;
using Simando.Infrastructure.RecordHub;
using Simando.Infrastructure.Security;
using Simando.Infrastructure.Workflow;
using Simando.Integration.Tests;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.RecordHub;

// Same Testcontainers/seed pattern as WorkflowServiceTests.cs — drives real
// WorkflowService transitions, then asserts on CompanyDetailService's read
// model. CanSubmit/CanAct/CanChooseReviewers are precomputed for button
// visibility only; the actual enforcement stays in WorkflowService and is
// covered there, not re-tested here.
public class CompanyDetailServiceTests : IAsyncLifetime
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

    [Fact(DisplayName = "Draft record: creator can submit, no holder")]
    public async Task GetDetail_Draft_CreatorCanSubmit()
    {
        var seed = await SeedAsync();
        var detailService = NewDetailService();

        var detail = await detailService.GetDetailAsync(
            seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea });

        detail.ShouldNotBeNull();
        detail.Status.ShouldBe(RecordStatus.Draft);
        detail.HolderLabel.ShouldBeNull();
        detail.CanSubmit.ShouldBeTrue();
        detail.CanAct.ShouldBeFalse();
        detail.Nomor.ShouldBe(seed.Company.Nomor);
        detail.SalesRepName.ShouldBe("creator");
    }

    [Fact(DisplayName = "After submit: Area Head is the holder and can act; Regional Admin cannot")]
    public async Task GetDetail_AfterSubmit_AreaHeadCanAct()
    {
        var seed = await SeedAsync();
        var workflow = NewWorkflowService();
        var detailService = NewDetailService();
        await workflow.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea });

        var forAreaHead = await detailService.GetDetailAsync(
            seed.Company.Id, seed.AreaHeadUserId, seed.AreaHeadPermissions, new HashSet<Role> { Role.AreaHead });
        forAreaHead!.Status.ShouldBe(RecordStatus.AreaHead);
        forAreaHead.HolderLabel.ShouldBe("Area Head");
        forAreaHead.CanAct.ShouldBeTrue();
        forAreaHead.CurrentStepId.ShouldNotBeNull();

        var forRegionalAdmin = await detailService.GetDetailAsync(
            seed.Company.Id, seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });
        forRegionalAdmin!.CanAct.ShouldBeFalse();
    }

    [Fact(DisplayName = "Reviewer slot resolves a specific holder name, not just a role label")]
    public async Task GetDetail_ReviewerSlot_ResolvesSpecificHolderName()
    {
        var seed = await SeedAsync();
        var workflow = NewWorkflowService();
        var detailService = NewDetailService();
        await workflow.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea });
        await ActAreaHeadSetujuAsync(workflow, seed);
        var instanceId = await InstanceIdAsync(seed.Company.Id);
        await workflow.ChooseReviewersAsync(
            instanceId, [seed.Reviewer1UserId, seed.Reviewer2UserId],
            seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });
        var regionalAdminStep = await StepAsync(instanceId, WorkflowStepKind.RegionalAdmin);
        await workflow.ActAsync(regionalAdminStep.Id, WorkflowAction.Setuju, null, seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });

        var detail = await detailService.GetDetailAsync(
            seed.Company.Id, seed.Reviewer1UserId, seed.ReviewerPermissions, new HashSet<Role> { Role.Reviewer });

        detail!.Status.ShouldBe(RecordStatus.Reviewer1);
        detail.HolderLabel.ShouldBe("Reviewer 1");
        detail.HolderName.ShouldBe("reviewer.1");
    }

    [Fact(DisplayName = "Rejected record has no holder")]
    public async Task GetDetail_Rejected_NoHolder()
    {
        var seed = await SeedAsync();
        var workflow = NewWorkflowService();
        var detailService = NewDetailService();
        await workflow.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea });
        var areaHeadStep = await StepAsync(await InstanceIdAsync(seed.Company.Id), WorkflowStepKind.AreaHead);
        await workflow.ActAsync(areaHeadStep.Id, WorkflowAction.Tolak, "tidak sesuai", seed.AreaHeadUserId, seed.AreaHeadPermissions, new HashSet<Role> { Role.AreaHead });

        var detail = await detailService.GetDetailAsync(
            seed.Company.Id, seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });

        detail!.Status.ShouldBe(RecordStatus.Rejected);
        detail.HolderLabel.ShouldBeNull();
        detail.CanAct.ShouldBeFalse();
    }

    [Fact(DisplayName = "An actor scoped to a different Area can view the record but cannot act")]
    public async Task GetDetail_OutOfScopeActor_CannotActOrSubmit()
    {
        var seed = await SeedAsync();
        var detailService = NewDetailService();
        var outOfScopePermissions = new EffectivePermissions(AccessScope.Area, Guid.NewGuid(), null, seed.CreatorPermissions.Capabilities);

        var detail = await detailService.GetDetailAsync(
            seed.Company.Id, seed.CreatorId, outOfScopePermissions, new HashSet<Role> { Role.SalesArea });

        detail.ShouldNotBeNull();
        detail.CanSubmit.ShouldBeFalse();
    }

    [Fact(DisplayName = "GetDetailAsync returns null for a company that doesn't exist")]
    public async Task GetDetail_UnknownCompany_ReturnsNull()
    {
        var seed = await SeedAsync();
        var detailService = NewDetailService();

        var detail = await detailService.GetDetailAsync(
            Guid.NewGuid(), seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea });

        detail.ShouldBeNull();
    }

    [Fact(DisplayName = "GetTimelineAsync returns events newest-first with role labels")]
    public async Task GetTimeline_NewestFirstWithRoleLabels()
    {
        var seed = await SeedAsync();
        var workflow = NewWorkflowService();
        var detailService = NewDetailService();
        await workflow.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea });
        await ActAreaHeadSetujuAsync(workflow, seed);

        var timeline = await detailService.GetTimelineAsync(seed.Company.Id);

        timeline.Count.ShouldBe(2);
        timeline[0].Action.ShouldBe(StatusEventAction.Setuju);
        timeline[0].RoleLabel.ShouldBe("Area Head");
        timeline[0].ActorName.ShouldBe("area.head");
        timeline[1].Action.ShouldBe(StatusEventAction.Submit);
        timeline[1].RoleLabel.ShouldBe("Sales Area");
        timeline[0].OccurredAt.ShouldBeGreaterThanOrEqualTo(timeline[1].OccurredAt);
    }

    private async Task ActAreaHeadSetujuAsync(WorkflowService workflow, SeedData seed)
    {
        var instanceId = await InstanceIdAsync(seed.Company.Id);
        var areaHeadStep = await StepAsync(instanceId, WorkflowStepKind.AreaHead);
        var result = await workflow.ActAsync(
            areaHeadStep.Id, WorkflowAction.Setuju, null,
            seed.AreaHeadUserId, seed.AreaHeadPermissions, new HashSet<Role> { Role.AreaHead });
        result.Succeeded.ShouldBeTrue();
    }

    private async Task<Guid> InstanceIdAsync(Guid companyId)
    {
        await using var db = NewContext();
        return await db.WorkflowInstances.Where(i => i.CompanyId == companyId).Select(i => i.Id).SingleAsync();
    }

    private async Task<WorkflowStep> StepAsync(Guid instanceId, WorkflowStepKind kind)
    {
        await using var db = NewContext();
        return await db.WorkflowSteps.SingleAsync(s => s.WorkflowInstanceId == instanceId && s.Kind == kind);
    }

    private WorkflowService NewWorkflowService() => new(
        new SingleContextFactory(_container.GetConnectionString()),
        new InAppNotificationChannel(new SingleContextFactory(_container.GetConnectionString())));

    private BreakGlassService NewBreakGlassService() => new(
        new SingleContextFactory(_container.GetConnectionString()),
        new InAppNotificationChannel(new SingleContextFactory(_container.GetConnectionString())));

    private CompanyDetailService NewDetailService() => new(
        new SingleContextFactory(_container.GetConnectionString()),
        NewBreakGlassService());

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
        db.Regencies.Add(new Regency { Id = regencyId, ProvinceId = provinceId, BpsCode = "1101", Type = RegencyType.Kota, Name = "Test Regency" });
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

        return new SeedData(
            company, creator.Id, areaHead.Id, regionalAdmin.Id, reviewer1.Id, reviewer2.Id, divisionHead.Id,
            PermissionsFor(Role.SalesArea, AccessScope.Area, areaId, null),
            PermissionsFor(Role.AreaHead, AccessScope.Area, areaId, null),
            PermissionsFor(Role.RegionalAdmin, AccessScope.Region, null, regionId),
            PermissionsFor(Role.Reviewer, AccessScope.Region, null, regionId),
            PermissionsFor(Role.DivisionHead, AccessScope.Region, null, regionId));
    }

    private static ApplicationUser NewUser(string userName) =>
        new() { Id = Guid.NewGuid(), UserName = userName, FullName = userName };

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
