using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Organisation;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/domain/master-data.md §3: area(id, region_id, code, name, active).
public sealed class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.ToTable("area");

        builder.HasKey(a => a.Id);

        // See RegionConfiguration — column names spelled out as snake_case
        // to match docs/domain/master-data.md §3's schema.
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.RegionId).HasColumnName("region_id");

        builder.Property(a => a.Code)
            .HasColumnName("code")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Active)
            .HasColumnName("active")
            .IsRequired();

        // FK-only relationship — Area has no navigation property to Region
        // (Simando.Domain keeps neither entity carrying navigation
        // collections). Restrict, not cascade: "deleting an
        // Area that holds records is blocked; deactivate instead" extends
        // to the Region/Area relationship itself — nothing here should
        // silently cascade-delete.
        builder.HasOne<Region>()
            .WithMany()
            .HasForeignKey(a => a.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        // The docs don't state whether Area.Code is globally unique or only
        // within its Region — 'SBY' for Surabaya reads like a natural key,
        // but nothing rules out the same short code reused across Regions.
        // Composite uniqueness is the conservative reading, called out here
        // as an inference rather than a sourced fact.
        builder.HasIndex(a => new { a.RegionId, a.Code }).IsUnique();
    }
}
