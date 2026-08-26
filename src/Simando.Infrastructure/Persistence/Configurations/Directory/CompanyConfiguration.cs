using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Directory;
using Simando.Domain.Geography;
using Simando.Domain.MasterData;
using Simando.Domain.Organisation;
using Simando.Infrastructure.Identity;

namespace Simando.Infrastructure.Persistence.Configurations;

// docs/design/data-model.md#company--the-spine.
public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("company");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnName("id");

        // company_nomor_seq is declared once, model-wide, in
        // SimandoDbContext.OnModelCreating — a plain PostgreSQL SEQUENCE,
        // atomic and lock-free, allocated on first save via nextval().
        // docs/domain/03-directory-plotting.md#allocating-the-number.
        builder.Property(c => c.NomorSeq)
            .HasColumnName("nomor_seq")
            .HasDefaultValueSql("nextval('company_nomor_seq')")
            .ValueGeneratedOnAdd();

        // Rendered "{seq}-{bps_prov}-{bps_kab}" — re-rendered while Draft,
        // frozen once the record leaves Draft.
        builder.Property(c => c.Nomor)
            .HasColumnName("nomor")
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(c => c.NamaPerusahaan)
            .HasColumnName("nama_perusahaan")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Website)
            .HasColumnName("website")
            .HasMaxLength(200);

        builder.Property(c => c.VillageId).HasColumnName("village_id");

        builder.Property(c => c.Alamat)
            .HasColumnName("alamat")
            .IsRequired();

        // geography(Point,4326) via NetTopologySuite — not two decimal
        // columns, the map does radius and bbox queries.
        // docs/infra: infra-postgis-geography-column-gist-index.
        builder.Property(c => c.Location)
            .HasColumnName("location")
            .HasColumnType("geography(Point,4326)");

        builder.Property(c => c.IndustryTypeId).HasColumnName("industry_type_id");

        builder.Property(c => c.Npwp).HasColumnName("npwp").HasMaxLength(30);
        builder.Property(c => c.Email).HasColumnName("email").HasMaxLength(200);
        builder.Property(c => c.KodePos).HasColumnName("kode_pos").HasMaxLength(10);
        builder.Property(c => c.Telp).HasColumnName("telp").HasMaxLength(30);
        builder.Property(c => c.Fax).HasColumnName("fax").HasMaxLength(30);

        builder.Property(c => c.AreaId).HasColumnName("area_id");

        builder.Property(c => c.CurrentStage)
            .HasColumnName("current_stage")
            .IsRequired();

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.CreatedBy).HasColumnName("created_by");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");
        builder.Property(c => c.DeletedAt).HasColumnName("deleted_at");

        // Restrict, not cascade — village/industry-type/area are shared
        // reference data, deactivated rather than deleted.
        builder.HasOne<Village>()
            .WithMany()
            .HasForeignKey(c => c.VillageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<IndustryType>()
            .WithMany()
            .HasForeignKey(c => c.IndustryTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Area>()
            .WithMany()
            .HasForeignKey(c => c.AreaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.NomorSeq).IsUnique();
        builder.HasIndex(c => c.Nomor).IsUnique();

        // Every list screen filters on these three.
        builder.HasIndex(c => new { c.AreaId, c.CurrentStage, c.Status });

        // GiST index for map radius/bbox queries.
        builder.HasIndex(c => c.Location).HasMethod("GIST");
    }
}
