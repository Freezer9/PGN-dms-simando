using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Geography;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/domain/master-data.md §4: village (kelurahan/desa) — id,
// district_id, bps_code, type, name, created_at, deleted_at.
public sealed class VillageConfiguration : AuditableEntityConfiguration<Village>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Village> builder)
    {
        builder.ToTable("village");

        builder.Property(v => v.DistrictId).HasColumnName("district_id");

        builder.Property(v => v.BpsCode)
            .HasColumnName("bps_code")
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(v => v.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(v => v.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne<District>()
            .WithMany()
            .HasForeignKey(v => v.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => new { v.DistrictId, v.BpsCode }).IsUnique().HasFilter("deleted_at IS NULL");
    }
}
