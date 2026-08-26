using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.MasterData;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/domain/master-data.md §8: meter_size(id, g_size, nominal_flow,
// max_flow, pressure_rating, created_at, deleted_at).
public sealed class MeterSizeConfiguration : AuditableEntityConfiguration<MeterSize>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MeterSize> builder)
    {
        builder.ToTable("meter_size");

        // 'G65'.
        builder.Property(m => m.GSize)
            .HasColumnName("g_size")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(m => m.NominalFlow)
            .HasColumnName("nominal_flow")
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(m => m.MaxFlow)
            .HasColumnName("max_flow")
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(m => m.PressureRating)
            .HasColumnName("pressure_rating")
            .HasPrecision(18, 3)
            .IsRequired();

        builder.HasIndex(m => m.GSize).IsUnique().HasFilter("deleted_at IS NULL");
    }
}
