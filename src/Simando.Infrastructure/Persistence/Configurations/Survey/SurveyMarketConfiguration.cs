using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Directory;
using Simando.Domain.MasterData;
using Simando.Domain.Survey;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/design/data-model.md#survey--stage-4-kk0-header. Repeating child of
// Survey — Orientasi Pasar; same shape as SurveyRawMaterialConfiguration,
// CountryId means destination here rather than origin.
public sealed class SurveyMarketConfiguration : IEntityTypeConfiguration<SurveyMarket>
{
    public void Configure(EntityTypeBuilder<SurveyMarket> builder)
    {
        builder.ToTable("survey_market");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.CompanyId).HasColumnName("company_id");

        builder.Property(m => m.Bahan).HasColumnName("bahan").HasMaxLength(200);
        builder.Property(m => m.Asal).HasColumnName("asal").HasConversion<string>().HasMaxLength(10);
        builder.Property(m => m.CountryId).HasColumnName("country_id");
        builder.Property(m => m.Volume).HasColumnName("volume").HasPrecision(18, 3);
        builder.Property(m => m.SatuanUnitId).HasColumnName("satuan_unit_id");
        builder.Property(m => m.SortOrder).HasColumnName("sort_order").IsRequired();

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(m => m.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(m => m.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UnitOfMeasure>()
            .WithMany()
            .HasForeignKey(m => m.SatuanUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.CompanyId);
    }
}
