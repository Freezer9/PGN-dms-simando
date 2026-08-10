using BlazorBlueprint.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Pgn.Dms.Web.Components;
using Pgn.Dms.Web.Components.Account;
using Pgn.Dms.Web.Data;
using Pgn.Dms.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazorBlueprintComponents(configureTheme: options =>
{
    options.DefaultBaseColor = BaseColor.Slate;
    options.DefaultPrimaryColor = PrimaryColor.Blue;
    options.DefaultRadius = 0.5;

    // SIMANDO is light-only. All three are needed: DetectSystemPreference would let an
    // OS dark setting win, and PersistToLocalStorage would restore a `bb-theme` entry
    // saved before the toggle was removed.
    options.DefaultDarkMode = false;
    options.DetectSystemPreference = false;
    options.PersistToLocalStorage = false;
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(SimandoPolicies.IsSalesArea, policy => policy.RequireRole(SimandoRoles.SalesArea))
    .AddPolicy(SimandoPolicies.IsAreaHead, policy => policy.RequireRole(SimandoRoles.AreaHead))
    .AddPolicy(SimandoPolicies.IsRegionSales, policy => policy.RequireRole(SimandoRoles.RegionSales))
    .AddPolicy(SimandoPolicies.IsReviewer, policy => policy.RequireRole(SimandoRoles.Reviewer))
    .AddPolicy(SimandoPolicies.CanViewSubscriptions, policy => policy.RequireRole(SimandoRoles.All))
    .AddPolicy(SimandoPolicies.CanEditRecord, policy => policy.RequireRole(
        SimandoRoles.SalesArea, SimandoRoles.RegionSales))
    .AddPolicy(SimandoPolicies.CanAccessEvaluation, policy => policy.RequireRole(SimandoRoles.RegionSales))
    .AddPolicy(SimandoPolicies.CanAccessQaQc, policy => policy.RequireRole(
        SimandoRoles.RegionSales, SimandoRoles.Reviewer))
    .AddPolicy(SimandoPolicies.CanApprove, policy => policy.RequireRole(
        SimandoRoles.AreaHead, SimandoRoles.RegionSales, SimandoRoles.Reviewer));

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<BearerTokenHandler>();

var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5010";

builder.Services.AddHttpClient<IAuthService, AuthService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<ISubscriptionService, SubscriptionService>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<BearerTokenHandler>();
builder.Services.AddHttpClient<IWorkflowService, WorkflowService>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<BearerTokenHandler>();
builder.Services.AddHttpClient<IEvaluationService, EvaluationService>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<BearerTokenHandler>();
builder.Services.AddHttpClient<IActivityService, ActivityService>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<BearerTokenHandler>();
builder.Services.AddHttpClient<IUserService, UserService>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<BearerTokenHandler>();
builder.Services.AddHttpClient<IStageService, StageService>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<BearerTokenHandler>();
builder.Services.AddHttpClient<IMasterDataService, MasterDataService>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<BearerTokenHandler>();

builder.Services.AddScoped<CommandPaletteService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

app.Run();
