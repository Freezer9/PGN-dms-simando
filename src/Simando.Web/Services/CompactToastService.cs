using BlazorBlueprint.Components;

namespace Simando.Web.Services;

/// <summary>
/// Custom ToastService that automatically sets all toast notifications to compact mode (ToastSize.Compact).
/// </summary>
public class CompactToastService : ToastService
{
    public CompactToastService()
    {
        OnChange += EnforceCompactSize;
    }

    private void EnforceCompactSize()
    {
        foreach (var toast in Toasts)
        {
            toast.Size = ToastSize.Compact;
        }
    }
}
