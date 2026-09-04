using Microsoft.EntityFrameworkCore;
using Shouldly;
using Simando.Domain.Attachments;
using Simando.Application.Tasks;
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
using Simando.Infrastructure.Tasks;
using Simando.Infrastructure.Workflow;
using Simando.Integration.Tests;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Tasks;

// Same Testcontainers/seed pattern as WorkflowServiceTests.cs — TasksService
// reads the state WorkflowService writes, so tests drive real workflow
// transitions first and then assert on the query service's output.
public class TasksServiceTests : IAsyncLifetime
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

    [Fact(DisplayName = "GetMyTasksAsync returns the company when it's the actor's turn, empty for a user who doesn't hold the role")]
    public async Task GetMyTasks_ReturnsOnlyHolderOfCurrentStep()
    {
        var seed = await SeedAsync();
        var workflow = NewWorkflowService();
        var tasks = NewTasksService();
        await workflow.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea });

        var areaHeadTasks = await tasks.GetMyTasksAsync(seed.AreaHeadUserId, seed.AreaHeadPermissions, new HashSet<Role> { Role.AreaHead });
        areaHeadTasks.Count.ShouldBe(1);
        areaHeadTasks[0].CompanyId.ShouldBe(seed.Company.Id);
        areaHeadTasks[0].StepKind.ShouldBe(WorkflowStepKind.AreaHead);

        var regionalAdminTasks = await tasks.GetMyTasksAsync(seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });
        regionalAdminTasks.ShouldBeEmpty();
    }

    [Fact(DisplayName = "After Area Head Setuju, the company leaves Area Head's list and appears in Regional Admin's")]
    public async Task GetMyTasks_AfterSetuju_MovesToNextHolder()
    {
        var seed = await SeedAsync();
        var workflow = NewWorkflowService();
        var tasks = NewTasksService();
        var instanceId = (await workflow.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea })).WorkflowInstanceId!.Value;
        await ActAreaHeadSetujuAsync(workflow, seed, instanceId);

        var areaHeadTasks = await tasks.GetMyTasksAsync(seed.AreaHeadUserId, seed.AreaHeadPermissions, new HashSet<Role> { Role.AreaHead });
        areaHeadTasks.ShouldBeEmpty();

        var regionalAdminTasks = await tasks.GetMyTasksAsync(seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });
        regionalAdminTasks.Count.ShouldBe(1);
        regionalAdminTasks[0].StepKind.ShouldBe(WorkflowStepKind.RegionalAdmin);
    }

    [Fact(DisplayName = "GetMyTasksAsync excludes a reviewer step assigned to a different specific reviewer")]
    public async Task GetMyTasks_ExcludesWrongSpecificReviewer()
    {
        var seed = await SeedAsync();
        var workflow = NewWorkflowService();
        var tasks = NewTasksService();
        var instanceId = (await workflow.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea })).WorkflowInstanceId!.Value;
        await ActAreaHeadSetujuAsync(workflow, seed, instanceId);
        await workflow.ChooseReviewersAsync(
            instanceId, [seed.Reviewer1UserId, seed.Reviewer2UserId],
            seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });
        var regionalAdminStep = await StepAsync(instanceId, WorkflowStepKind.RegionalAdmin);
        await workflow.ActAsync(regionalAdminStep.Id, WorkflowAction.Setuju, null, seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });

        var reviewer1Tasks = await tasks.GetMyTasksAsync(seed.Reviewer1UserId, seed.ReviewerPermissions, new HashSet<Role> { Role.Reviewer });
        reviewer1Tasks.Count.ShouldBe(1);

        var reviewer2Tasks = await tasks.GetMyTasksAsync(seed.Reviewer2UserId, seed.ReviewerPermissions, new HashSet<Role> { Role.Reviewer });
        reviewer2Tasks.ShouldBeEmpty();
    }

    [Fact(DisplayName = "GetRegionTasksAsync includes a company at a step held by a different role, excludes out-of-region companies")]
    public async Task GetRegionTasks_ScopedByRegion_NotByTurn()
    {
        var seed = await SeedAsync();
        var workflow = NewWorkflowService();
        var tasks = NewTasksService();
        await workflow.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea });

        var (otherRegionCompany, otherRegionCreatorPermissions) = await SeedCompanyInNewRegionAsync();
        await workflow.StartAsync(otherRegionCompany.Id, seed.CreatorId, otherRegionCreatorPermissions, new HashSet<Role> { Role.SalesArea });

        var regionTasks = await tasks.GetRegionTasksAsync(seed.RegionalAdminPermissions);

        regionTasks.ShouldContain(t => t.CompanyId == seed.Company.Id && t.StepKind == WorkflowStepKind.AreaHead);
        regionTasks.ShouldNotContain(t => t.CompanyId == otherRegionCompany.Id);
    }

    [Fact(DisplayName = "GetHistoryAsync returns a decision the actor made, most recent first")]
    public async Task GetHistory_ReturnsActorsOwnDecisions()
    {
        var seed = await SeedAsync();
        var workflow = NewWorkflowService();
        var tasks = NewTasksService();
        var instanceId = (await workflow.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea })).WorkflowInstanceId!.Value;
        await ActAreaHeadSetujuAsync(workflow, seed, instanceId);

        var history = await tasks.GetHistoryAsync(seed.AreaHeadUserId);

        history.Count.ShouldBe(1);
        history[0].CompanyId.ShouldBe(seed.Company.Id);
        history[0].Action.ShouldBe(StatusEventAction.Setuju);
        history[0].ToStatus.ShouldBe(RecordStatus.RegionalAdmin);
    }

    [Fact(DisplayName = "WaitingSince on the second step equals the first step's ActedAt, not the instance start")]
    public async Task GetMyTasks_WaitingSince_IsPredecessorActedAt()
    {
        var seed = await SeedAsync();
        var workflow = NewWorkflowService();
        var tasks = NewTasksService();
        var instanceId = (await workflow.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea })).WorkflowInstanceId!.Value;
        await ActAreaHeadSetujuAsync(workflow, seed, instanceId);

        var areaHeadStep = await StepAsync(instanceId, WorkflowStepKind.AreaHead);
        var regionalAdminTasks = await tasks.GetMyTasksAsync(seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });

        regionalAdminTasks.Count.ShouldBe(1);
        regionalAdminTasks[0].WaitingSince.ShouldBe(areaHeadStep.ActedAt!.Value);
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

    private TasksService NewTasksService() => new(new SingleContextFactory(_container.GetConnectionString()));

    private async Task<WorkflowStep> StepAsync(Guid instanceId, WorkflowStepKind kind)
    {
        await using var db = NewContext();
        return await db.WorkflowSteps.SingleAsync(s => s.WorkflowInstanceId == instanceId && s.Kind == kind);
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
    private Guid _creatorId;

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
            new Attachment { Id = Guid.NewGuid(), CompanyId = company.Id, AttachableType = "Company", AttachableId = company.Id, Kind = AttachmentKind.Kk0, Filename = "kk0.pdf", MimeType = "application/pdf", SizeBytes = 100, Checksum = "abc", StorageProvider = StorageProvider.S3, StorageKey = "kk0", UploadedBy = _creatorId != Guid.Empty ? _creatorId : company.CreatedBy, UploadedAt = DateTimeOffset.UtcNow, Version = 1 },
            new Attachment { Id = Guid.NewGuid(), CompanyId = company.Id, AttachableType = "Company", AttachableId = company.Id, Kind = AttachmentKind.A1, Filename = "a1.pdf", MimeType = "application/pdf", SizeBytes = 100, Checksum = "abc", StorageProvider = StorageProvider.S3, StorageKey = "a1", UploadedBy = _creatorId != Guid.Empty ? _creatorId : company.CreatedBy, UploadedAt = DateTimeOffset.UtcNow, Version = 1 },
            new Attachment { Id = Guid.NewGuid(), CompanyId = company.Id, AttachableType = "Company", AttachableId = company.Id, Kind = AttachmentKind.CapexPreGr3, Filename = "capex.pdf", MimeType = "application/pdf", SizeBytes = 100, Checksum = "abc", StorageProvider = StorageProvider.S3, StorageKey = "capex", UploadedBy = _creatorId != Guid.Empty ? _creatorId : company.CreatedBy, UploadedAt = DateTimeOffset.UtcNow, Version = 1 }
        );
        await db.SaveChangesAsync();

        _villageId = villageId;
        _industryTypeId = industryTypeId;
        _creatorId = creator.Id;

        return new SeedData(
            company, creator.Id, areaHead.Id, regionalAdmin.Id, reviewer1.Id, reviewer2.Id, divisionHead.Id,
            PermissionsFor(Role.SalesArea, AccessScope.Area, areaId, null),
            PermissionsFor(Role.AreaHead, AccessScope.Area, areaId, null),
            PermissionsFor(Role.RegionalAdmin, AccessScope.Region, null, regionId),
            PermissionsFor(Role.Reviewer, AccessScope.Region, null, regionId),
            PermissionsFor(Role.DivisionHead, AccessScope.Region, null, regionId));
    }

    // A second company in a brand-new Region/Area, reusing the village/industry
    // type/creator from SeedAsync — just enough for GetRegionTasksAsync's
    // out-of-region exclusion, without re-seeding the whole geography chain.
    // Returns creator permissions re-scoped to the new Area too, since the
    // same physical creator user now needs to submit into a different Area
    // than SeedData.CreatorPermissions covers.
    private async Task<(Company Company, EffectivePermissions CreatorPermissions)> SeedCompanyInNewRegionAsync()
    {
        await using var db = NewContext();

        var otherRegionId = Guid.NewGuid();
        var otherAreaId = Guid.NewGuid();
        db.Regions.Add(new Region { Id = otherRegionId, Code = "SOR-OTHER", Name = "Other Region", Active = true });
        db.Areas.Add(new Area { Id = otherAreaId, RegionId = otherRegionId, Code = "OTH", Name = "Other Area", Active = true });

        var company = new Company
        {
            Id = Guid.NewGuid(),
            Nomor = "2-11-1101",
            NamaPerusahaan = "PT Other Region",
            VillageId = _villageId,
            Alamat = "Jl. Other",
            IndustryTypeId = _industryTypeId,
            AreaId = otherAreaId,
            CurrentStage = 7,
            Status = RecordStatus.Draft,
            CreatedBy = _creatorId,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.Companies.Add(company);
        db.Attachments.AddRange(
            new Attachment { Id = Guid.NewGuid(), CompanyId = company.Id, AttachableType = "Company", AttachableId = company.Id, Kind = AttachmentKind.Kk0, Filename = "kk0.pdf", MimeType = "application/pdf", SizeBytes = 100, Checksum = "abc", StorageProvider = StorageProvider.S3, StorageKey = "kk0", UploadedBy = _creatorId != Guid.Empty ? _creatorId : company.CreatedBy, UploadedAt = DateTimeOffset.UtcNow, Version = 1 },
            new Attachment { Id = Guid.NewGuid(), CompanyId = company.Id, AttachableType = "Company", AttachableId = company.Id, Kind = AttachmentKind.A1, Filename = "a1.pdf", MimeType = "application/pdf", SizeBytes = 100, Checksum = "abc", StorageProvider = StorageProvider.S3, StorageKey = "a1", UploadedBy = _creatorId != Guid.Empty ? _creatorId : company.CreatedBy, UploadedAt = DateTimeOffset.UtcNow, Version = 1 },
            new Attachment { Id = Guid.NewGuid(), CompanyId = company.Id, AttachableType = "Company", AttachableId = company.Id, Kind = AttachmentKind.CapexPreGr3, Filename = "capex.pdf", MimeType = "application/pdf", SizeBytes = 100, Checksum = "abc", StorageProvider = StorageProvider.S3, StorageKey = "capex", UploadedBy = _creatorId != Guid.Empty ? _creatorId : company.CreatedBy, UploadedAt = DateTimeOffset.UtcNow, Version = 1 }
        );
        await db.SaveChangesAsync();

        return (company, PermissionsFor(Role.SalesArea, AccessScope.Area, otherAreaId, null));
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
