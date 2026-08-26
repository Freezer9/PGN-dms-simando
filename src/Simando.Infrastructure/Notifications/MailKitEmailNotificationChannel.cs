using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Simando.Application.Notifications;
using Simando.Infrastructure.Persistence;

namespace Simando.Infrastructure.Notifications;

public sealed class MailKitEmailNotificationChannel : INotificationChannel
{
    private readonly IDbContextFactory<SimandoDbContext> _dbContextFactory;
    private readonly SmtpOptions _options;
    private readonly ILogger<MailKitEmailNotificationChannel> _logger;

    public MailKitEmailNotificationChannel(
        IDbContextFactory<SimandoDbContext> dbContextFactory,
        IOptions<SmtpOptions> options,
        ILogger<MailKitEmailNotificationChannel> logger)
    {
        _dbContextFactory = dbContextFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(Guid recipientUserId, Guid companyId, string message, CancellationToken ct = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Email notification channel is disabled per configuration. Skipping email for recipient {UserId}", recipientUserId);
            return;
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        var recipient = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == recipientUserId, ct);
        if (recipient is null || string.IsNullOrWhiteSpace(recipient.Email))
        {
            _logger.LogWarning("Recipient user {UserId} not found or has no email address. Skipping email notification.", recipientUserId);
            return;
        }

        var company = await db.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == companyId, ct);
        var companyName = company?.NamaPerusahaan ?? "Calon Pelanggan";

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        mimeMessage.To.Add(new MailboxAddress(recipient.FullName, recipient.Email));
        mimeMessage.Subject = $"[DMS Simando] Notifikasi: {companyName}";

        var bodyBuilder = new BodyBuilder
        {
            TextBody = $"{message}\n\nSilakan login ke aplikasi DMS Simando untuk menindaklanjuti permohonan ini.",
            HtmlBody = $"""
                <div style="font-family: sans-serif; padding: 20px; line-height: 1.5;">
                    <h2>Notifikasi DMS Simando</h2>
                    <p>{message}</p>
                    <p><strong>Perusahaan:</strong> {companyName}</p>
                    <hr/>
                    <p>Silakan login ke aplikasi DMS Simando untuk menindaklanjuti permohonan ini.</p>
                </div>
                """
        };

        mimeMessage.Body = bodyBuilder.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(_options.Host, _options.Port, MailKit.Security.SecureSocketOptions.Auto, ct);
            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                await client.AuthenticateAsync(_options.Username, _options.Password, ct);
            }
            await client.SendAsync(mimeMessage, ct);
            await client.DisconnectAsync(true, ct);

            _logger.LogInformation("Successfully sent email notification to {Email} for company {CompanyId}", recipient.Email, companyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email notification to {Email}", recipient.Email);
        }
    }
}
