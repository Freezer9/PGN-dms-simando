namespace Simando.Application.Directory;

public sealed record ContactDetail(
    Guid Id,
    string Nama,
    string Jabatan,
    string? Email,
    string? NoHp,
    string? LinkedIn,
    string? Instagram,
    string? Facebook,
    bool IsPrimary,
    short SortOrder);

public sealed record SaveContactRequest(
    string Nama,
    string Jabatan,
    string? Email,
    string? NoHp,
    string? LinkedIn,
    string? Instagram,
    string? Facebook,
    bool IsPrimary);
