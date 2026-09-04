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
using Simando.Infrastructure.Workflow;
using Simando.Integration.Tests;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Workflow;

// PostGIS integration tests, container-per-test isolation — same pattern as
// OrganisationServiceTests.cs. WorkflowStep's FKs to ApplicationUser are
// real (Restrict), so every actor referenced needs a real app_user row, not
// just an arbitrary Guid — SeedAsync builds the full chain once per test.
public class WorkflowServiceTests : IAsyncLifetime
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

    [Fact(DisplayName = "StartAsync creates the instance and the first two steps, moves status to AreaHead")]
    public async Task StartAsync_CreatesInstanceAndFirstSteps()
    {
        var seed = await SeedAsync();
        var service = NewService();

        var instanceId = (await service.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea })).WorkflowInstanceId!.Value;

        await using var verify = NewContext();
        var company = await verify.Companies.SingleAsync(c => c.Id == seed.Company.Id);
        company.Status.ShouldBe(RecordStatus.AreaHead);

        var steps = await verify.WorkflowSteps
            .Where(s => s.WorkflowInstanceId == instanceId)
            .OrderBy(s => s.StepOrder)
            .ToListAsync();
        steps.Select(s => s.Kind).ShouldBe([WorkflowStepKind.AreaHead, WorkflowStepKind.RegionalAdmin]);

        var submitEvent = await verify.StatusEvents.SingleAsync(e => e.CompanyId == seed.Company.Id);
        submitEvent.Action.ShouldBe(StatusEventAction.Submit);
        submitEvent.ToStatus.ShouldBe(RecordStatus.AreaHead);
        submitEvent.WorkflowStepId.ShouldBe(steps[0].Id);
    }

    [Fact(DisplayName = "StartAsync: someone other than the creator cannot submit")]
    public async Task StartAsync_NotCreator_Rejected()
    {
        var seed = await SeedAsync();
        var service = NewService();

        var result = await service.StartAsync(seed.Company.Id, seed.AreaHeadUserId, seed.AreaHeadPermissions, new HashSet<Role> { Role.AreaHead });

        result.Succeeded.ShouldBeFalse();
        result.WorkflowInstanceId.ShouldBeNull();

        await using var verify = NewContext();
        var company = await verify.Companies.SingleAsync(c => c.Id == seed.Company.Id);
        company.Status.ShouldBe(RecordStatus.Draft);
    }

    [Fact(DisplayName = "StartAsync: creator without SubmitForApproval capability is rejected")]
    public async Task StartAsync_MissingCapability_Rejected()
    {
        var seed = await SeedAsync();
        var service = NewService();
        var noCapabilityPermissions = new EffectivePermissions(AccessScope.Area, seed.CreatorPermissions.AreaId, null, new HashSet<Capability>());

        var result = await service.StartAsync(seed.Company.Id, seed.CreatorId, noCapabilityPermissions, new HashSet<Role> { Role.SalesArea });

        result.Succeeded.ShouldBeFalse();
    }

    [Fact(DisplayName = "StartAsync: creator scoped to a different Area cannot submit")]
    public async Task StartAsync_OutOfScope_Rejected()
    {
        var seed = await SeedAsync();
        var service = NewService();
        var outOfScopePermissions = new EffectivePermissions(AccessScope.Area, Guid.NewGuid(), null, seed.CreatorPermissions.Capabilities);

        var result = await service.StartAsync(seed.Company.Id, seed.CreatorId, outOfScopePermissions, new HashSet<Role> { Role.SalesArea });

        result.Succeeded.ShouldBeFalse();
    }

    [Fact(DisplayName = "StartAsync: company at stage < 6 is rejected")]
    public async Task StartAsync_StageLessThan6_Rejected()
    {
        var seed = await SeedAsync();
        await using (var db = NewContext())
        {
            var company = await db.Companies.SingleAsync(c => c.Id == seed.Company.Id);
            company.CurrentStage = 2;
            await db.SaveChangesAsync();
        }

        var service = NewService();
        var result = await service.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea });

        result.Succeeded.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldContain("Tahap 6");
    }

    [Fact(DisplayName = "ActAsync: Area Head setuju advances to Regional Admin")]
    public async Task ActAsync_AreaHeadSetuju_AdvancesToRegionalAdmin()
    {
        var seed = await SeedAsync();
        var service = NewService();
        var instanceId = (await service.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea })).WorkflowInstanceId!.Value;
        var areaHeadStep = await StepAsync(instanceId, WorkflowStepKind.AreaHead);

        var result = await service.ActAsync(
            areaHeadStep.Id, WorkflowAction.Setuju, null,
            seed.AreaHeadUserId, seed.AreaHeadPermissions, new HashSet<Role> { Role.AreaHead });

        result.Succeeded.ShouldBeTrue();
        result.NewStatus.ShouldBe(RecordStatus.RegionalAdmin);
    }

    [Fact(DisplayName = "ActAsync: wrong role cannot act on Area Head's step")]
    public async Task ActAsync_WrongRole_Rejected()
    {
        var seed = await SeedAsync();
        var service = NewService();
        var instanceId = (await service.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea })).WorkflowInstanceId!.Value;
        var areaHeadStep = await StepAsync(instanceId, WorkflowStepKind.AreaHead);

        var result = await service.ActAsync(
            areaHeadStep.Id, WorkflowAction.Setuju, null,
            seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });

        result.Succeeded.ShouldBeFalse();
    }

    [Fact(DisplayName = "ActAsync: acting on a step that isn't current is rejected")]
    public async Task ActAsync_NotCurrentStep_Rejected()
    {
        var seed = await SeedAsync();
        var service = NewService();
        var instanceId = (await service.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea })).WorkflowInstanceId!.Value;
        var regionalAdminStep = await StepAsync(instanceId, WorkflowStepKind.RegionalAdmin);

        var result = await service.ActAsync(
            regionalAdminStep.Id, WorkflowAction.Setuju, null,
            seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });

        result.Succeeded.ShouldBeFalse();
    }

    [Fact(DisplayName = "ActAsync: empty comment on Revisi is rejected")]
    public async Task ActAsync_EmptyCommentOnRevisi_Rejected()
    {
        var seed = await SeedAsync();
        var service = NewService();
        var instanceId = (await service.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea })).WorkflowInstanceId!.Value;
        var areaHeadStep = await StepAsync(instanceId, WorkflowStepKind.AreaHead);

        var result = await service.ActAsync(
            areaHeadStep.Id, WorkflowAction.Revisi, null,
            seed.AreaHeadUserId, seed.AreaHeadPermissions, new HashSet<Role> { Role.AreaHead });

        result.Succeeded.ShouldBeFalse();
    }

    [Fact(DisplayName = "ChooseReviewersAsync then Regional Admin setuju advances into Reviewer 1, only the chosen reviewer is assigned")]
    public async Task ChooseReviewers_ThenSetuju_AdvancesToReviewer1()
    {
        var seed = await SeedAsync();
        var service = NewService();
        var instanceId = (await service.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea })).WorkflowInstanceId!.Value;
        await ActAreaHeadSetujuAsync(service, seed, instanceId);

        var chooseResult = await service.ChooseReviewersAsync(
            instanceId, [seed.Reviewer1UserId, seed.Reviewer2UserId],
            seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });
        chooseResult.Succeeded.ShouldBeTrue();

        var regionalAdminStep = await StepAsync(instanceId, WorkflowStepKind.RegionalAdmin);
        var actResult = await service.ActAsync(
            regionalAdminStep.Id, WorkflowAction.Setuju, null,
            seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });

        actResult.Succeeded.ShouldBeTrue();
        actResult.NewStatus.ShouldBe(RecordStatus.Reviewer1);

        var reviewer1Step = await StepAsync(instanceId, WorkflowStepKind.Reviewer1);
        reviewer1Step.AssignedUserId.ShouldBe(seed.Reviewer1UserId);

        // Reviewer2's user cannot act on Reviewer1's step even though both hold Role.Reviewer.
        var wrongReviewer = await service.ActAsync(
            reviewer1Step.Id, WorkflowAction.Setuju, null,
            seed.Reviewer2UserId, seed.ReviewerPermissions, new HashSet<Role> { Role.Reviewer });
        wrongReviewer.Succeeded.ShouldBeFalse();
    }

    [Fact(DisplayName = "ChooseReviewersAsync rejects a reviewer list containing the record's creator")]
    public async Task ChooseReviewers_ContainingCreator_Rejected()
    {
        var seed = await SeedAsync();
        var service = NewService();
        var instanceId = (await service.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea })).WorkflowInstanceId!.Value;
        await ActAreaHeadSetujuAsync(service, seed, instanceId);

        var result = await service.ChooseReviewersAsync(
            instanceId, [seed.CreatorId, seed.Reviewer2UserId],
            seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });

        result.Succeeded.ShouldBeFalse();
    }

    [Fact(DisplayName = "Regional Admin setuju before reviewers are chosen is rejected")]
    public async Task RegionalAdminSetuju_BeforeReviewersChosen_Rejected()
    {
        var seed = await SeedAsync();
        var service = NewService();
        var instanceId = (await service.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea })).WorkflowInstanceId!.Value;
        await ActAreaHeadSetujuAsync(service, seed, instanceId);

        var regionalAdminStep = await StepAsync(instanceId, WorkflowStepKind.RegionalAdmin);
        var result = await service.ActAsync(
            regionalAdminStep.Id, WorkflowAction.Setuju, null,
            seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });

        result.Succeeded.ShouldBeFalse();
    }

    [Fact(DisplayName = "Full 2-reviewer chain reaches IssuedNol, skips Reviewer 3, completes the instance")]
    public async Task FullChain_TwoReviewers_ReachesIssuedNol()
    {
        var seed = await SeedAsync();
        var service = NewService();
        var instanceId = (await service.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea })).WorkflowInstanceId!.Value;
        await ActAreaHeadSetujuAsync(service, seed, instanceId);
        await service.ChooseReviewersAsync(
            instanceId, [seed.Reviewer1UserId, seed.Reviewer2UserId],
            seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });

        var regionalAdminStep = await StepAsync(instanceId, WorkflowStepKind.RegionalAdmin);
        await service.ActAsync(regionalAdminStep.Id, WorkflowAction.Setuju, null, seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });

        var reviewer1Step = await StepAsync(instanceId, WorkflowStepKind.Reviewer1);
        await service.ActAsync(reviewer1Step.Id, WorkflowAction.Setuju, null, seed.Reviewer1UserId, seed.ReviewerPermissions, new HashSet<Role> { Role.Reviewer });

        var reviewer2Step = await StepAsync(instanceId, WorkflowStepKind.Reviewer2);
        var afterReviewer2 = await service.ActAsync(reviewer2Step.Id, WorkflowAction.Setuju, null, seed.Reviewer2UserId, seed.ReviewerPermissions, new HashSet<Role> { Role.Reviewer });
        afterReviewer2.NewStatus.ShouldBe(RecordStatus.Approval); // 2-reviewer chain skips Reviewer3

        var divisionHeadStep = await StepAsync(instanceId, WorkflowStepKind.DivisionHead);
        var final = await service.ActAsync(divisionHeadStep.Id, WorkflowAction.Setuju, null, seed.DivisionHeadUserId, seed.DivisionHeadPermissions, new HashSet<Role> { Role.DivisionHead });

        final.Succeeded.ShouldBeTrue();
        final.NewStatus.ShouldBe(RecordStatus.IssuedNol);

        await using var verify = NewContext();
        var instance = await verify.WorkflowInstances.SingleAsync(i => i.Id == instanceId);
        instance.CompletedAt.ShouldNotBeNull();
        instance.FinalStatus.ShouldBe(RecordStatus.IssuedNol);

        (await verify.WorkflowSteps.AnyAsync(s => s.WorkflowInstanceId == instanceId && s.Kind == WorkflowStepKind.Reviewer3)).ShouldBeFalse();
    }

    [Fact(DisplayName = "Tolak from mid-chain jumps to Rejected regardless of position")]
    public async Task Tolak_MidChain_JumpsToRejected()
    {
        var seed = await SeedAsync();
        var service = NewService();
        var instanceId = (await service.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea })).WorkflowInstanceId!.Value;
        await ActAreaHeadSetujuAsync(service, seed, instanceId);
        await service.ChooseReviewersAsync(
            instanceId, [seed.Reviewer1UserId, seed.Reviewer2UserId],
            seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });

        var regionalAdminStep = await StepAsync(instanceId, WorkflowStepKind.RegionalAdmin);
        await service.ActAsync(regionalAdminStep.Id, WorkflowAction.Setuju, null, seed.RegionalAdminUserId, seed.RegionalAdminPermissions, new HashSet<Role> { Role.RegionalAdmin });

        var reviewer1Step = await StepAsync(instanceId, WorkflowStepKind.Reviewer1);
        await service.ActAsync(reviewer1Step.Id, WorkflowAction.Setuju, null, seed.Reviewer1UserId, seed.ReviewerPermissions, new HashSet<Role> { Role.Reviewer });

        var reviewer2Step = await StepAsync(instanceId, WorkflowStepKind.Reviewer2);
        var tolak = await service.ActAsync(reviewer2Step.Id, WorkflowAction.Tolak, "tidak lengkap", seed.Reviewer2UserId, seed.ReviewerPermissions, new HashSet<Role> { Role.Reviewer });

        tolak.Succeeded.ShouldBeTrue();
        tolak.NewStatus.ShouldBe(RecordStatus.Rejected);

        await using var verify = NewContext();
        var instance = await verify.WorkflowInstances.SingleAsync(i => i.Id == instanceId);
        instance.CompletedAt.ShouldNotBeNull();
        instance.FinalStatus.ShouldBe(RecordStatus.Rejected);
    }

    [Fact(DisplayName = "Self-approval is rejected: the creator cannot act on their own record")]
    public async Task SelfApproval_Rejected()
    {
        var seed = await SeedAsync();
        var service = NewService();
        var instanceId = (await service.StartAsync(seed.Company.Id, seed.CreatorId, seed.CreatorPermissions, new HashSet<Role> { Role.SalesArea })).WorkflowInstanceId!.Value;
        var areaHeadStep = await StepAsync(instanceId, WorkflowStepKind.AreaHead);

        // The creator also happens to hold Area Head — segregation of duties
        // must still block them.
        var result = await service.ActAsync(
            areaHeadStep.Id, WorkflowAction.Setuju, null,
            seed.CreatorId, seed.AreaHeadPermissions, new HashSet<Role> { Role.AreaHead });

        result.Succeeded.ShouldBeFalse();
    }

    private async Task ActAreaHeadSetujuAsync(WorkflowService service, SeedData seed, Guid instanceId)
    {
        var areaHeadStep = await StepAsync(instanceId, WorkflowStepKind.AreaHead);
        var result = await service.ActAsync(
            areaHeadStep.Id, WorkflowAction.Setuju, null,
            seed.AreaHeadUserId, seed.AreaHeadPermissions, new HashSet<Role> { Role.AreaHead });
        result.Succeeded.ShouldBeTrue();
    }

    private WorkflowService NewService() => new(
        new SingleContextFactory(_container.GetConnectionString()),
        new InAppNotificationChannel(new SingleContextFactory(_container.GetConnectionString())));

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
