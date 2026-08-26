using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.MasterData;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/domain/master-data.md §5: industry_type(id, name, contoh_produk, created_at, deleted_at).
public sealed class IndustryTypeConfiguration : AuditableEntityConfiguration<IndustryType>
{
    protected override void ConfigureEntity(EntityTypeBuilder<IndustryType> builder)
    {
        builder.ToTable("industry_type");

        builder.Property(i => i.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.ContohProduk)
            .HasColumnName("contoh_produk")
            .HasMaxLength(200);

        builder.HasIndex(i => i.Name).IsUnique().HasFilter("deleted_at IS NULL");
    }
}
