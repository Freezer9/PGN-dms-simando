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
    }
}
