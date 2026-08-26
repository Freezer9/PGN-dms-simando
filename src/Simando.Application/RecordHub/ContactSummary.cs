namespace Simando.Application.RecordHub;

// Read-only projection of CompanyContact for the Ringkasan tab — full
// contact CRUD lives on the Prospek tab (web-record-hub-plotting-contacts-tabs),
// not built here.
public sealed record ContactSummary(
    Guid Id,
    string Nama,
    string Jabatan,
    bool IsPrimary,
    string? Email,
    string? NoHp);
