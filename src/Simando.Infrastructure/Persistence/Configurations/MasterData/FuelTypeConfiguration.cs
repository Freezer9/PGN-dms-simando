using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.MasterData;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/domain/master-data.md §7: fuel_type(id, name, created_at, deleted_at).
public sealed class FuelTypeConfiguration : AuditableEntityConfiguration<FuelType>
{
    protected override void ConfigureEntity(EntityTypeBuilder<FuelType> builder)
    {
        builder.ToTable("fuel_type");

        builder.Property(f => f.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(f => f.Name).IsUnique().HasFilter("deleted_at IS NULL");
    }
}
