using Simando.Domain.Survey;

namespace Simando.Application.Directory;

// Same convention as CompanyLabels — single-flag labels, used for both
// checkbox item text and read-only display.
public static class SurveyLabels
{
    public static string AsalLabel(Asal value) => value switch
    {
        Asal.Impor => "Impor",
        Asal.Lokal => "Lokal",
        _ => "-",
    };

    public static string KebutuhanEnergiLabel(KebutuhanEnergiJenis value) => value switch
    {
        KebutuhanEnergiJenis.Listrik => "Listrik",
        KebutuhanEnergiJenis.Steam => "Steam",
        KebutuhanEnergiJenis.Panas => "Panas",
        KebutuhanEnergiJenis.Dingin => "Dingin",
        KebutuhanEnergiJenis.Lainnya => "Lainnya",
        _ => "-",
    };

    public static string BahanBakarLabel(BahanBakarEksisting value) => value switch
    {
        BahanBakarEksisting.Lpg => "LPG",
        BahanBakarEksisting.Hsd => "HSD (Solar)",
        BahanBakarEksisting.Mfo => "MFO",
        BahanBakarEksisting.Cng => "CNG",
        BahanBakarEksisting.Lainnya => "Lainnya",
        _ => "-",
    };

    public static string RencanaPemanfaatanGasLabel(RencanaPemanfaatanGas value) => value switch
    {
        RencanaPemanfaatanGas.BahanBaku => "Bahan Baku",
        RencanaPemanfaatanGas.BahanBakar => "Bahan Bakar",
        RencanaPemanfaatanGas.PembangkitListrik => "Pembangkit Listrik",
        RencanaPemanfaatanGas.Cng => "CNG",
        RencanaPemanfaatanGas.TransportasiGas => "Transportasi Gas",
        RencanaPemanfaatanGas.Lainnya => "Lainnya",
        _ => "-",
    };
}
