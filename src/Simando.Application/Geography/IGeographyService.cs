namespace Simando.Application.Geography;

// Cascading Provinsi -> Kota/Kab -> Kecamatan -> Kel/Desa lookups backing
// every region filter and the create-company form's LOKASI section.
public interface IGeographyService
{
    Task<IReadOnlyList<GeographyOption>> GetProvincesAsync(CancellationToken ct = default);

    Task<IReadOnlyList<GeographyOption>> GetRegenciesAsync(Guid provinceId, CancellationToken ct = default);

    Task<IReadOnlyList<GeographyOption>> GetDistrictsAsync(Guid regencyId, CancellationToken ct = default);

    Task<IReadOnlyList<GeographyOption>> GetVillagesAsync(Guid districtId, CancellationToken ct = default);
}
