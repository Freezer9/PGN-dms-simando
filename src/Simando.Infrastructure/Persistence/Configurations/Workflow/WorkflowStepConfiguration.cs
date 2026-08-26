using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Identity;

namespace Simando.Infrastructure.Persistence.Configurations;

public sealed class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
{
    public void Configure(EntityTypeBuilder<WorkflowStep> builder)
    {
        builder.ToTable("workflow_step");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.WorkflowInstanceId).HasColumnName("workflow_instance_id");
        builder.Property(s => s.StepOrder).HasColumnName("step_order").IsRequired();

        builder.Property(s => s.Kind)
            .HasColumnName("kind")
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.AssignedUserId).HasColumnName("assigned_user_id");
        builder.Property(s => s.ActedAt).HasColumnName("acted_at");
        builder.Property(s => s.ActedBy).HasColumnName("acted_by");

        builder.Property(s => s.Action)
            .HasColumnName("action")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.Comment).HasColumnName("comment");

        builder.HasOne<WorkflowInstance>()
            .WithMany()
            .HasForeignKey(s => s.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(s => s.AssignedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(s => s.ActedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.WorkflowInstanceId);
    }
}
