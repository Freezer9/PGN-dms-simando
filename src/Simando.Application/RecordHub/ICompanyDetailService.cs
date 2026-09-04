using Simando.Application.Directory;
using Simando.Domain.Security;

namespace Simando.Application.RecordHub;

// Backs /companies/{id}. Read-only — GetDetailAsync returns null when the
// company doesn't exist (404, not 403); the page itself applies the scope
// gate (PermissionEvaluator.CanViewRecord) once it has AreaId/RegionId, same
// as every other capability+scope gate in Simando.Web.
public interface ICompanyDetailService
{
    Task<CompanyDetail?> GetDetailAsync(
        Guid companyId,
        Guid actorUserId,
        EffectivePermissions actor,
        IReadOnlySet<Role> actorRoles,
        CancellationToken ct = default);

    Task<CompanyRecordDto?> GetCompanyRecordAsync(
        Guid companyId,
        Guid actorUserId,
        EffectivePermissions actor,
        IReadOnlySet<Role> actorRoles,
        CancellationToken ct = default);

    Task<IReadOnlyList<TimelineEntry>> GetTimelineAsync(Guid companyId, bool isBreakGlass = false, CancellationToken ct = default);
}
