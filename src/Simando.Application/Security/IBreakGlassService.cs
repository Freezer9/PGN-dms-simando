using Simando.Application.Common;
using Simando.Domain.Audit;
using Simando.Domain.Security;

namespace Simando.Application.Security;

public sealed record BreakGlassAccessDto(
    Guid Id,
    Guid CompanyId,
    string CompanyNomor,
    string CompanyName,
    Guid UserId,
    string UserName,
    string Reason,
    DateTimeOffset RequestedAt,
    DateTimeOffset ExpiresAt,
    bool IsActive
);

public interface IBreakGlassService
{
    Task<bool> HasActiveAccessAsync(Guid userId, Guid companyId, CancellationToken ct = default);

    Task<BreakGlassAccessDto?> RequestAccessAsync(
        Guid companyId,
        string reason,
        Guid userId,
        EffectivePermissions actor,
        CancellationToken ct = default);

    Task<IReadOnlyList<BreakGlassAccessDto>> GetAuditLogsAsync(
        EffectivePermissions actor,
        CancellationToken ct = default);

    Task<PagedResult<BreakGlassAccessDto>> GetPagedAuditLogsAsync(
        EffectivePermissions actor,
        int page = 1,
        int pageSize = 25,
        CancellationToken ct = default);
}
