using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Audit;

namespace Simando.Infrastructure.Persistence.Configurations;

public sealed class BreakGlassAccessConfiguration : IEntityTypeConfiguration<BreakGlassAccess>
{
    public void Configure(EntityTypeBuilder<BreakGlassAccess> builder)
    {
        builder.ToTable("break_glass_access");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Reason).IsRequired().HasMaxLength(500);
        builder.Property(b => b.RequestedAt).IsRequired();
        builder.Property(b => b.ExpiresAt).IsRequired();

        builder.HasIndex(b => new { b.UserId, b.CompanyId, b.ExpiresAt });
    }
}
