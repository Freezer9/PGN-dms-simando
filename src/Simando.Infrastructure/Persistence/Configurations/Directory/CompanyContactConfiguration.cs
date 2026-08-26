using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Directory;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/design/data-model.md#company_contact--stage-3-repeating.
public sealed class CompanyContactConfiguration : IEntityTypeConfiguration<CompanyContact>
{
    public void Configure(EntityTypeBuilder<CompanyContact> builder)
    {
        builder.ToTable("company_contact");

        builder.HasKey(cc => cc.Id);

        builder.Property(cc => cc.Id).HasColumnName("id");
        builder.Property(cc => cc.CompanyId).HasColumnName("company_id");

        builder.Property(cc => cc.Nama)
            .HasColumnName("nama")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cc => cc.Jabatan)
            .HasColumnName("jabatan")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cc => cc.Email).HasColumnName("email").HasMaxLength(200);
        builder.Property(cc => cc.NoHp).HasColumnName("no_hp").HasMaxLength(30);
        builder.Property(cc => cc.LinkedIn).HasColumnName("linkedin").HasMaxLength(200);
        builder.Property(cc => cc.Instagram).HasColumnName("instagram").HasMaxLength(200);
        builder.Property(cc => cc.Facebook).HasColumnName("facebook").HasMaxLength(200);

        builder.Property(cc => cc.IsPrimary).HasColumnName("is_primary").IsRequired();
        builder.Property(cc => cc.SortOrder).HasColumnName("sort_order").IsRequired();

        // Cascade — owned repeating child of one Company, same reasoning as
        // Plotting (see PlottingConfiguration).
        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(cc => cc.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cc => cc.CompanyId);
    }
}
