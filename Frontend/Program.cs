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
    .AddPolicy(SimandoPolicies.CanViewDashboard, policy => policy.RequireRole(SimandoRoles.All))
    .AddPolicy(SimandoPolicies.CanViewSubscriptions, policy => policy.RequireRole(SimandoRoles.Operational))
    .AddPolicy(SimandoPolicies.CanCreateSubscription, policy => policy.RequireRole(
        SimandoRoles.SalesArea, SimandoRoles.AdminRegional))
    .AddPolicy(SimandoPolicies.CanReview, policy => policy.RequireRole(
        SimandoRoles.AreaHead, SimandoRoles.AdminRegional, SimandoRoles.Reviewer))
    .AddPolicy(SimandoPolicies.CanApprove, policy => policy.RequireRole(
        SimandoRoles.AreaHead, SimandoRoles.AdminRegional, SimandoRoles.Reviewer, SimandoRoles.DivisionHead))
    .AddPolicy(SimandoPolicies.CanAccessEvaluation, policy => policy.RequireRole(SimandoRoles.AdminRegional))
    .AddPolicy(SimandoPolicies.CanViewRegion, policy => policy.RequireRole(
        SimandoRoles.AdminRegional, SimandoRoles.Reviewer, SimandoRoles.DivisionHead))
    .AddPolicy(SimandoPolicies.CanAdminister, policy => policy.RequireRole(
        SimandoRoles.AdminRegional, SimandoRoles.SystemAdmin))
    .AddPolicy(SimandoPolicies.CanIssueNOL, policy => policy.RequireRole(SimandoRoles.DivisionHead))
    .AddPolicy(SimandoPolicies.CanManageTasksBlocked, policy => policy.RequireRole(SimandoRoles.AdminRegional))
    .AddPolicy(SimandoPolicies.CanAccessBreakGlass, policy => policy.RequireRole(
        SimandoRoles.AdminRegional, SimandoRoles.DivisionHead, SimandoRoles.SystemAdmin));

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
