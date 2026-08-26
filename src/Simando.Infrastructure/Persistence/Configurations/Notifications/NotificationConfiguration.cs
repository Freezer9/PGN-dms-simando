using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simando.Domain.Directory;
using Simando.Domain.Notifications;
using Simando.Infrastructure.Identity;

namespace Simando.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notification");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id).HasColumnName("id");
        builder.Property(n => n.RecipientUserId).HasColumnName("recipient_user_id");
        builder.Property(n => n.CompanyId).HasColumnName("company_id");
        builder.Property(n => n.Message).HasColumnName("message").IsRequired();
        builder.Property(n => n.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(n => n.ReadAt).HasColumnName("read_at");

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(n => n.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(n => n.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // The query the (separate, not-yet-built) bell panel will run: unread
        // notifications for a recipient.
        builder.HasIndex(n => new { n.RecipientUserId, n.ReadAt });
    }
}
