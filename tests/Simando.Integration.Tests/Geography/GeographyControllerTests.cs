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
using Simando.Application.Geography;
using Simando.Domain.Geography;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Geography;

public class GeographyControllerTests : IAsyncLifetime
{
    private const string AdminEmail = "admin.geo@pgn.co.id";
    private const string AdminInitialPassword = "Correct-Horse-Battery-Staple-1";
    private const string AdminPassword = "New-Correct-Horse-Password-1";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgis/postgis:18-3.6-alpine")
        .WithDatabase("simando")
        .WithUsername("simando")
        .WithPassword("simando")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private Guid _provinceId;
    private Guid _regencyId;
    private Guid _districtId;

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
            await seeder.SeedAsync("admin.geo", AdminInitialPassword, "Admin Geo", email: AdminEmail);

            var province = new Province { Id = Guid.NewGuid(), BpsCode = "35", Name = "Jawa Timur" };
            _provinceId = province.Id;

            var regency = new Regency { Id = Guid.NewGuid(), ProvinceId = province.Id, BpsCode = "3578", Type = RegencyType.Kota, Name = "Kota Surabaya" };
            _regencyId = regency.Id;

            var district = new District { Id = Guid.NewGuid(), RegencyId = regency.Id, BpsCode = "357801", Name = "Tegalsari" };
            _districtId = district.Id;

            var village = new Village { Id = Guid.NewGuid(), DistrictId = district.Id, BpsCode = "3578011001", Type = VillageType.Kelurahan, Name = "Tegalsari" };

            db.Provinces.Add(province);
            db.Regencies.Add(regency);
            db.Districts.Add(district);
            db.Villages.Add(village);

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

    [Fact(DisplayName = "GET /api/geography cascading endpoints return valid hierarchical options")]
    public async Task GeographyEndpoints_ReturnValidOptions()
    {
        // 1. Provinces
        var provResponse = await _client.GetAsync("/api/geography/provinces");
        provResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var provinces = await provResponse.Content.ReadFromJsonAsync<List<GeographyOption>>();
        provinces.ShouldNotBeNull();
        provinces.ShouldContain(p => p.Name == "Jawa Timur");

        // 2. Regencies
        var regResponse = await _client.GetAsync($"/api/geography/regencies?provinceId={_provinceId}");
        regResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var regencies = await regResponse.Content.ReadFromJsonAsync<List<GeographyOption>>();
        regencies.ShouldNotBeNull();
        regencies.ShouldContain(r => r.Name == "Kota Surabaya");

        // 3. Districts
        var distResponse = await _client.GetAsync($"/api/geography/districts?regencyId={_regencyId}");
        distResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var districts = await distResponse.Content.ReadFromJsonAsync<List<GeographyOption>>();
        districts.ShouldNotBeNull();
        districts.ShouldContain(d => d.Name == "Tegalsari");

        // 4. Villages
        var villResponse = await _client.GetAsync($"/api/geography/villages?districtId={_districtId}");
        villResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var villages = await villResponse.Content.ReadFromJsonAsync<List<GeographyOption>>();
        villages.ShouldNotBeNull();
        villages.ShouldContain(v => v.Name == "Tegalsari");
    }
}
