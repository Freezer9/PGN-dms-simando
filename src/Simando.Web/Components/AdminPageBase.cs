using Microsoft.AspNetCore.Components;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;
using Simando.Web.Components.Layout;
using Simando.Web.Security;

namespace Simando.Web.Components;

// Shared OnInitializedAsync for admin CRUD pages: load CurrentUser, gate on
// a capability, set the breadcrumb, then hand off to the page's own load.
// Pages override RequiredCapability/Breadcrumbs and OnAuthorizedInitializedAsync
// instead of repeating this block themselves.
public abstract class AdminPageBase : ComponentBase
{
    [Inject] protected SimandoDbContext Db { get; set; } = default!;
    [Inject] protected CurrentUser CurrentUser { get; set; } = default!;
    [Inject] protected BreadcrumbState BreadcrumbState { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;

    protected bool Authorized { get; private set; }

    // Array, not a single Capability: Pengguna is reachable via either
    // ManageMasterData (System Admin) or AssignRoles (Regional Admin), and
    // RegionalAdmin's capability set (RoleCapabilities.cs) has the latter but
    // not the former — a single-capability gate would wrongly lock it out.
    protected abstract Capability[] RequiredCapabilities { get; }
    protected abstract BreadcrumbItem[] Breadcrumbs { get; }

    protected override async Task OnInitializedAsync()
    {
        await CurrentUser.LoadAsync(Db);

        if (!RequiredCapabilities.Any(CurrentUser.HasCapability))
        {
            Nav.NavigateTo("/access-denied");
            return;
        }

        BreadcrumbState.Set(Breadcrumbs);
        Authorized = true;
        await OnAuthorizedInitializedAsync();
    }

    protected virtual Task OnAuthorizedInitializedAsync() => Task.CompletedTask;
}
