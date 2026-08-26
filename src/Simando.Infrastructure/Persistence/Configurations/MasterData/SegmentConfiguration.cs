using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.MasterData;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/domain/master-data.md §6: segment(id, name, sort_order, created_at, deleted_at).
public sealed class SegmentConfiguration : AuditableEntityConfiguration<Segment>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Segment> builder)
    {
        builder.ToTable("segment");

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(50);

        // Bronze 1 .. Platinum has a fixed tier order that name-sorting
        // can't reproduce.
        builder.Property(s => s.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder.HasIndex(s => s.Name).IsUnique().HasFilter("deleted_at IS NULL");
    }
}
