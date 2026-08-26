using Simando.Domain.Directory;
using Simando.Domain.Workflow;

namespace Simando.Application.Directory;

public sealed record CompanyListItem(
    Guid Id,
    string Nomor,
    string NamaPerusahaan,
    string IndustryTypeName,
    string LocationLabel,
    byte CurrentStage,
    RecordStatus Status,
    // Plotting (stage 2) fields — null until a Plotting row exists. Directory
    // and Plotting are views of the same record, not separate tables
    // (docs/domain/03-directory-plotting.md), so one list query serves both.
    Guid? SalesUserId,
    string? SalesUserName,
    PosisiPelanggan? PosisiPelanggan,
    Kawasan? Kawasan,
    // Null only for a company with no pin dropped yet — Location is optional
    // on Company but pin-drop is mandatory from /directory/new onward, so in
    // practice this is only null for rows seeded directly (tests).
    double? Latitude,
    double? Longitude);
