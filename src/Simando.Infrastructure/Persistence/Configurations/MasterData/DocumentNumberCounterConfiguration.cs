using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.MasterData;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/domain/master-data.md §9: document_number_counter(document_type,
// scope_key, period_key, next_seq) — composite PK, no surrogate id.
public sealed class DocumentNumberCounterConfiguration : IEntityTypeConfiguration<DocumentNumberCounter>
{
    public void Configure(EntityTypeBuilder<DocumentNumberCounter> builder)
    {
        builder.ToTable("document_number_counter");

        builder.HasKey(c => new { c.DocumentType, c.ScopeKey, c.PeriodKey });

        builder.Property(c => c.DocumentType)
            .HasColumnName("document_type")
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(c => c.ScopeKey)
            .HasColumnName("scope_key")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.PeriodKey)
            .HasColumnName("period_key")
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(c => c.NextSeq)
            .HasColumnName("next_seq")
            .IsRequired();
    }
}
