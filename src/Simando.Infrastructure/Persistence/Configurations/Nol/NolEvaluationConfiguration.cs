using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Attachments;
using Simando.Domain.Nol;
using Simando.Infrastructure.Identity;

namespace Simando.Infrastructure.Persistence.Configurations;

public sealed class NolEvaluationConfiguration : IEntityTypeConfiguration<NolEvaluation>
{
    public void Configure(EntityTypeBuilder<NolEvaluation> builder)
    {
        builder.ToTable("nol_evaluation");

        builder.HasKey(e => e.NolRequestId);
        builder.Property(e => e.NolRequestId).HasColumnName("nol_request_id");

        builder.Property(e => e.FeedStatus)
            .HasColumnName("feed_status")
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.Property(e => e.FeedCompletedAt).HasColumnName("feed_completed_at");

        builder.Property(e => e.CapexFinal).HasColumnName("capex_final").HasPrecision(18, 3);

        builder.Property(e => e.PipaIndukPanjangM).HasColumnName("pipa_induk_panjang_m").HasPrecision(18, 3);
        builder.Property(e => e.PipaIndukDiameter).HasColumnName("pipa_induk_diameter").HasPrecision(18, 3);
        builder.Property(e => e.PipaIndukDiameterUnit)
            .HasColumnName("pipa_induk_diameter_unit")
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(e => e.PipaServicePanjangM).HasColumnName("pipa_service_panjang_m").HasPrecision(18, 3);
        builder.Property(e => e.PipaServiceDiameter).HasColumnName("pipa_service_diameter").HasPrecision(18, 3);
        builder.Property(e => e.PipaServiceDiameterUnit)
            .HasColumnName("pipa_service_diameter_unit")
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(e => e.SpesifikasiMrs).HasColumnName("spesifikasi_mrs");
        builder.Property(e => e.GSize).HasColumnName("g_size").HasMaxLength(50);
        builder.Property(e => e.Tekanan).HasColumnName("tekanan").HasPrecision(18, 3);
        builder.Property(e => e.MaksFlowrate).HasColumnName("maks_flowrate").HasPrecision(18, 3);

        builder.Property(e => e.MaksKapasitasMeterM3Jam).HasColumnName("maks_kapasitas_meter_m3_jam").HasPrecision(18, 3);
        builder.Property(e => e.DurasiPelaksanaanBulan).HasColumnName("durasi_pelaksanaan_bulan");

        builder.Property(e => e.StatusRkap)
            .HasColumnName("status_rkap")
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.Property(e => e.SkemaPembayaran)
            .HasColumnName("skema_pembayaran")
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(e => e.JaminanStatus).HasColumnName("jaminan_status").HasMaxLength(100);
        builder.Property(e => e.JaminanJenis).HasColumnName("jaminan_jenis").HasMaxLength(100);
        builder.Property(e => e.JaminanMasaBerlaku).HasColumnName("jaminan_masa_berlaku").HasMaxLength(100);
        builder.Property(e => e.JaminanPenerbit).HasColumnName("jaminan_penerbit").HasMaxLength(200);

        builder.Property(e => e.KetersediaanPasokanBbtud).HasColumnName("ketersediaan_pasokan_bbtud").HasPrecision(18, 3);

        builder.Property(e => e.AnalisisKomersial).HasColumnName("analisis_komersial");
        builder.Property(e => e.AnalisisKompetitor).HasColumnName("analisis_kompetitor");
        builder.Property(e => e.Kesimpulan).HasColumnName("kesimpulan");
        builder.Property(e => e.RadiusKompetitorKm).HasColumnName("radius_kompetitor_km").HasPrecision(18, 3);

        builder.Property(e => e.EvaluatedBy).HasColumnName("evaluated_by");
        builder.Property(e => e.EvaluatedAt).HasColumnName("evaluated_at");

        builder.HasOne<NolRequest>()
            .WithOne()
            .HasForeignKey<NolEvaluation>(e => e.NolRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.EvaluatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Scenarios)
            .WithOne()
            .HasForeignKey(s => s.NolEvaluationNolRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class NolEvaluationScenarioConfiguration : IEntityTypeConfiguration<NolEvaluationScenario>
{
    public void Configure(EntityTypeBuilder<NolEvaluationScenario> builder)
    {
        builder.ToTable("nol_evaluation_scenario");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.NolEvaluationNolRequestId).HasColumnName("nol_evaluation_nol_request_id");

        builder.Property(s => s.Label).HasColumnName("label").HasMaxLength(100).IsRequired();
        builder.Property(s => s.IrrPct).HasColumnName("irr_pct").HasPrecision(5, 2);
        builder.Property(s => s.Npv).HasColumnName("npv").HasPrecision(18, 3);
        builder.Property(s => s.PaybackYears).HasColumnName("payback_years").HasPrecision(5, 2);
        builder.Property(s => s.HasilAnalisis).HasColumnName("hasil_analisis");
    }
}

public sealed class EvaluationResumeConfiguration : IEntityTypeConfiguration<EvaluationResume>
{
    public void Configure(EntityTypeBuilder<EvaluationResume> builder)
    {
        builder.ToTable("evaluation_resume");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.NolEvaluationNolRequestId).HasColumnName("nol_evaluation_nol_request_id");
        builder.Property(r => r.GeneratedAt).HasColumnName("generated_at");
        builder.Property(r => r.GeneratedByUserId).HasColumnName("generated_by_user_id");
        builder.Property(r => r.AttachmentId).HasColumnName("attachment_id");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(r => r.GeneratedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Attachment>()
            .WithMany()
            .HasForeignKey(r => r.AttachmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
