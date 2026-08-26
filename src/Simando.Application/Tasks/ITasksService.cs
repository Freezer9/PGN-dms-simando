using Simando.Application.Common;
using Simando.Domain.Security;

namespace Simando.Application.Tasks;

// Read-only cross-entity queries backing /tasks — not part of IWorkflowService
// since these never mutate; joins Company + WorkflowStep + Area/Region, which
// nothing else in the codebase does yet.
public interface ITasksService
{
    Task<IReadOnlyList<TaskListItem>> GetMyTasksAsync(
        Guid actorUserId, EffectivePermissions actor, IReadOnlySet<Role> actorRoles, CancellationToken ct = default);

    Task<IReadOnlyList<TaskListItem>> GetRegionTasksAsync(
        EffectivePermissions actor, CancellationToken ct = default);

    Task<IReadOnlyList<TaskListItem>> GetBlockedTasksAsync(
        EffectivePermissions actor, CancellationToken ct = default);

    Task<IReadOnlyList<TaskHistoryItem>> GetHistoryAsync(
        Guid actorUserId, CancellationToken ct = default);

    Task<PagedResult<TaskHistoryItem>> GetPagedHistoryAsync(
        Guid actorUserId, int page = 1, int pageSize = 25, CancellationToken ct = default);
}
