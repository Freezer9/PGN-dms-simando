using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Directory;
using Simando.Infrastructure.Identity;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/design/data-model.md#plotting--stage-2. 1:1 with Company;
// company_id is the primary key, no surrogate id.
public sealed class PlottingConfiguration : IEntityTypeConfiguration<Plotting>
{
    public void Configure(EntityTypeBuilder<Plotting> builder)
    {
        builder.ToTable("plotting");

        builder.HasKey(p => p.CompanyId);

        builder.Property(p => p.CompanyId).HasColumnName("company_id");
        builder.Property(p => p.SalesUserId).HasColumnName("sales_user_id");

        builder.Property(p => p.PosisiPelanggan)
            .HasColumnName("posisi_pelanggan")
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.Kawasan)
            .HasColumnName("kawasan")
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(30);

        // Cascade, unlike Company's own reference-data FKs: Plotting is an
        // owned 1:1 extension of one specific Company row, not shared
        // master data — it has no meaning once its Company is gone.
        builder.HasOne<Company>()
            .WithOne()
            .HasForeignKey<Plotting>(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(p => p.SalesUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
