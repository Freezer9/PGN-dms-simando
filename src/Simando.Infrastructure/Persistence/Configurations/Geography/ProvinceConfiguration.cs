using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Geography;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/domain/master-data.md §4: province(id, bps_code, name, created_at, deleted_at).
public sealed class ProvinceConfiguration : AuditableEntityConfiguration<Province>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Province> builder)
    {
        builder.ToTable("province");

        // '35' — Kemendagri codes are globally unique at province level
        // (unlike regency/district/kelurahan, which repeat per parent).
        builder.Property(p => p.BpsCode)
            .HasColumnName("bps_code")
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(p => p.BpsCode).IsUnique().HasFilter("deleted_at IS NULL");
    }
}
