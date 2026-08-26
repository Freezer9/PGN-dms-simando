using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Directory;
using Simando.Domain.Workflow;

namespace Simando.Infrastructure.Persistence.Configurations;

public sealed class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.ToTable("workflow_instance");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.CompanyId).HasColumnName("company_id");

        builder.Property(i => i.ReviewerCount)
            .HasColumnName("reviewer_count")
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(i => i.StartedAt).HasColumnName("started_at").IsRequired();
        builder.Property(i => i.CompletedAt).HasColumnName("completed_at");

        builder.Property(i => i.FinalStatus)
            .HasColumnName("final_status")
            .HasConversion<string>()
            .HasMaxLength(20);

        // Restrict — same reasoning as StatusEvent: an instance's history
        // must never be silently swept away by a parent delete.
        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(i => i.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.CompanyId);
    }
}
