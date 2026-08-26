using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Organisation;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/domain/master-data.md §3 "Organisation & scope": region(id, code, name, active).
public sealed class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder.ToTable("region");

        builder.HasKey(r => r.Id);

        // Column names spelled out explicitly as snake_case — the schema
        // throughout docs/design/data-model.md and
        // docs/domain/master-data.md is snake_case, and EF Core's Npgsql
        // provider does not derive that from PascalCase C# property names
        // on its own.
        builder.Property(r => r.Id).HasColumnName("id");

        builder.Property(r => r.Code)
            .HasColumnName("code")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Active)
            .HasColumnName("active")
            .IsRequired();

        // 'SOR-II' etc. read as a natural key — regions are a short, curated
        // list (SOR I-IV) so global uniqueness is unambiguous, unlike Area's
        // code (see AreaConfiguration).
        builder.HasIndex(r => r.Code).IsUnique();
    }
}
