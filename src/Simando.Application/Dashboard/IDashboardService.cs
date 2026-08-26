using Simando.Domain.Audit;
using Simando.Domain.Directory;
using Simando.Domain.Security;

namespace Simando.Application.Dashboard;

public sealed record ReturnedWorkItem(
    Guid CompanyId,
    string CompanyNomor,
    string CompanyName,
    StatusEventAction Action,
    string ReturnReason,
    string ActorRoleLabel,
    DateTimeOffset ReturnedAt
);

public sealed record ActiveApprovalItem(
    Guid CompanyId,
    string CompanyNomor,
    string CompanyName,
    byte CurrentStage,
    string HolderLabel,
    DateTimeOffset SubmittedAt
);

public sealed record SalesAreaDashboardDto(
    IReadOnlyList<ReturnedWorkItem> ReturnedWorkItems,
    IReadOnlyDictionary<byte, int> StageCounts,
    IReadOnlyList<ActiveApprovalItem> ActiveApprovalItems
);

public sealed record PendingApprovalItem(
    Guid CompanyId,
    string CompanyNomor,
    string CompanyName,
    byte Stage,
    string SubmittedByName,
    DateTimeOffset WaitingSince
);

public sealed record ApproverPerformanceDto(
    double AverageTurnaroundDays,
    int ApprovedThisMonth,
    int RevisedThisMonth
);

public sealed record AreaActivityItem(
    DateTimeOffset OccurredAt,
    string ActorName,
    StatusEventAction Action,
    string CompanyName,
    string NextHolderLabel
);

public sealed record ApproverDashboardDto(
    IReadOnlyList<PendingApprovalItem> PendingApprovals,
    int TotalActiveRecords,
    int TotalPendingApprovals,
    int NolIssuedThisMonth,
    ApproverPerformanceDto Performance,
    IReadOnlyList<AreaActivityItem> RecentActivity
);

public sealed record StuckTaskItem(
    Guid CompanyId,
    string CompanyNomor,
    string CompanyName,
    string Reason,
    int WaitingDays
);

public sealed record AgeingSummaryItem(
    Guid CompanyId,
    string CompanyNomor,
    string CompanyName,
    int WaitingDays
);

public sealed record RegionalAdminDashboardDto(
    IReadOnlyList<StuckTaskItem> StuckTasks,
    int PendingMyActionCount,
    IReadOnlyDictionary<byte, int> RegionFunnelCounts,
    int TotalWaitingActionCount,
    AgeingSummaryItem? OldestWaitingItem
);

public sealed record MasterDataHealthItem(
    string Key,
    string Title,
    bool IsHealthy,
    string Description
);

public sealed record SystemAdminDashboardDto(
    IReadOnlyList<MasterDataHealthItem> HealthItems,
    int ActiveUsersCount,
    int ActiveRegionsCount,
    int ActiveAreasCount,
    int DocumentTemplatesCount
);

public interface IDashboardService
{
    Task<SalesAreaDashboardDto> GetSalesAreaDashboardAsync(Guid areaId, CancellationToken ct = default);
    Task<ApproverDashboardDto> GetApproverDashboardAsync(Guid userId, EffectivePermissions actor, IReadOnlySet<Role> roles, CancellationToken ct = default);
    Task<RegionalAdminDashboardDto> GetRegionalAdminDashboardAsync(Guid regionId, EffectivePermissions actor, CancellationToken ct = default);
    Task<SystemAdminDashboardDto> GetSystemAdminDashboardAsync(CancellationToken ct = default);
}
