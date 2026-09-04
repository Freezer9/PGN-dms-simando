using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Simando.Domain.Attachments;
using Simando.Domain.Audit;
using Simando.Domain.Directory;
using Simando.Domain.Geography;
using Simando.Domain.MasterData;
using Simando.Domain.Nol;
using Simando.Domain.Notifications;
using Simando.Domain.Organisation;
using Simando.Domain.Registration;
using Simando.Domain.Security;
using Simando.Domain.Survey;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Identity;

namespace Simando.Infrastructure.Persistence;

// IdentityUserContext used because RBAC is RoleAssignment-driven (no IdentityRole).
//
// currentUser drives the Company/Plotting/CompanyContact row-level-security
// query filters below. It's injected as ICurrentUser, not the Web-layer
// CurrentUser class (Infrastructure can't reference Simando.Web) — see
// CurrentUser's own comment for why that class takes no DbContext
// dependency in return, avoiding a construction-order cycle.
public sealed class SimandoDbContext(DbContextOptions<SimandoDbContext> options, ICurrentUser currentUser)
    : IdentityUserContext<ApplicationUser, Guid>(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    public DbSet<Region> Regions => Set<Region>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<RoleAssignment> RoleAssignments => Set<RoleAssignment>();

    public DbSet<Province> Provinces => Set<Province>();
    public DbSet<Regency> Regencies => Set<Regency>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<Village> Villages => Set<Village>();

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<IndustryType> IndustryTypes => Set<IndustryType>();
    public DbSet<Segment> Segments => Set<Segment>();
    public DbSet<FuelType> FuelTypes => Set<FuelType>();
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();
    public DbSet<UnitSetMember> UnitSetMembers => Set<UnitSetMember>();
    public DbSet<MeterSize> MeterSizes => Set<MeterSize>();
    public DbSet<MrsSpec> MrsSpecs => Set<MrsSpec>();
    public DbSet<ReferenceDocument> ReferenceDocuments => Set<ReferenceDocument>();
    public DbSet<ReasonCategory> ReasonCategories => Set<ReasonCategory>();
    public DbSet<DocumentNumberCounter> DocumentNumberCounters => Set<DocumentNumberCounter>();

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Plotting> Plottings => Set<Plotting>();
    public DbSet<CompanyContact> CompanyContacts => Set<CompanyContact>();
    public DbSet<Survey> Surveys => Set<Survey>();
    public DbSet<SurveyProduct> SurveyProducts => Set<SurveyProduct>();
    public DbSet<SurveyRawMaterial> SurveyRawMaterials => Set<SurveyRawMaterial>();
    public DbSet<SurveyMarket> SurveyMarkets => Set<SurveyMarket>();
    public DbSet<SurveyEquipment> SurveyEquipment => Set<SurveyEquipment>();
    public DbSet<A1Registration> A1Registrations => Set<A1Registration>();
    public DbSet<A1UsagePeriod> A1UsagePeriods => Set<A1UsagePeriod>();
    public DbSet<NolRequest> NolRequests => Set<NolRequest>();
    public DbSet<NolRequestPeriod> NolRequestPeriods => Set<NolRequestPeriod>();
    public DbSet<NolRequestDaily> NolRequestDailies => Set<NolRequestDaily>();
    public DbSet<NolRequestReference> NolRequestReferences => Set<NolRequestReference>();
    public DbSet<NolEvaluation> NolEvaluations => Set<NolEvaluation>();
    public DbSet<NolEvaluationScenario> NolEvaluationScenarios => Set<NolEvaluationScenario>();
    public DbSet<EvaluationResume> EvaluationResumes => Set<EvaluationResume>();
    public DbSet<NolIssuance> NolIssuances => Set<NolIssuance>();
    public DbSet<NolIssuanceApprovedTerm> NolIssuanceApprovedTerms => Set<NolIssuanceApprovedTerm>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<StatusEvent> StatusEvents => Set<StatusEvent>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<BreakGlassAccess> BreakGlassAccesses => Set<BreakGlassAccess>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enable PostGIS extension up front for geography Point columns.
        modelBuilder.HasPostgresExtension("postgis");

        // Global, atomic, lock-free — see CompanyConfiguration and
        // docs/domain/03-directory-plotting.md#allocating-the-number.
        modelBuilder.HasSequence<int>("company_nomor_seq").StartsAt(1);

        modelBuilder.Entity<IdentityUserClaim<Guid>>(b => b.ToTable("user_claim"));
        modelBuilder.Entity<IdentityUserLogin<Guid>>(b => b.ToTable("user_login"));
        modelBuilder.Entity<IdentityUserToken<Guid>>(b => b.ToTable("user_token"));

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SimandoDbContext).Assembly);

        // Row-level security. docs/build/architecture.md#row-level-security.
        // Company carries the actual scope check; Plotting/CompanyContact
        // just require their parent Company to be visible — EF Core applies
        // an entity's own query filter automatically wherever that entity
        // is referenced via Set<T>(), including from inside another
        // entity's filter, so the scope logic lives in exactly one place.
        // (No navigation properties in this codebase — see AreaConfiguration
        // — so "Company.Area.RegionId" is a Set<Area>() subquery instead.)
        modelBuilder.Entity<Company>().HasQueryFilter(c =>
            c.DeletedAt == null &&
            ((currentUser.Scope == AccessScope.All && currentUser.HasCapability(Capability.ViewCompanyRecords)) ||
             (currentUser.Scope == AccessScope.Region &&
                Set<Area>().Any(a => a.Id == c.AreaId && a.RegionId == currentUser.RegionId)) ||
             (currentUser.Scope == AccessScope.Area && c.AreaId == currentUser.AreaId)));

        modelBuilder.Entity<Plotting>().HasQueryFilter(p =>
            Set<Company>().Any(c => c.Id == p.CompanyId));

        modelBuilder.Entity<CompanyContact>().HasQueryFilter(cc =>
            Set<Company>().Any(c => c.Id == cc.CompanyId));

        modelBuilder.Entity<Survey>().HasQueryFilter(s =>
            Set<Company>().Any(c => c.Id == s.CompanyId));

        modelBuilder.Entity<SurveyProduct>().HasQueryFilter(p =>
            Set<Company>().Any(c => c.Id == p.CompanyId));

        modelBuilder.Entity<SurveyRawMaterial>().HasQueryFilter(m =>
            Set<Company>().Any(c => c.Id == m.CompanyId));

        modelBuilder.Entity<SurveyMarket>().HasQueryFilter(m =>
            Set<Company>().Any(c => c.Id == m.CompanyId));

        modelBuilder.Entity<SurveyEquipment>().HasQueryFilter(e =>
            Set<Company>().Any(c => c.Id == e.CompanyId));

        modelBuilder.Entity<A1Registration>().HasQueryFilter(a =>
            Set<Company>().Any(c => c.Id == a.CompanyId));

        modelBuilder.Entity<NolRequest>().HasQueryFilter(n =>
            Set<Company>().Any(c => c.Id == n.CompanyId));

        modelBuilder.Entity<NolEvaluation>().HasQueryFilter(e =>
            Set<Company>().Any(c => c.Id == e.NolRequestId));

        modelBuilder.Entity<NolIssuance>().HasQueryFilter(i =>
            Set<Company>().Any(c => c.Id == i.NolRequestId));

        modelBuilder.Entity<Attachment>().HasQueryFilter(a =>
            a.CompanyId == null || Set<Company>().Any(c => c.Id == a.CompanyId));

        modelBuilder.Entity<StatusEvent>().HasQueryFilter(e =>
            Set<Company>().Any(c => c.Id == e.CompanyId));

        modelBuilder.Entity<WorkflowInstance>().HasQueryFilter(i =>
            Set<Company>().Any(c => c.Id == i.CompanyId));

        modelBuilder.Entity<WorkflowStep>().HasQueryFilter(s =>
            Set<WorkflowInstance>().Any(i => i.Id == s.WorkflowInstanceId));

        modelBuilder.Entity<Notification>().HasQueryFilter(n =>
            Set<Company>().Any(c => c.Id == n.CompanyId));
    }
}
