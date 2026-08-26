using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.MasterData;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/domain/master-data.md §7: unit_set_member(id, set_code, unit_id, sort_order).
public sealed class UnitSetMemberConfiguration : IEntityTypeConfiguration<UnitSetMember>
{
    public void Configure(EntityTypeBuilder<UnitSetMember> builder)
    {
        builder.ToTable("unit_set_member");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).HasColumnName("id");

        builder.Property(m => m.SetCode)
            .HasColumnName("set_code")
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(m => m.UnitId).HasColumnName("unit_id");

        builder.Property(m => m.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder.HasOne<UnitOfMeasure>()
            .WithMany()
            .HasForeignKey(m => m.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        // A unit shouldn't appear twice in the same set's dropdown.
        builder.HasIndex(m => new { m.SetCode, m.UnitId }).IsUnique();
    }
}
