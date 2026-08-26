using Simando.Domain.Directory;

namespace Simando.Application.Directory;

// Shared between Directory.razor, CompanyHub.razor and Plotting.razor —
// extracted once a second consumer needed the same label logic, same
// convention as WorkflowLabels.
public static class CompanyLabels
{
    public static string TruncatedNomor(string nomor) => nomor.Length > 4 ? $"…{nomor[4..]}" : nomor;

    public static string PosisiPelangganLabel(PosisiPelanggan? value) => value switch
    {
        PosisiPelanggan.Pengembangan => "Pengembangan",
        PosisiPelanggan.JalurExisting => "Jalur Existing",
        _ => "-",
    };

    public static string KawasanLabel(Kawasan? value) => value switch
    {
        Kawasan.KawasanIndustri => "Kawasan Industri",
        Kawasan.NonKawasanIndustri => "Non Kawasan Industri",
        _ => "-",
    };

    public static readonly string[] StageLabels =
        ["Direktori", "Plotting", "Prospek", "Survei", "A1", "Permohonan", "Evaluasi", "Persetujuan"];

    public static string StageLabel(byte stage) => stage is >= 1 and <= 8 ? StageLabels[stage - 1] : "-";

    public static string FormattedStageLabel(byte stage) => stage is >= 1 and <= 8 ? $"{stage}. {StageLabels[stage - 1]}" : "-";

    public static string StageDotColor(byte stage) => stage switch
    {
        1 => "#94a3b8", // Direktori - Slate
        2 => "#60a5fa", // Plotting - Blue
        3 => "#38bdf8", // Prospek - Sky
        4 => "#34d399", // Survei - Emerald Green
        5 => "#fbbf24", // A1 - Amber
        6 => "#fb923c", // Permohonan NOL - Orange
        7 or 8 => "#22c55e", // Evaluasi / Persetujuan / NOL - Green
        _ => "#94a3b8",
    };

    public static string StageBadgeStyle(byte stage) => stage switch
    {
        1 => "background-color: #f1f5f9; color: #334155; border-color: #cbd5e1;",
        2 => "background-color: #eff6ff; color: #1d4ed8; border-color: #bfdbfe;",
        3 => "background-color: #f0f9ff; color: #0369a1; border-color: #bae6fd;",
        4 => "background-color: #ecfdf5; color: #047857; border-color: #a7f3d0;",
        5 => "background-color: #fffbeb; color: #b45309; border-color: #fde68a;",
        6 => "background-color: #fff7ed; color: #c2410c; border-color: #fed7aa;",
        7 or 8 => "background-color: #f0fdf4; color: #15803d; border-color: #bbf7d0;",
        _ => "background-color: #f8fafc; color: #64748b; border-color: #e2e8f0;",
    };
}
