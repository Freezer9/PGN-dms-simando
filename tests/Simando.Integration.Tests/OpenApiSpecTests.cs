using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Simando.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests;

public class OpenApiSpecTests : IAsyncLifetime
{
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
        }

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Fact(DisplayName = "OpenAPI endpoint /openapi/v1.json returns 200 OK with valid OpenAPI 3.x document")]
    public async Task OpenApiDocument_ReturnsValidJson()
    {
        var response = await _client.GetAsync("/openapi/v1.json");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");

        var json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.TryGetProperty("openapi", out var openApiVersion).ShouldBeTrue();
        openApiVersion.GetString().ShouldStartWith("3.");

        root.TryGetProperty("info", out var info).ShouldBeTrue();
        info.GetProperty("title").GetString().ShouldBe("Simando DMS API");

        root.TryGetProperty("paths", out var paths).ShouldBeTrue();
        paths.TryGetProperty("/api/auth/login", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/auth/logout", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/auth/me", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/auth/change-password", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/companies", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/companies/{id}", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/companies/{id}/contacts", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/companies/{id}/plotting", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/companies/{id}/survey", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/companies/{id}/registration", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/companies/{id}/nol-request", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/companies/{id}/nol-evaluation", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/companies/{id}/nol-issuance", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/companies/{id}/workflow/start", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/companies/{id}/workflow/choose-reviewers", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/workflow/steps/{stepId}/act", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/workflow/steps/{stepId}/reassign", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/tasks/inbox", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/tasks/region", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/tasks/blocked", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/tasks/history", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/tasks/summary", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/companies/map-pins", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/geography/provinces", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/master/industry-types", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/master/fuel-types", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/master/units", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/master/countries", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/master/segments", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/master/reference-documents", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/master/mrs-specs", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/master/meter-sizes", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/master/reviewers", out _).ShouldBeTrue();
    }

    [Fact(DisplayName = "Scalar interactive docs /scalar/v1 returns 200 OK HTML")]
    public async Task ScalarApiReference_ReturnsOkHtml()
    {
        var response = await _client.GetAsync("/scalar/v1");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("Scalar");
    }
}
