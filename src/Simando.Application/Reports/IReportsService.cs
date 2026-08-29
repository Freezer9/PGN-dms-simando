using Simando.Domain.Security;

namespace Simando.Application.Reports;

// Read-only reporting queries backing /reports/*. Same shape as
// ITasksService — a separate interface because these never gate on turn or
// per-user assignment, only on scope + Capability.ViewAgeingReport.
public interface IReportsService
{
    Task<IReadOnlyList<AgeingRow>> GetAgeingAsync(EffectivePermissions actor, CancellationToken ct = default);
    Task<FunnelReportDto> GetFunnelAsync(EffectivePermissions actor, Guid? areaId = null, Guid? regionId = null, CancellationToken ct = default);
    Task<GasDemandReportDto> GetGasDemandAsync(EffectivePermissions actor, CancellationToken ct = default);
    Task<SurveyProductivityReportDto> GetSurveyProductivityAsync(EffectivePermissions actor, int? year = null, CancellationToken ct = default);
    Task<NolOutcomesReportDto> GetNolOutcomesAsync(EffectivePermissions actor, CancellationToken ct = default);
    Task<IReadOnlyList<CompanyDirectoryRow>> GetCompanyDirectoryRowsAsync(CancellationToken ct = default);
}
