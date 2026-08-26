using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Attachments;
using Simando.Domain.Directory;
using Simando.Domain.MasterData;
using Simando.Domain.Registration;

namespace Simando.Infrastructure.Persistence.Configurations;

public sealed class A1RegistrationConfiguration : IEntityTypeConfiguration<A1Registration>
{
    public void Configure(EntityTypeBuilder<A1Registration> builder)
    {
        builder.ToTable("a1_registration");

        builder.HasKey(a => a.CompanyId);
        builder.Property(a => a.CompanyId).HasColumnName("company_id");

        builder.Property(a => a.TanggalRegistrasi).HasColumnName("tanggal_registrasi");
        builder.Property(a => a.RegistrasiSource)
            .HasColumnName("registrasi_source")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.NamaPenanggungJawab).HasColumnName("nama_penanggung_jawab").HasMaxLength(200);
        builder.Property(a => a.Jabatan).HasColumnName("jabatan").HasMaxLength(200);

        builder.Property(a => a.BulanDimulai).HasColumnName("bulan_dimulai");
        builder.Property(a => a.BasisKontrak)
            .HasColumnName("basis_kontrak")
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.Property(a => a.SkemaHarga)
            .HasColumnName("skema_harga")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.SegmentId).HasColumnName("segment_id");
        builder.Property(a => a.KodeHarga).HasColumnName("kode_harga").HasMaxLength(50);

        builder.Property(a => a.HargaNilai).HasColumnName("harga_nilai").HasPrecision(18, 3);
        builder.Property(a => a.HargaCurrency)
            .HasColumnName("harga_currency")
            .HasConversion<string>()
            .HasMaxLength(10);
        builder.Property(a => a.HargaUnit)
            .HasColumnName("harga_unit")
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(a => a.CapexAwal).HasColumnName("capex_awal").HasPrecision(18, 3);
        builder.Property(a => a.MomSigasTersedia).HasColumnName("mom_sigas_tersedia");

        builder.Property(a => a.StatusBangunan)
            .HasColumnName("status_bangunan")
            .HasConversion<string>()
            .HasMaxLength(30);
        builder.Property(a => a.Sektor)
            .HasColumnName("sektor")
            .HasConversion<string>()
            .HasMaxLength(30);
        builder.Property(a => a.ProduksiUtama).HasColumnName("produksi_utama").HasMaxLength(200);

        builder.Property(a => a.JenisPeralatanGas).HasColumnName("jenis_peralatan_gas");
        builder.Property(a => a.TekananOperasiBarg).HasColumnName("tekanan_operasi_barg").HasPrecision(18, 3);

        builder.Property(a => a.SignedDocumentId).HasColumnName("signed_document_id");
        builder.Property(a => a.SignatureMethod)
            .HasColumnName("signature_method")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne<Company>()
            .WithOne()
            .HasForeignKey<A1Registration>(a => a.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Segment>()
            .WithMany()
            .HasForeignKey(a => a.SegmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Attachment>()
            .WithMany()
            .HasForeignKey(a => a.SignedDocumentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.UsagePeriods)
            .WithOne()
            .HasForeignKey(u => u.A1RegistrationCompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class A1UsagePeriodConfiguration : IEntityTypeConfiguration<A1UsagePeriod>
{
    public void Configure(EntityTypeBuilder<A1UsagePeriod> builder)
    {
        builder.ToTable("a1_usage_period");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.A1RegistrationCompanyId).HasColumnName("a1_registration_company_id");

        builder.Property(u => u.PeriodeMulai).HasColumnName("periode_mulai");
        builder.Property(u => u.PeriodeSelesai).HasColumnName("periode_selesai");
        builder.Property(u => u.RataRata).HasColumnName("rata_rata").HasPrecision(18, 3);
        builder.Property(u => u.Minimum).HasColumnName("minimum").HasPrecision(18, 3);
        builder.Property(u => u.Maksimum).HasColumnName("maksimum").HasPrecision(18, 3);
        builder.Property(u => u.SortOrder).HasColumnName("sort_order");
    }
}
