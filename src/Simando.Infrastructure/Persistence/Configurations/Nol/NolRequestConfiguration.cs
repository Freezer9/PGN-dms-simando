using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Directory;
using Simando.Domain.MasterData;
using Simando.Domain.Nol;
using Simando.Domain.Workflow;

namespace Simando.Infrastructure.Persistence.Configurations;

public sealed class NolRequestConfiguration : IEntityTypeConfiguration<NolRequest>
{
    public void Configure(EntityTypeBuilder<NolRequest> builder)
    {
        builder.ToTable("nol_request");

        builder.HasKey(n => n.CompanyId);
        builder.Property(n => n.CompanyId).HasColumnName("company_id");

        builder.Property(n => n.NomorNotaDinas).HasColumnName("nomor_nota_dinas").HasMaxLength(100);
        builder.Property(n => n.RegistrationType)
            .HasColumnName("registration_type")
            .HasConversion<string>()
            .HasMaxLength(30);
        builder.Property(n => n.SamaDenganA1).HasColumnName("sama_dengan_a1");

        builder.Property(n => n.BulanDimulai).HasColumnName("bulan_dimulai");
        builder.Property(n => n.BasisKontrak)
            .HasColumnName("basis_kontrak")
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.Property(n => n.SkemaHarga)
            .HasColumnName("skema_harga")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(n => n.SegmentId).HasColumnName("segment_id");
        builder.Property(n => n.KodeHarga).HasColumnName("kode_harga").HasMaxLength(50);
        builder.Property(n => n.HargaNilai).HasColumnName("harga_nilai").HasPrecision(18, 3);
        builder.Property(n => n.HargaCurrency)
            .HasColumnName("harga_currency")
            .HasConversion<string>()
            .HasMaxLength(10);
        builder.Property(n => n.HargaUnit)
            .HasColumnName("harga_unit")
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(n => n.AlasanKontrakBersyarat).HasColumnName("alasan_kontrak_bersyarat");
        builder.Property(n => n.NamaPimpinanPerusahaan).HasColumnName("nama_pimpinan_perusahaan").HasMaxLength(200);
        builder.Property(n => n.JangkaWaktuKontrak).HasColumnName("jangka_waktu_kontrak").HasMaxLength(100);

        builder.Property(n => n.CapexPreGr3).HasColumnName("capex_pre_gr3").HasPrecision(18, 3);
        builder.Property(n => n.BiayaPenyambunganReguler).HasColumnName("biaya_penyambungan_reguler").HasPrecision(18, 3);
        builder.Property(n => n.BiayaPenyambunganExtra).HasColumnName("biaya_penyambungan_extra").HasPrecision(18, 3);

        builder.Property(n => n.WorkflowInstanceId).HasColumnName("workflow_instance_id");
        builder.Property(n => n.SubmittedAt).HasColumnName("submitted_at");

        builder.HasOne<Company>()
            .WithOne()
            .HasForeignKey<NolRequest>(n => n.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Segment>()
            .WithMany()
            .HasForeignKey(n => n.SegmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<WorkflowInstance>()
            .WithMany()
            .HasForeignKey(n => n.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(n => n.Periods)
            .WithOne()
            .HasForeignKey(p => p.NolRequestCompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(n => n.DailyBasisRows)
            .WithOne()
            .HasForeignKey(d => d.NolRequestCompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(n => n.References)
            .WithOne()
            .HasForeignKey(r => r.NolRequestCompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class NolRequestPeriodConfiguration : IEntityTypeConfiguration<NolRequestPeriod>
{
    public void Configure(EntityTypeBuilder<NolRequestPeriod> builder)
    {
        builder.ToTable("nol_request_period");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.NolRequestCompanyId).HasColumnName("nol_request_company_id");

        builder.Property(p => p.PeriodeMulai).HasColumnName("periode_mulai");
        builder.Property(p => p.PeriodeSelesai).HasColumnName("periode_selesai");
        builder.Property(p => p.RataRata).HasColumnName("rata_rata").HasPrecision(18, 3);
        builder.Property(p => p.KontrakMinimum).HasColumnName("kontrak_minimum").HasPrecision(18, 3);
        builder.Property(p => p.KontrakMaksimum).HasColumnName("kontrak_maksimum").HasPrecision(18, 3);
        builder.Property(p => p.SortOrder).HasColumnName("sort_order");
    }
}

public sealed class NolRequestDailyConfiguration : IEntityTypeConfiguration<NolRequestDaily>
{
    public void Configure(EntityTypeBuilder<NolRequestDaily> builder)
    {
        builder.ToTable("nol_request_daily");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.NolRequestCompanyId).HasColumnName("nol_request_company_id");

        builder.Property(d => d.Hari)
            .HasColumnName("hari")
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.Property(d => d.Min).HasColumnName("min").HasPrecision(18, 3);
        builder.Property(d => d.Max).HasColumnName("max").HasPrecision(18, 3);
    }
}

public sealed class NolRequestReferenceConfiguration : IEntityTypeConfiguration<NolRequestReference>
{
    public void Configure(EntityTypeBuilder<NolRequestReference> builder)
    {
        builder.ToTable("nol_request_reference");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.NolRequestCompanyId).HasColumnName("nol_request_company_id");
        builder.Property(r => r.ReferenceDocumentId).HasColumnName("reference_document_id");

        builder.HasOne<ReferenceDocument>()
            .WithMany()
            .HasForeignKey(r => r.ReferenceDocumentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
