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
using Simando.Infrastructure.Reports;
using Simando.Infrastructure.Workflow;
using Simando.Integration.Tests;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Reports;

// PostGIS integration tests, container-per-test isolation — same pattern as
// WorkflowServiceTests.cs, whose real WorkflowService.StartAsync/ActAsync
// seed the state ReportsService reads rather than hand-crafting rows.
public class ReportsServiceTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgis/postgis:18-3.6-alpine")
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

    [Fact(DisplayName = "GetAgeingAsync: a submitted company appears with the Area Head role label, no name")]
    public async Task SubmittedCompany_AppearsWithAreaHeadRoleLabel()
    {
        var seed = await SeedAsync();
        await StartAsync(seed);

        var rows = await NewReportsService().GetAgeingAsync(seed.RegionalAdminPermissions);

        var row = rows.ShouldHaveSingleItem();
        row.CompanyId.ShouldBe(seed.Company.Id);
        row.StepKind.ShouldBe(WorkflowStepKind.AreaHead);
        row.ActorLabel.ShouldBe("Area Head");
    }

    [Fact(DisplayName = "GetAgeingAsync: an assigned reviewer step resolves ActorLabel to \"Reviewer 2 (name)\"")]
    public async Task AssignedReviewerStep_ActorLabelIncludesName()
    {
        var seed = await SeedAsync();
        var instanceId = await StartAsync(seed);
        await ActAsync(instanceId, WorkflowStepKind.AreaHead, seed.AreaHeadUserId, seed.AreaHeadPermissions, Role.AreaHead);

        var workflowService = NewWorkflowService();
        var chooseResult = await workflowService.ChooseReviewersAsync(
            instanceId, [seed.Reviewer1UserId, seed.Reviewer2UserId],
            seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });
        chooseResult.Succeeded.ShouldBeTrue();
        await ActAsync(instanceId, WorkflowStepKind.RegionalAdmin, seed.RegionalAdminUserId, seed.RegionalAdminPermissions, Role.RegionalAdmin);

        var rows = await NewReportsService().GetAgeingAsync(seed.RegionalAdminPermissions);

        var row = rows.ShouldHaveSingleItem();
        row.StepKind.ShouldBe(WorkflowStepKind.Reviewer1);
        row.ActorLabel.ShouldBe("Reviewer 1 (reviewer.1)");
    }

    [Fact(DisplayName = "GetAgeingAsync: a Draft company (never submitted) is excluded")]
    public async Task DraftCompany_Excluded()
    {
        var seed = await SeedAsync();

        var rows = await NewReportsService().GetAgeingAsync(seed.RegionalAdminPermissions);

        rows.ShouldBeEmpty();
    }

    [Fact(DisplayName = "GetAgeingAsync: a company outside the actor's Area scope is excluded")]
    public async Task OutOfScopeCompany_Excluded()
    {
        var seed = await SeedAsync();
        await StartAsync(seed);

        var outOfScopeActor = new EffectivePermissions(AccessScope.Area, Guid.NewGuid(), null, seed.AreaHeadPermissions.Capabilities);
        var rows = await NewReportsService().GetAgeingAsync(outOfScopeActor);

        rows.ShouldBeEmpty();
    }

    [Fact(DisplayName = "GetAgeingAsync: rows sort oldest-waiting first")]
    public async Task SortedOldestWaitFirst()
    {
        var seedA = await SeedAsync();
        var instanceA = await StartAsync(seedA);
        var seedB = await SeedAsync();
        await StartAsync(seedB);

        await using (var db = NewContext())
        {
            await db.WorkflowInstances
                .Where(i => i.Id == instanceA)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.StartedAt, DateTimeOffset.UtcNow.AddDays(-10)));
        }

        var rows = await NewReportsService().GetAgeingAsync(new EffectivePermissions(AccessScope.All, null, null, seedA.RegionalAdminPermissions.Capabilities));

        rows.Count.ShouldBe(2);
        rows[0].CompanyId.ShouldBe(seedA.Company.Id);
        rows[1].CompanyId.ShouldBe(seedB.Company.Id);
    }

    private async Task<Guid> StartAsync(SeedData seed) =>
        (await NewWorkflowService().StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea }))
        .WorkflowInstanceId!.Value;

    private async Task ActAsync(Guid instanceId, WorkflowStepKind kind, Guid actorId, EffectivePermissions actorPermissions, Role role)
    {
        var step = await StepAsync(instanceId, kind);
        var result = await NewWorkflowService().ActAsync(step.Id, WorkflowAction.Setuju, null, actorId, actorPermissions, new HashSet<Role> { role });
        result.Succeeded.ShouldBeTrue();
    }

    private async Task<WorkflowStep> StepAsync(Guid instanceId, WorkflowStepKind kind)
    {
        await using var db = NewContext();
        return await db.WorkflowSteps.SingleAsync(s => s.WorkflowInstanceId == instanceId && s.Kind == kind);
    }

    private ReportsService NewReportsService() => new(new SingleContextFactory(_container.GetConnectionString()));

    private WorkflowService NewWorkflowService() => new(
        new SingleContextFactory(_container.GetConnectionString()),
        new InAppNotificationChannel(new SingleContextFactory(_container.GetConnectionString())));

    private sealed record SeedData(
        Company Company,
        Guid CreatorId,
        Guid AreaHeadUserId,
        Guid RegionalAdminUserId,
        Guid Reviewer1UserId,
        Guid Reviewer2UserId,
        EffectivePermissions CreatorPermissions,
        EffectivePermissions AreaHeadPermissions,
        EffectivePermissions RegionalAdminPermissions);

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

        db.Provinces.Add(new Province { Id = provinceId, BpsCode = Guid.NewGuid().ToString("N")[..6], Name = "Test Province" });
        db.Regencies.Add(new Regency { Id = regencyId, ProvinceId = provinceId, BpsCode = Guid.NewGuid().ToString("N")[..6], Type = RegencyType.Kabupaten, Name = "Test Regency" });
        db.Districts.Add(new District { Id = districtId, RegencyId = regencyId, BpsCode = Guid.NewGuid().ToString("N")[..6], Name = "Test District" });
        db.Villages.Add(new Village { Id = villageId, DistrictId = districtId, BpsCode = Guid.NewGuid().ToString("N")[..10], Type = VillageType.Desa, Name = "Test Village" });
        db.IndustryTypes.Add(new IndustryType { Id = industryTypeId, Name = $"Test Industry {Guid.NewGuid():N}" });
        db.Regions.Add(new Region { Id = regionId, Code = Guid.NewGuid().ToString("N")[..8], Name = "Test Region", Active = true });
        db.Areas.Add(new Area { Id = areaId, RegionId = regionId, Code = Guid.NewGuid().ToString("N")[..8], Name = "Test Area", Active = true });

        var creator = NewUser("creator");
        var areaHead = NewUser("area.head");
        var regionalAdmin = NewUser("regional.admin");
        var reviewer1 = NewUser("reviewer.1");
        var reviewer2 = NewUser("reviewer.2");
        db.Users.AddRange(creator, areaHead, regionalAdmin, reviewer1, reviewer2);

        await db.SaveChangesAsync();

        var company = new Company
        {
            Id = Guid.NewGuid(),
            Nomor = $"1-11-{Guid.NewGuid().ToString("N")[..4]}",
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
            company, creator.Id, areaHead.Id, regionalAdmin.Id, reviewer1.Id, reviewer2.Id,
            PermissionsFor(Role.SalesArea, AccessScope.Area, areaId, null),
            PermissionsFor(Role.AreaHead, AccessScope.Area, areaId, null),
            PermissionsFor(Role.RegionalAdmin, AccessScope.Region, null, regionId));
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
