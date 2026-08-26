using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Simando.Domain.Security;
using Simando.Infrastructure;
using Simando.Infrastructure.Identity;

namespace Simando.Web.Cli;

// This standalone container has no signed-in user, but SimandoDbContext's
// row-level-security query filter (see CompanyConfiguration) still needs an
// ICurrentUser to construct. AccessScope.All so a seed run never gets
// filtered out of its own writes.
internal sealed class SystemCurrentUser : ICurrentUser
{
    public Guid UserId => Guid.Empty;
    public AccessScope Scope => AccessScope.All;
    public Guid? AreaId => null;
    public Guid? RegionId => null;
    public bool HasCapability(Capability capability) => true;
}

// Admin seed CLI command per docs/design/roles-permissions.md §2.6.
public static class SeedAdminCommand
{
    public const string Name = "seed-admin";

    public static async Task<int> RunAsync()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var username = configuration["SeedAdmin:Username"] ?? "admin";
        var password = configuration["SeedAdmin:Password"];
        var fullName = configuration["SeedAdmin:FullName"] ?? "System Admin";
        var email = configuration["SeedAdmin:Email"] ?? "admin@pgn.co.id";

        if (string.IsNullOrWhiteSpace(password))
        {
            await Console.Error.WriteLineAsync(
                "SeedAdmin:Password must be supplied " +
                "(e.g. SeedAdmin__Password environment variable) -- never committed.");
            return 1;
        }

        var services = new ServiceCollection();
        services.AddLogging(logging => logging.AddConsole());
        services.AddSingleton<ICurrentUser, SystemCurrentUser>();
        services.AddInfrastructure(configuration);

        // Standalone CLI container (ASP0000 suppression rationale).
#pragma warning disable ASP0000
        await using var provider = services.BuildServiceProvider();
#pragma warning restore ASP0000
        await using var scope = provider.CreateAsyncScope();

        var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
        var result = await seeder.SeedAsync(username, password, fullName, email);

        switch (result.Outcome)
        {
            case AdminSeedOutcome.Created:
                Console.WriteLine($"Seed System Admin '{username}' created. must_change_password is set.");
                return 0;
            case AdminSeedOutcome.AlreadyExists:
                Console.WriteLine("An active System Admin already exists. Nothing to do.");
                return 0;
            default:
                foreach (var error in result.Errors)
                {
                    await Console.Error.WriteLineAsync(error);
                }

                return 1;
        }
    }
}
