using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.MasterData;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/domain/master-data.md §9: reference_document(id, name, version,
// effective_from, effective_to, blob_key).
public sealed class ReferenceDocumentConfiguration : IEntityTypeConfiguration<ReferenceDocument>
{
    public void Configure(EntityTypeBuilder<ReferenceDocument> builder)
    {
        builder.ToTable("reference_document");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).HasColumnName("id");

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Version)
            .HasColumnName("version")
            .IsRequired();

        builder.Property(r => r.EffectiveFrom)
            .HasColumnName("effective_from")
            .IsRequired();

        builder.Property(r => r.EffectiveTo)
            .HasColumnName("effective_to");

        builder.Property(r => r.BlobKey)
            .HasColumnName("blob_key")
            .HasMaxLength(500);

        // A given document name's versions must not overlap in time —
        // "currently in force" per name is meant to be unambiguous.
        builder.HasIndex(r => new { r.Name, r.Version }).IsUnique();
    }
}
