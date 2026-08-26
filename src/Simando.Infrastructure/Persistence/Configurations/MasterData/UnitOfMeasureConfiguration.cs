using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.MasterData;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/domain/master-data.md §7: unit_of_measure(id, code, name, dimension, created_at, deleted_at).
public sealed class UnitOfMeasureConfiguration : AuditableEntityConfiguration<UnitOfMeasure>
{
    protected override void ConfigureEntity(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.ToTable("unit_of_measure");

        builder.Property(u => u.Code)
            .HasColumnName("code")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(u => u.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Dimension)
            .HasColumnName("dimension")
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(u => u.Code).IsUnique().HasFilter("deleted_at IS NULL");
    }
}
