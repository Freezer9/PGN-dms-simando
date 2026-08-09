using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Pgn.Dms.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubmissionRecord> SubmissionRecords => Set<SubmissionRecord>();
    public DbSet<ReviewStep> ReviewSteps => Set<ReviewStep>();
    public DbSet<ResumeEvaluasi> ResumeEvaluasi => Set<ResumeEvaluasi>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

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
    public DbSet<NolRequestReference> NolRequestReferences => Set<NolRequestReference>();
    public DbSet<NolEvaluation> NolEvaluations => Set<NolEvaluation>();
    public DbSet<NolEvaluationScenario> NolEvaluationScenarios => Set<NolEvaluationScenario>();
    public DbSet<NolIssuance> NolIssuances => Set<NolIssuance>();
    public DbSet<NolIssuanceTerm> NolIssuanceTerms => Set<NolIssuanceTerm>();
    public DbSet<NolIssuanceCondition> NolIssuanceConditions => Set<NolIssuanceCondition>();
    public DbSet<MasterDataEntry> MasterData => Set<MasterDataEntry>();
    public DbSet<BreakGlassGrant> BreakGlassGrants => Set<BreakGlassGrant>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Region>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasMany(r => r.Areas).WithOne(a => a.Region).HasForeignKey(a => a.RegionId);
        });

        builder.Entity<Area>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasMany(a => a.Subscriptions).WithOne(s => s.Area).HasForeignKey(s => s.AreaId);
        });

        builder.Entity<Subscription>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasOne(s => s.CreatedBy).WithMany().HasForeignKey(s => s.CreatedById).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(s => s.Submissions).WithOne(r => r.Subscription).HasForeignKey(r => r.SubscriptionId);
            e.HasMany(s => s.ReviewSteps).WithOne(r => r.Subscription).HasForeignKey(r => r.SubscriptionId);
            e.HasMany(s => s.ActivityLogs).WithOne(a => a.Subscription).HasForeignKey(a => a.SubscriptionId);
            e.HasOne(s => s.ResumeEvaluasi).WithOne(r => r.Subscription).HasForeignKey<ResumeEvaluasi>(r => r.SubscriptionId);
        });

        builder.Entity<SubmissionRecord>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasOne(r => r.UploadedBy).WithMany().HasForeignKey(r => r.UploadedById).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ReviewStep>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasOne(r => r.Reviewer).WithMany().HasForeignKey(r => r.ReviewerId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ResumeEvaluasi>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasOne(r => r.CreatedBy).WithMany().HasForeignKey(r => r.CreatedById).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ActivityLog>(e => e.HasKey(a => a.Id));

        builder.Entity<ApplicationUser>(e =>
        {
            e.HasOne(u => u.Area).WithMany().HasForeignKey(u => u.AreaId);
        });

        ConfigureStages(builder);
    }

    // Stage satellite tables. Each is 1:1 or 1:N off Subscription; children cascade from
    // their parent, and every ApplicationUser FK is Restrict so users stay deletable-blocked
    // rather than silently orphaning records — same rule as the core entities above.
    private static void ConfigureStages(ModelBuilder builder)
    {
        builder.Entity<Plotting>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.SubscriptionId).IsUnique();
            e.HasOne(p => p.Subscription).WithOne().HasForeignKey<Plotting>(p => p.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.SalesUser).WithMany().HasForeignKey(p => p.SalesUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CompanyContact>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.SubscriptionId);
            e.HasOne(c => c.Subscription).WithMany().HasForeignKey(c => c.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Survey>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasIndex(s => s.SubscriptionId).IsUnique();
            e.HasOne(s => s.Subscription).WithOne().HasForeignKey<Survey>(s => s.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.SurveyorUser).WithMany().HasForeignKey(s => s.SurveyorUserId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasMany(s => s.Products).WithOne(p => p.Survey).HasForeignKey(p => p.SurveyId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(s => s.RawMaterials).WithOne(r => r.Survey).HasForeignKey(r => r.SurveyId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(s => s.Markets).WithOne(m => m.Survey).HasForeignKey(m => m.SurveyId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(s => s.Equipment).WithOne(q => q.Survey).HasForeignKey(q => q.SurveyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SurveyProduct>(e => e.HasKey(p => p.Id));
        builder.Entity<SurveyRawMaterial>(e => e.HasKey(r => r.Id));
        builder.Entity<SurveyMarket>(e => e.HasKey(m => m.Id));
        builder.Entity<SurveyEquipment>(e => e.HasKey(q => q.Id));

        builder.Entity<A1Registration>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.SubscriptionId).IsUnique();
            e.HasOne(a => a.Subscription).WithOne().HasForeignKey<A1Registration>(a => a.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Segment).WithMany().HasForeignKey(a => a.SegmentId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasMany(a => a.Periods).WithOne(p => p.A1Registration).HasForeignKey(p => p.A1RegistrationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<A1UsagePeriod>(e => e.HasKey(p => p.Id));

        builder.Entity<NolRequest>(e =>
        {
            e.HasKey(n => n.Id);
            e.HasIndex(n => n.SubscriptionId).IsUnique();
            e.HasOne(n => n.Subscription).WithOne().HasForeignKey<NolRequest>(n => n.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(n => n.Segment).WithMany().HasForeignKey(n => n.SegmentId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasMany(n => n.Periods).WithOne(p => p.NolRequest).HasForeignKey(p => p.NolRequestId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(n => n.References).WithOne(r => r.NolRequest).HasForeignKey(r => r.NolRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<NolRequestPeriod>(e => e.HasKey(p => p.Id));
        builder.Entity<NolRequestReference>(e => e.HasKey(r => r.Id));

        builder.Entity<NolEvaluation>(e =>
        {
            e.HasKey(n => n.Id);
            e.HasIndex(n => n.SubscriptionId).IsUnique();
            e.HasOne(n => n.Subscription).WithOne().HasForeignKey<NolEvaluation>(n => n.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(n => n.MrsSpec).WithMany().HasForeignKey(n => n.MrsSpecId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(n => n.MeterSize).WithMany().HasForeignKey(n => n.MeterSizeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(n => n.EvaluatedBy).WithMany().HasForeignKey(n => n.EvaluatedById)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasMany(n => n.Scenarios).WithOne(s => s.NolEvaluation).HasForeignKey(s => s.NolEvaluationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<NolEvaluationScenario>(e => e.HasKey(s => s.Id));

        builder.Entity<NolIssuance>(e =>
        {
            e.HasKey(n => n.Id);
            e.HasIndex(n => n.SubscriptionId).IsUnique();
            e.HasOne(n => n.Subscription).WithOne().HasForeignKey<NolIssuance>(n => n.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(n => n.SignedBy).WithMany().HasForeignKey(n => n.SignedById)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasMany(n => n.ApprovedTerms).WithOne(t => t.NolIssuance).HasForeignKey(t => t.NolIssuanceId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(n => n.Conditions).WithOne(c => c.NolIssuance).HasForeignKey(c => c.NolIssuanceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<NolIssuanceTerm>(e => e.HasKey(t => t.Id));
        builder.Entity<NolIssuanceCondition>(e => e.HasKey(c => c.Id));

        builder.Entity<MasterDataEntry>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasIndex(m => new { m.Category, m.SortOrder });
        });

        builder.Entity<BreakGlassGrant>(e =>
        {
            e.HasKey(g => g.Id);
            e.HasIndex(g => g.ExpiresAt);
            e.HasOne(g => g.Subscription).WithMany().HasForeignKey(g => g.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(g => g.GrantedToUser).WithMany().HasForeignKey(g => g.GrantedToUserId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(g => g.GrantedBy).WithMany().HasForeignKey(g => g.GrantedById)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
