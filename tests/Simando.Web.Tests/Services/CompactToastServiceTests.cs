using BlazorBlueprint.Components;
using Shouldly;
using Simando.Web.Services;

namespace Simando.Web.Tests.Services;

public class CompactToastServiceTests
{
    [Fact]
    public void CompactToastService_EnforcesCompactSize_ForSuccessToasts()
    {
        var service = new CompactToastService();
        service.Success("Operasi berhasil", "Berhasil");

        service.Toasts.Count.ShouldBe(1);
        service.Toasts[0].Size.ShouldBe(ToastSize.Compact);
    }

    [Fact]
    public void CompactToastService_EnforcesCompactSize_ForErrorToasts()
    {
        var service = new CompactToastService();
        service.Error("Gagal menyimpan data", "Gagal");

        service.Toasts.Count.ShouldBe(1);
        service.Toasts[0].Size.ShouldBe(ToastSize.Compact);
    }

    [Fact]
    public void CompactToastService_EnforcesCompactSize_ForWarningToasts()
    {
        var service = new CompactToastService();
        service.Warning("Sesi akan berakhir", "Peringatan");

        service.Toasts.Count.ShouldBe(1);
        service.Toasts[0].Size.ShouldBe(ToastSize.Compact);
    }

    [Fact]
    public void CompactToastService_EnforcesCompactSize_ForInfoToasts()
    {
        var service = new CompactToastService();
        service.Info("Notifikasi baru", "Info");

        service.Toasts.Count.ShouldBe(1);
        service.Toasts[0].Size.ShouldBe(ToastSize.Compact);
    }

    [Fact]
    public void CompactToastService_EnforcesCompactSize_ForCustomToastData()
    {
        var service = new CompactToastService();
        service.Show(new ToastData
        {
            Title = "Kustom",
            Description = "Pesan kustom",
            Size = ToastSize.Default
        });

        service.Toasts.Count.ShouldBe(1);
        service.Toasts[0].Size.ShouldBe(ToastSize.Compact);
    }
}
