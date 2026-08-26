using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Directory;
using Simando.Domain.MasterData;
using Simando.Domain.Survey;
using Simando.Infrastructure.Identity;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/design/data-model.md#survey--stage-4-kk0-header. 1:1 with Company,
// same shape as PlottingConfiguration — company_id is the primary key, no
// surrogate id.
public sealed class SurveyConfiguration : IEntityTypeConfiguration<Survey>
{
    public void Configure(EntityTypeBuilder<Survey> builder)
    {
        builder.ToTable("survey");

        builder.HasKey(s => s.CompanyId);

        builder.Property(s => s.CompanyId).HasColumnName("company_id");
        builder.Property(s => s.TanggalSurvey).HasColumnName("tanggal_survey");
        builder.Property(s => s.SurveyorUserId).HasColumnName("surveyor_user_id");

        builder.Property(s => s.JumlahKaryawan).HasColumnName("jumlah_karyawan");
        builder.Property(s => s.JumlahShift).HasColumnName("jumlah_shift");
        builder.Property(s => s.JamKerjaPerHari).HasColumnName("jam_kerja_per_hari").HasPrecision(5, 2);
        builder.Property(s => s.HariPerMinggu).HasColumnName("hari_per_minggu");

        builder.Property(s => s.BebanPuncak1Mulai).HasColumnName("beban_puncak1_mulai");
        builder.Property(s => s.BebanPuncak1Selesai).HasColumnName("beban_puncak1_selesai");
        builder.Property(s => s.BebanPuncak2Mulai).HasColumnName("beban_puncak2_mulai");
        builder.Property(s => s.BebanPuncak2Selesai).HasColumnName("beban_puncak2_selesai");

        builder.Property(s => s.KebutuhanEnergi)
            .HasColumnName("kebutuhan_energi")
            .HasConversion<string>()
            .HasMaxLength(100);
        builder.Property(s => s.KebutuhanEnergiLainnya).HasColumnName("kebutuhan_energi_lainnya").HasMaxLength(200);
        builder.Property(s => s.KapasitasNilai).HasColumnName("kapasitas_nilai").HasPrecision(18, 3);
        builder.Property(s => s.KapasitasUnitId).HasColumnName("kapasitas_unit_id");
        builder.Property(s => s.PemakaianNilai).HasColumnName("pemakaian_nilai").HasPrecision(18, 3);
        builder.Property(s => s.PemakaianUnitId).HasColumnName("pemakaian_unit_id");

        builder.Property(s => s.PipaTerdekatJarakM).HasColumnName("pipa_terdekat_jarak_m").HasPrecision(18, 3);
        builder.Property(s => s.PipaTerdekatDiameter).HasColumnName("pipa_terdekat_diameter").HasPrecision(18, 3);
        builder.Property(s => s.PipaTerdekatTekanan).HasColumnName("pipa_terdekat_tekanan").HasPrecision(18, 3);

        builder.Property(s => s.BahanBakarEksisting)
            .HasColumnName("bahan_bakar_eksisting")
            .HasConversion<string>()
            .HasMaxLength(50);
        builder.Property(s => s.NamaPemasok).HasColumnName("nama_pemasok").HasMaxLength(200);
        builder.Property(s => s.KapasitasListrikKw).HasColumnName("kapasitas_listrik_kw").HasPrecision(18, 3);
        builder.Property(s => s.PemakaianListrikKwh).HasColumnName("pemakaian_listrik_kwh").HasPrecision(18, 3);

        builder.Property(s => s.RencanaPemanfaatanGas)
            .HasColumnName("rencana_pemanfaatan_gas")
            .HasConversion<string>()
            .HasMaxLength(100);
        builder.Property(s => s.DeskripsiProsesProduksi).HasColumnName("deskripsi_proses_produksi");

        builder.Property(s => s.MinEfisiensiDiharapkanPct).HasColumnName("min_efisiensi_diharapkan_pct").HasPrecision(5, 2);
        builder.Property(s => s.WillingnessToPayUsdMmbtu).HasColumnName("willingness_to_pay_usd_mmbtu").HasPrecision(18, 3);
        builder.Property(s => s.KeteranganLain).HasColumnName("keterangan_lain");
        builder.Property(s => s.JumlahKebutuhanEnergi).HasColumnName("jumlah_kebutuhan_energi").HasPrecision(18, 3).IsRequired();

        // Cascade, same reasoning as PlottingConfiguration: an owned 1:1
        // extension of one specific Company row.
        builder.HasOne<Company>()
            .WithOne()
            .HasForeignKey<Survey>(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(s => s.SurveyorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UnitOfMeasure>()
            .WithMany()
            .HasForeignKey(s => s.KapasitasUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UnitOfMeasure>()
            .WithMany()
            .HasForeignKey(s => s.PemakaianUnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
