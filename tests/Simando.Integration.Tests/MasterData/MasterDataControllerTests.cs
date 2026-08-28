using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Simando.Api.Controllers;
using Simando.Domain.MasterData;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.MasterData;

public class MasterDataControllerTests : IAsyncLifetime
{
    private const string AdminEmail = "admin.master@pgn.co.id";
    private const string AdminInitialPassword = "Correct-Horse-Battery-Staple-1";
    private const string AdminPassword = "New-Correct-Horse-Password-1";

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new(System.Text.Json.JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgis/postgis:18-3.6-alpine")
        .WithDatabase("simando")
        .WithUsername("simando")
        .WithPassword("simando")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder
                .ConfigureAppConfiguration((_, config) =>
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:Postgres"] = _container.GetConnectionString(),
                        ["Storage:Type"] = "S3",
                        ["Storage:S3:ServiceUrl"] = "http://localhost:9000",
                        ["Storage:S3:Bucket"] = "simando",
                        ["Storage:S3:AccessKey"] = "test",
                        ["Storage:S3:SecretKey"] = "test",
                    }))
                .ConfigureServices(services => services.RemoveAll<IHostedService>()));

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();
            await db.Database.MigrateAsync();

            var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
            await seeder.SeedAsync("admin.master", AdminInitialPassword, "Admin Master", email: AdminEmail);

            var region = new Region { Id = Guid.NewGuid(), Code = "SOR1", Name = "Region 1 - Sumatera", Active = true };
            var area = new Area { Id = Guid.NewGuid(), RegionId = region.Id, Code = "MED", Name = "Area Medan", Active = true };
            db.Regions.Add(region);
            db.Areas.Add(area);

            db.IndustryTypes.Add(new IndustryType { Id = Guid.NewGuid(), Name = "Industri Kimia", ContohProduk = "Pupuk, Polimer" });
            db.FuelTypes.Add(new FuelType { Id = Guid.NewGuid(), Name = "Batubara" });
            db.UnitsOfMeasure.Add(new UnitOfMeasure { Id = Guid.NewGuid(), Code = "MMBTU", Name = "Million BTU", Dimension = UnitDimension.Energy });
            db.Countries.Add(new Country { Id = Guid.NewGuid(), IsoCode = "ID", Name = "Indonesia" });
            db.Segments.Add(new Segment { Id = Guid.NewGuid(), Name = "Platinum", SortOrder = 1 });
            db.ReasonCategories.Add(new ReasonCategory { Id = Guid.NewGuid(), Name = "Kelengkapan Dokumen" });
            db.MrsSpecs.Add(new MrsSpec { Id = Guid.NewGuid(), Name = "MRS Type A 1000" });
            db.MeterSizes.Add(new MeterSize { Id = Guid.NewGuid(), GSize = "G16", NominalFlow = 16, MaxFlow = 25, PressureRating = 4 });

            await db.SaveChangesAsync();
        }

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Sign in as admin
        await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(AdminEmail, AdminInitialPassword));
        await _client.PostAsJsonAsync("/api/auth/change-password", new ChangePasswordRequest(AdminInitialPassword, AdminPassword));
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Fact(DisplayName = "GET /api/master/* endpoints return populated master data lookup lists")]
    public async Task MasterDataLookupEndpoints_ReturnPopulatedLists()
    {
        // 1. Industry Types
        var indResponse = await _client.GetAsync("/api/master/industry-types");
        indResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var industries = await indResponse.Content.ReadFromJsonAsync<List<IndustryTypeDto>>(JsonOptions);
        industries.ShouldNotBeNull();
        industries.ShouldContain(i => i.Name == "Industri Kimia");

        // 2. Areas
        var areaResponse = await _client.GetAsync("/api/master/areas");
        areaResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var areas = await areaResponse.Content.ReadFromJsonAsync<List<AreaDto>>(JsonOptions);
        areas.ShouldNotBeNull();
        areas.ShouldContain(a => a.Code == "MED");

        // 3. Fuel Types
        var fuelResponse = await _client.GetAsync("/api/master/fuel-types");
        fuelResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var fuels = await fuelResponse.Content.ReadFromJsonAsync<List<FuelTypeDto>>(JsonOptions);
        fuels.ShouldNotBeNull();
        fuels.ShouldContain(f => f.Name == "Batubara");

        // 4. Units
        var unitResponse = await _client.GetAsync("/api/master/units");
        unitResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var units = await unitResponse.Content.ReadFromJsonAsync<List<UnitOfMeasureDto>>(JsonOptions);
        units.ShouldNotBeNull();
        units.ShouldContain(u => u.Code == "MMBTU");

        // 5. Countries
        var countryResponse = await _client.GetAsync("/api/master/countries");
        countryResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var countries = await countryResponse.Content.ReadFromJsonAsync<List<CountryDto>>(JsonOptions);
        countries.ShouldNotBeNull();
        countries.ShouldContain(c => c.IsoCode == "ID");

        // 6. Segments
        var segResponse = await _client.GetAsync("/api/master/segments");
        segResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var segments = await segResponse.Content.ReadFromJsonAsync<List<SegmentDto>>(JsonOptions);
        segments.ShouldNotBeNull();
        segments.ShouldContain(s => s.Name == "Platinum");

        // 7. Reason Categories
        var rcResponse = await _client.GetAsync("/api/master/reason-categories");
        rcResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var rcs = await rcResponse.Content.ReadFromJsonAsync<List<ReasonCategoryDto>>(JsonOptions);
        rcs.ShouldNotBeNull();
        rcs.ShouldContain(r => r.Name == "Kelengkapan Dokumen");

        // 8. MRS Specs
        var mrsResponse = await _client.GetAsync("/api/master/mrs-specs");
        mrsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var mrs = await mrsResponse.Content.ReadFromJsonAsync<List<MrsSpecDto>>(JsonOptions);
        mrs.ShouldNotBeNull();
        mrs.ShouldContain(m => m.Name == "MRS Type A 1000");

        // 9. Meter Sizes
        var meterResponse = await _client.GetAsync("/api/master/meter-sizes");
        meterResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var meters = await meterResponse.Content.ReadFromJsonAsync<List<MeterSizeDto>>(JsonOptions);
        meters.ShouldNotBeNull();
        meters.ShouldContain(m => m.GSize == "G16");
    }
}
