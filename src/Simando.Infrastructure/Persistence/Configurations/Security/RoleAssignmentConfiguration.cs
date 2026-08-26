using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Infrastructure.Identity;

namespace Simando.Infrastructure.Persistence.Configurations;

// RoleAssignment entity configuration per docs/design/roles-permissions.md §4.
public sealed class RoleAssignmentConfiguration : IEntityTypeConfiguration<RoleAssignment>
{
    public void Configure(EntityTypeBuilder<RoleAssignment> builder)
    {
        builder.ToTable("user_role_assignment");

        builder.HasKey(ra => ra.Id);
        builder.Property(ra => ra.Id).HasColumnName("id");

        builder.Property(ra => ra.UserId).HasColumnName("user_id");

        // Enum string conversion for database readability and ordering safety.
        builder.Property(ra => ra.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(ra => ra.AreaId).HasColumnName("area_id");
        builder.Property(ra => ra.RegionId).HasColumnName("region_id");
        builder.Property(ra => ra.Active).HasColumnName("active").IsRequired();
        builder.Property(ra => ra.AssignedBy).HasColumnName("assigned_by");
        builder.Property(ra => ra.AssignedAt).HasColumnName("assigned_at");

        // FK-only restrict relationships per docs/design/roles-permissions.md.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(ra => ra.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Area>()
            .WithMany()
            .HasForeignKey(ra => ra.AreaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Region>()
            .WithMany()
            .HasForeignKey(ra => ra.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Fast lookup index for PermissionEvaluator.Resolve.
        builder.HasIndex(ra => ra.UserId);
    }
}
