using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Common;

namespace Simando.Infrastructure.Persistence.Configurations;

// Maps AuditableEntity's shared Id/CreatedAt/DeletedAt columns and applies
// the soft-delete query filter once, so concrete configs only need to
// handle ToTable and their own entity-specific columns/indexes.
public abstract class AuditableEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : AuditableEntity
{
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        // Retired rows stay in the table (referenced by historical records)
        // but drop out of every query by default.
        builder.HasQueryFilter(e => e.DeletedAt == null);

        ConfigureEntity(builder);
    }

    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
}
