using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Directory;
using Simando.Domain.Survey;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/design/data-model.md#survey--stage-4-kk0-header. Repeating child of
// Survey, same shape as CompanyContactConfiguration.
public sealed class SurveyProductConfiguration : IEntityTypeConfiguration<SurveyProduct>
{
    public void Configure(EntityTypeBuilder<SurveyProduct> builder)
    {
        builder.ToTable("survey_product");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.CompanyId).HasColumnName("company_id");

        builder.Property(p => p.Produk).HasColumnName("produk").IsRequired().HasMaxLength(200);
        builder.Property(p => p.Kapasitas).HasColumnName("kapasitas").HasPrecision(18, 3);
        builder.Property(p => p.HargaProduk).HasColumnName("harga_produk").HasPrecision(18, 2);
        builder.Property(p => p.Catatan).HasColumnName("catatan");
        builder.Property(p => p.SortOrder).HasColumnName("sort_order").IsRequired();

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.CompanyId);
    }
}
