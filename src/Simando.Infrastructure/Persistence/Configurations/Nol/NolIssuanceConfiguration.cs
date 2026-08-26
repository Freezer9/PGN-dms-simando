using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Attachments;
using Simando.Domain.Nol;
using Simando.Infrastructure.Identity;

namespace Simando.Infrastructure.Persistence.Configurations;

public sealed class NolIssuanceConfiguration : IEntityTypeConfiguration<NolIssuance>
{
    public void Configure(EntityTypeBuilder<NolIssuance> builder)
    {
        builder.ToTable("nol_issuance");

        builder.HasKey(i => i.NolRequestId);
        builder.Property(i => i.NolRequestId).HasColumnName("nol_request_id");

        builder.Property(i => i.Outcome)
            .HasColumnName("outcome")
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.Property(i => i.NomorNotaDinas).HasColumnName("nomor_nota_dinas").HasMaxLength(100);

        builder.Property(i => i.KontrakBersyarat)
            .HasColumnName("kontrak_bersyarat");

        builder.Property(i => i.BerlakuSejak).HasColumnName("berlaku_sejak");
        builder.Property(i => i.BerlakuSampai).HasColumnName("berlaku_sampai");

        builder.Property(i => i.SignedByUserId).HasColumnName("signed_by_user_id");
        builder.Property(i => i.SignedAt).HasColumnName("signed_at");

        builder.Property(i => i.DocumentId).HasColumnName("document_id");

        builder.HasOne<NolRequest>()
            .WithOne()
            .HasForeignKey<NolIssuance>(i => i.NolRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(i => i.SignedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Attachment>()
            .WithMany()
            .HasForeignKey(i => i.DocumentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.ApprovedTerms)
            .WithOne()
            .HasForeignKey(a => a.NolIssuanceNolRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class NolIssuanceApprovedTermConfiguration : IEntityTypeConfiguration<NolIssuanceApprovedTerm>
{
    public void Configure(EntityTypeBuilder<NolIssuanceApprovedTerm> builder)
    {
        builder.ToTable("nol_issuance_approved_term");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.NolIssuanceNolRequestId).HasColumnName("nol_issuance_nol_request_id");

        builder.Property(a => a.PeriodeMulai).HasColumnName("periode_mulai");
        builder.Property(a => a.PeriodeSelesai).HasColumnName("periode_selesai");
        builder.Property(a => a.RataRata).HasColumnName("rata_rata").HasPrecision(18, 3);
        builder.Property(a => a.KontrakMinimum).HasColumnName("kontrak_minimum").HasPrecision(18, 3);
        builder.Property(a => a.KontrakMaksimum).HasColumnName("kontrak_maksimum").HasPrecision(18, 3);
        builder.Property(a => a.SortOrder).HasColumnName("sort_order");
    }
}
