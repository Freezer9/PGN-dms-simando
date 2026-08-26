using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Audit;
using Simando.Domain.Directory;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Identity;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/design/data-model.md#status_event--the-visibility-engine.
public sealed class StatusEventConfiguration : IEntityTypeConfiguration<StatusEvent>
{
    public void Configure(EntityTypeBuilder<StatusEvent> builder)
    {
        builder.ToTable("status_event", t => t.HasCheckConstraint(
            "ck_status_event_comment_required_on_revisi_tolak",
            """(action not in ('Revisi', 'Tolak')) or (comment is not null)"""));

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.CompanyId).HasColumnName("company_id");

        builder.Property(e => e.FromStage).HasColumnName("from_stage");
        builder.Property(e => e.ToStage).HasColumnName("to_stage").IsRequired();

        builder.Property(e => e.FromStatus)
            .HasColumnName("from_status")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.ToStatus)
            .HasColumnName("to_status")
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.ActorId).HasColumnName("actor_id");

        builder.Property(e => e.Action)
            .HasColumnName("action")
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.Comment).HasColumnName("comment");

        builder.Property(e => e.OccurredAt).HasColumnName("occurred_at").IsRequired();

        builder.Property(e => e.WorkflowStepId).HasColumnName("workflow_step_id");

        // Restrict — an audit trail row must never be silently swept away by
        // a parent delete. Company is soft-delete only, so this never fires
        // in practice, but Restrict documents the intent regardless.
        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.ActorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<WorkflowStep>()
            .WithMany()
            .HasForeignKey(e => e.WorkflowStepId)
            .OnDelete(DeleteBehavior.Restrict);

        // The timeline query: all events for a company, in order.
        builder.HasIndex(e => new { e.CompanyId, e.OccurredAt });
    }
}
