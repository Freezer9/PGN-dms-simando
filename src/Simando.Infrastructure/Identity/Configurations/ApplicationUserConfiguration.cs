using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Simando.Infrastructure.Identity.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // Not "user" — a reserved word / builtin in PostgreSQL (SELECT user;).
        builder.ToTable("app_user");

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.ExternalId)
            .HasMaxLength(100);
    }
}
