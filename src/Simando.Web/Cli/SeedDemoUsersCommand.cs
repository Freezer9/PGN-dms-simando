using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Simando.Domain.Security;
using Simando.Infrastructure;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;

namespace Simando.Web.Cli;

// Seeds one demo/test account per non-SystemAdmin role (SystemAdmin stays
// seed-admin's job) plus the minimal Region/Area org data those roles need.
// For demo/testing use, not part of the go-live path -- see DemoUserSeeder.
public static class SeedDemoUsersCommand
{
    public const string Name = "seed-demo-users";

    public static async Task<int> RunAsync()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var password = configuration["SeedDemo:Password"];

        if (string.IsNullOrWhiteSpace(password))
        {
            await Console.Error.WriteLineAsync(
                "SeedDemo:Password must be supplied " +
                "(e.g. SeedDemo__Password environment variable) -- never committed.");
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

        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<SimandoDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync();
        await db.Database.MigrateAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<DemoUserSeeder>();
        var result = await seeder.SeedAsync(password);

        if (result.Errors.Count > 0)
        {
            foreach (var error in result.Errors)
            {
                await Console.Error.WriteLineAsync(error);
            }

            return 1;
        }

        Console.WriteLine("Demo accounts (password is the one you supplied):");
        Console.WriteLine($"{"Role",-16} {"Username",-22} Status");
        foreach (var account in result.Accounts)
        {
            var status = account.WasCreated ? "created" : "already existed";
            Console.WriteLine($"{account.Role,-16} {account.Username,-22} {status}");
        }

        return 0;
    }
}
