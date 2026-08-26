namespace Simando.Application.Directory;

public sealed record SurveyEquipmentDetail(
    Guid Id,
    string JenisPeralatan,
    decimal? Kapasitas,
    Guid? KapasitasUnitId,
    decimal? JamPerHari,
    short? HariPerMinggu,
    Guid? FuelTypeId,
    decimal? HargaBahanBakar,
    decimal? KonsumsiPerBulan,
    Guid? KonsumsiUnitId,
    decimal KonversiKeGas,
    short SortOrder);

public sealed record SaveSurveyEquipmentRequest(
    string JenisPeralatan,
    decimal? Kapasitas,
    Guid? KapasitasUnitId,
    decimal? JamPerHari,
    short? HariPerMinggu,
    Guid? FuelTypeId,
    decimal? HargaBahanBakar,
    decimal? KonsumsiPerBulan,
    Guid? KonsumsiUnitId,
    decimal KonversiKeGas);
