using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;
using Simando.Domain.Security;
using Simando.Infrastructure.Notifications;
using Simando.Infrastructure.Persistence;

namespace Simando.Integration.Tests.Notifications;

public class MailKitEmailNotificationChannelTests
{
    [Fact(DisplayName = "SendAsync: when SmtpOptions.Enabled is false, skips email sending cleanly")]
    public async Task SendAsync_Disabled_SkipsSendingCleanly()
    {
        var options = Options.Create(new SmtpOptions { Enabled = false });
        var dbFactory = new DummyDbContextFactory();
        var channel = new MailKitEmailNotificationChannel(dbFactory, options, NullLogger<MailKitEmailNotificationChannel>.Instance);

        var recipientId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        // Should return without throwing
        await channel.SendAsync(recipientId, companyId, "Test message");
    }

    private sealed class DummyDbContextFactory : IDbContextFactory<SimandoDbContext>
    {
        public SimandoDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<SimandoDbContext>().Options;
            return new SimandoDbContext(options, new DummyCurrentUser());
        }

        public Task<SimandoDbContext> CreateDbContextAsync(CancellationToken ct = default) => Task.FromResult(CreateDbContext());
    }

    private sealed class DummyCurrentUser : ICurrentUser
    {
        public Guid UserId => Guid.NewGuid();
        public AccessScope Scope => AccessScope.All;
        public Guid? AreaId => null;
        public Guid? RegionId => null;
        public bool HasCapability(Capability capability) => true;
    }
}
