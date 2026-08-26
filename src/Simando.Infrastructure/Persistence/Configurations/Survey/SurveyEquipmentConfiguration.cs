using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Directory;
using Simando.Domain.MasterData;
using Simando.Domain.Survey;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/design/data-model.md#survey--stage-4-kk0-header. Repeating child of
// Survey — the equipment table that drives Survey.JumlahKebutuhanEnergi.
public sealed class SurveyEquipmentConfiguration : IEntityTypeConfiguration<SurveyEquipment>
{
    public void Configure(EntityTypeBuilder<SurveyEquipment> builder)
    {
        builder.ToTable("survey_equipment");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.CompanyId).HasColumnName("company_id");

        builder.Property(e => e.JenisPeralatan).HasColumnName("jenis_peralatan").IsRequired().HasMaxLength(200);
        builder.Property(e => e.Kapasitas).HasColumnName("kapasitas").HasPrecision(18, 3);
        builder.Property(e => e.KapasitasUnitId).HasColumnName("kapasitas_unit_id");
        builder.Property(e => e.JamPerHari).HasColumnName("jam_per_hari").HasPrecision(5, 2);
        builder.Property(e => e.HariPerMinggu).HasColumnName("hari_per_minggu");
        builder.Property(e => e.FuelTypeId).HasColumnName("fuel_type_id");
        builder.Property(e => e.HargaBahanBakar).HasColumnName("harga_bahan_bakar").HasPrecision(18, 2);
        builder.Property(e => e.KonsumsiPerBulan).HasColumnName("konsumsi_per_bulan").HasPrecision(18, 3);
        builder.Property(e => e.KonsumsiUnitId).HasColumnName("konsumsi_unit_id");

        // Plain manually-typed field, not derived — no conversion service.
        // docs/domain/04-prospect-survey.md#the-conversion-engine.
        builder.Property(e => e.KonversiKeGas).HasColumnName("konversi_ke_gas").HasPrecision(18, 3).IsRequired();

        builder.Property(e => e.SortOrder).HasColumnName("sort_order").IsRequired();

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<FuelType>()
            .WithMany()
            .HasForeignKey(e => e.FuelTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UnitOfMeasure>()
            .WithMany()
            .HasForeignKey(e => e.KapasitasUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UnitOfMeasure>()
            .WithMany()
            .HasForeignKey(e => e.KonsumsiUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.CompanyId);
    }
}
