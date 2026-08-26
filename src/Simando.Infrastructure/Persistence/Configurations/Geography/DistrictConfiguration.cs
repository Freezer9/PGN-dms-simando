using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Geography;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/domain/master-data.md §4: district (kecamatan) — id, regency_id,
// bps_code, name, created_at, deleted_at.
public sealed class DistrictConfiguration : AuditableEntityConfiguration<District>
{
    protected override void ConfigureEntity(EntityTypeBuilder<District> builder)
    {
        builder.ToTable("district");

        builder.Property(d => d.RegencyId).HasColumnName("regency_id");

        builder.Property(d => d.BpsCode)
            .HasColumnName("bps_code")
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(d => d.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne<Regency>()
            .WithMany()
            .HasForeignKey(d => d.RegencyId)
            .OnDelete(DeleteBehavior.Restrict);

        // The docs don't state district code scope explicitly; following
        // the same "unique within parent" reading applied to Regency, since
        // Kemendagri codes repeat under different parents at every level.
        builder.HasIndex(d => new { d.RegencyId, d.BpsCode }).IsUnique().HasFilter("deleted_at IS NULL");
    }
}
