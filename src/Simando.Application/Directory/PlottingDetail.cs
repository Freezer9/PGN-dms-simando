using Simando.Domain.Directory;

namespace Simando.Application.Directory;

// Nullable throughout — a stage-1-only Company has no Plotting row yet.
public sealed record PlottingDetail(
    Guid CompanyId,
    Guid? SalesUserId,
    string? SalesUserName,
    PosisiPelanggan? PosisiPelanggan,
    Kawasan? Kawasan);

public sealed record SavePlottingRequest(Guid SalesUserId, PosisiPelanggan PosisiPelanggan, Kawasan Kawasan);
