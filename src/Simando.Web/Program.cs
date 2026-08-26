using BlazorBlueprint.Components;
using Microsoft.AspNetCore.Diagnostics;
using Simando.Infrastructure;
using Simando.Web;
using Simando.Web.Cli;
using Simando.Web.Components;
using Simando.Web.Middleware;
using Simando.Web.Services;

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

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWebIdentity(builder.Configuration);
builder.Services.AddShellServices();

builder.Services.AddControllers();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddBlazorBlueprintComponents();
builder.Services.AddScoped<ToastService, CompactToastService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

// Opt the API controllers (account, attachments, documents, reports/export)
// out of the re-execute above via the documented IStatusCodePagesFeature
// hook: a 403/404 from a download or export link should come back as a
// small, clean status-only response, not the full interactive-shell page
// (sidebar, nav, notifications — several more DB round-trips) re-executed
// as the body. Must run after UseStatusCodePagesWithReExecute, which is
// what adds the feature to toggle.
app.Use(async (context, next) =>
{
    if (IsApiControllerRoute(context.Request.Path))
    {
        var feature = context.Features.Get<IStatusCodePagesFeature>();
        if (feature is not null)
        {
            feature.Enabled = false;
        }
    }

    await next(context);
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Must come after UseAuthentication/UseAuthorization.
app.UseAntiforgery();

app.UseMiddleware<MustChangePasswordMiddleware>();

app.MapControllers();
app.MapStaticAssets().AllowAnonymous();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
return 0;

static bool IsApiControllerRoute(PathString path) =>
    path.StartsWithSegments("/account") ||
    path.StartsWithSegments("/attachments") ||
    path.StartsWithSegments("/documents") ||
    path.StartsWithSegments("/reports/export");

// The implicit Program class from top-level statements is internal by
// default; Simando.Integration.Tests' WebApplicationFactory<Program> (for
// SignInFlowTests, ShellNavigationTests) needs it public. Standard,
// documented workaround.
public partial class Program;
