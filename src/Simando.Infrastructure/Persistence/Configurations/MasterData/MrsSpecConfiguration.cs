using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.MasterData;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/domain/master-data.md §8: mrs_spec(id, name, created_at, deleted_at).
public sealed class MrsSpecConfiguration : AuditableEntityConfiguration<MrsSpec>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MrsSpec> builder)
    {
        builder.ToTable("mrs_spec");

        builder.Property(m => m.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(m => m.Name).IsUnique().HasFilter("deleted_at IS NULL");
    }
}
