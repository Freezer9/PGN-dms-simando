using Simando.Domain.Directory;

namespace Simando.Application.Directory;

// Every filter is optional ("Semua"). Scope is not a filter here — the
// DbContext's own row-level-security query filter on Company already
// restricts the result set to what the signed-in user may see.
public sealed record CompanyListFilter(
    Guid? ProvinceId = null,
    Guid? RegencyId = null,
    Guid? DistrictId = null,
    Guid? VillageId = null,
    Guid? IndustryTypeId = null,
    byte? Stage = null,
    PosisiPelanggan? PosisiPelanggan = null,
    Kawasan? Kawasan = null,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 25);
