namespace Simando.Web.Components.Layout;

public sealed record BreadcrumbItem(string Label, string? Route);

// Scoped, cascaded via MainLayout: a page calls Set(...) once (typically in
// OnInitialized) to declare its own crumb trail; MainLayout re-renders
// whatever's current. Only Home.razor sets one today — the mechanism is real
// infrastructure, just under-used until more pages exist.
public sealed class BreadcrumbState
{
    public IReadOnlyList<BreadcrumbItem> Items { get; private set; } = [];

    public event Action? Changed;

    public void Set(params BreadcrumbItem[] items)
    {
        Items = items;
        Changed?.Invoke();
    }
}
