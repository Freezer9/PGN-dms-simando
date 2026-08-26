namespace Simando.Application.Directory;

public sealed record CreateCompanyRequest(
    string NamaPerusahaan,
    string? Website,
    Guid VillageId,
    string Alamat,
    double Latitude,
    double Longitude,
    Guid IndustryTypeId,
    Guid AreaId,
    string? Email,
    string? KodePos,
    string? Telp,
    string? Npwp);

public sealed record CreateCompanyResult(Guid CompanyId, string Nomor);

public readonly record struct SoftDeleteResult
{
    public bool Succeeded { get; }
    public string? Error { get; }

    private SoftDeleteResult(bool succeeded, string? error)
    {
        Succeeded = succeeded;
        Error = error;
    }

    public static SoftDeleteResult Success() => new(true, null);
    public static SoftDeleteResult Rejected(string error) => new(false, error);
}
