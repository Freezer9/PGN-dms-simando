using Simando.Domain.Directory;
using Simando.Domain.Security;
using Simando.Domain.Survey;
using Simando.Domain.Workflow;

namespace Simando.Application.Directory;

public sealed record CompanyMapPinDto(
    Guid Id,
    string Nomor,
    string NamaPerusahaan,
    double Latitude,
    double Longitude,
    byte CurrentStage,
    RecordStatus Status,
    string IndustryTypeName,
    string LocationLabel,
    PosisiPelanggan? PosisiPelanggan,
    Kawasan? Kawasan,
    string? SalesUserName,
    Guid? ProvinceId = null,
    string? ProvinceName = null,
    Guid? RegencyId = null,
    string? RegencyName = null,
    Guid? DistrictId = null,
    string? DistrictName = null,
    Guid? VillageId = null,
    string? VillageName = null);

public sealed record UpdateLocationRequest(double Latitude, double Longitude);

public sealed record UpdateCompanyRequest(
    string NamaPerusahaan,
    string? Website,
    Guid VillageId,
    string Alamat,
    double Latitude,
    double Longitude,
    Guid IndustryTypeId,
    string? Email,
    string? KodePos,
    string? Telp,
    string? Npwp);

public sealed record ChooseReviewersRequest(IReadOnlyList<Guid> ReviewerUserIds);

public sealed record ReworkRequest(string? Comment);

public sealed record DiscontinueRequest(string Comment);

public sealed record CompanyRecordDto(
    Guid Id,
    string Nomor,
    string NamaPerusahaan,
    string? Website,
    string Alamat,
    Guid VillageId,
    string VillageName,
    Guid DistrictId,
    string DistrictName,
    Guid RegencyId,
    string RegencyName,
    Guid ProvinceId,
    string ProvinceName,
    string LocationLabel,
    Guid IndustryTypeId,
    string IndustryTypeName,
    string? Npwp,
    string? Email,
    string? KodePos,
    string? Telp,
    Guid AreaId,
    string AreaName,
    Guid RegionId,
    string RegionName,
    byte CurrentStage,
    RecordStatus Status,
    Guid CreatedBy,
    string SalesRepName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    double? Latitude,
    double? Longitude,
    string? HolderLabel,
    string? HolderName,
    DateTimeOffset StatusSince,
    Guid? CurrentStepId,
    WorkflowStepKind? CurrentStepKind,
    Guid? WorkflowInstanceId,
    bool CanSubmit,
    bool CanAct,
    bool CanChooseReviewers,
    IReadOnlyList<ContactDetail> Contacts);
