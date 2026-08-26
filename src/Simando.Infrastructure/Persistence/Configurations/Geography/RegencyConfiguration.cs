using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Geography;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/domain/master-data.md §4: regency(id, province_id, bps_code, type,
// name, created_at, deleted_at) — "regency" covers both Kota and Kabupaten.
public sealed class RegencyConfiguration : AuditableEntityConfiguration<Regency>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Regency> builder)
    {
        builder.ToTable("regency");

        builder.Property(r => r.ProvinceId).HasColumnName("province_id");

        builder.Property(r => r.BpsCode)
            .HasColumnName("bps_code")
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(r => r.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        // Restrict, not cascade — reorganisations retire rows, they
        // never delete them (Nomor embeds bps codes and must not shift
        // under an already-issued document).
        builder.HasOne<Province>()
            .WithMany()
            .HasForeignKey(r => r.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);

        // "The level-2 code is only unique within its province" —
        // docs/domain/master-data.md §4.
        builder.HasIndex(r => new { r.ProvinceId, r.BpsCode }).IsUnique().HasFilter("deleted_at IS NULL");
    }
}
