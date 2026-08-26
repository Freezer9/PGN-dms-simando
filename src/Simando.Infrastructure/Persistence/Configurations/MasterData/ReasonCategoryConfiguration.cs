using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.MasterData;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/domain/master-data.md §10: reason_category(id, name, created_at, deleted_at).
public sealed class ReasonCategoryConfiguration : AuditableEntityConfiguration<ReasonCategory>
{
    protected override void ConfigureEntity(EntityTypeBuilder<ReasonCategory> builder)
    {
        builder.ToTable("reason_category");

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(r => r.Name).IsUnique().HasFilter("deleted_at IS NULL");
    }
}
