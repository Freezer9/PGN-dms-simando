using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Attachments;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/design/data-model.md#attachment — polymorphic, one table for all
// upload points. StorageProvider and AttachmentKind stored as snake_case
// strings so adding enum values doesn't require a migration.
public sealed class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("attachment");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.CompanyId).HasColumnName("company_id");

        builder.Property(a => a.AttachableType)
            .HasColumnName("attachable_type")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.AttachableId).HasColumnName("attachable_id");

        builder.Property(a => a.Kind)
            .HasColumnName("kind")
            .IsRequired()
            .HasConversion(
                k => k.ToString().ToLowerInvariant(),
                s => Enum.Parse<AttachmentKind>(s, ignoreCase: true));

        builder.Property(a => a.Filename)
            .HasColumnName("filename")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.MimeType)
            .HasColumnName("mime_type")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.SizeBytes).HasColumnName("size_bytes");

        builder.Property(a => a.Checksum)
            .HasColumnName("checksum")
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(a => a.StorageProvider)
            .HasColumnName("storage_provider")
            .IsRequired()
            .HasConversion(
                p => p.ToString().ToLowerInvariant(),
                s => Enum.Parse<StorageProvider>(s, ignoreCase: true));

        builder.Property(a => a.StorageKey)
            .HasColumnName("storage_key")
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(a => a.UploadedBy).HasColumnName("uploaded_by");
        builder.Property(a => a.UploadedAt).HasColumnName("uploaded_at");
        builder.Property(a => a.Version).HasColumnName("version");

        // Fast lookup when resolving which attachment slot an entity has.
        builder.HasIndex(a => new { a.AttachableType, a.AttachableId });
        // Fast lookup for download by company scope.
        builder.HasIndex(a => a.CompanyId);
    }
}
