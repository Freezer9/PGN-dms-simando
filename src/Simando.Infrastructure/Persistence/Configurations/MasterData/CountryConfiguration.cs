using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.MasterData;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/domain/master-data.md §4 "Negara": country(id, iso_code, name, created_at, deleted_at).
public sealed class CountryConfiguration : AuditableEntityConfiguration<Country>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("country");

        // ISO 3166-1 alpha-2, e.g. 'ID'.
        builder.Property(c => c.IsoCode)
            .HasColumnName("iso_code")
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        // Partial index — a retired row must not block reusing its code for
        // a genuinely new one.
        builder.HasIndex(c => c.IsoCode).IsUnique().HasFilter("deleted_at IS NULL");
    }
}
