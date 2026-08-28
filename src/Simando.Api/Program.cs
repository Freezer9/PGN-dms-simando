using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Simando.Api;
using Simando.Api.Cli;
using Simando.Api.Middleware;
using Simando.Application;
using Simando.Infrastructure;
using Simando.Infrastructure.Persistence;

if (args is [SeedAdminCommand.Name, ..])
{
    return await SeedAdminCommand.RunAsync();
}

if (args is [SeedDemoUsersCommand.Name, ..])
{
    return await SeedDemoUsersCommand.RunAsync();
}

if (args is [SeedMasterDataCommand.Name, ..])
{
    return await SeedMasterDataCommand.RunAsync();
}

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddKeyPerFile("/run/secrets", optional: true);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Simando DMS API";
        document.Info.Version = "v1";
        document.Info.Description = "REST API for PGN Gas Subscription Management System (DMS Simando)";
        return Task.CompletedTask;
    });
});

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddControllers();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseStartup");
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<SimandoDbContext>>();
    await using var db = await dbFactory.CreateDbContextAsync();

    logger.LogInformation("Applying database migrations...");
    await db.Database.MigrateAsync();
    logger.LogInformation("Database migrations applied successfully.");
}

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
};
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseExceptionHandler();
app.UseStatusCodePages();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors(ApiDependencyInjection.CorsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<MustChangePasswordMiddleware>();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("Simando DMS API Documentation")
           .WithTheme(ScalarTheme.BluePlanet);
});

app.MapControllers();

app.Run();
return 0;

public partial class Program;
