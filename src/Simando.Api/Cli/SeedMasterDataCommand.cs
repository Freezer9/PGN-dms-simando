using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Simando.Domain.Security;
using Simando.Infrastructure;
using Simando.Infrastructure.Geography;
using Simando.Infrastructure.MasterData;
using Simando.Infrastructure.Persistence;

namespace Simando.Api.Cli;

// Go-live checklist items #2 and #4-#8 (docs/domain/master-data.md #13):
// administrative geography plus the small lookup lists (units, fuel types,
// industry types, countries, segments) that ship with this repo rather
// than needing PGN input. Regions/Areas, user accounts, meter sizes,
// document templates and reference documents stay out of scope -- those
// are blocked on PGN-supplied data (org chart, staff list, catalogues) and
// have no source to seed from yet.
public static class SeedMasterDataCommand
{
    public const string Name = "seed-master-data";

    public static async Task<int> RunAsync()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

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

        var geography = await scope.ServiceProvider.GetRequiredService<GeographySeeder>().SeedAsync();
        Console.WriteLine(geography.AlreadySeeded
            ? "Geography: already seeded, skipped."
            : $"Geography: {geography.Provinces} provinces, {geography.Regencies} regencies, " +
              $"{geography.Districts} districts, {geography.Villages} villages.");

        var masterData = await scope.ServiceProvider.GetRequiredService<MasterDataSeeder>().SeedAsync();
        Console.WriteLine($"Units of measure: {Describe(masterData.Units)}");
        Console.WriteLine($"Fuel types: {Describe(masterData.FuelTypes)}");
        Console.WriteLine($"Industry types: {Describe(masterData.IndustryTypes)}");
        Console.WriteLine($"Countries: {Describe(masterData.Countries)}");
        Console.WriteLine($"Segments: {Describe(masterData.Segments)}");

        return 0;
    }

    private static string Describe(int inserted) => inserted == 0 ? "already seeded, skipped." : $"{inserted} rows created.";
}
