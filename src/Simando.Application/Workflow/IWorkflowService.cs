using Simando.Application.Security;
using Simando.Domain.Security;
using Simando.Domain.Workflow;

namespace Simando.Application.Workflow;

// Bespoke, not IEntityService<T> — WorkflowInstance/WorkflowStep aren't
// AuditableEntity (append-only-adjacent snapshots, same reasoning as
// StatusEvent). StartAsync snapshots the chain on submit, re-checking
// creator+capability+scope server-side same as the other two;
// ChooseReviewersAsync is Regional Admin's "Tetapkan Reviewer" action;
// ActAsync executes Setuju/Revisi/Tolak/TidakLayak against a step,
// re-checking scope+capability+turn server-side every time.
public interface IWorkflowService
{
    Task<SubmitResult> StartAsync(
        Guid companyId,
        Guid actorUserId,
        EffectivePermissions actor,
        IReadOnlySet<Role> actorRoles,
        CancellationToken ct = default);

    Task<RoleAssignmentResult> ChooseReviewersAsync(
        Guid workflowInstanceId,
        IReadOnlyList<Guid> reviewerUserIds,
        Guid actorUserId,
        EffectivePermissions actor,
        IReadOnlySet<Role> actorRoles,
        CancellationToken ct = default);

    Task<WorkflowActResult> ActAsync(
        Guid stepId,
        WorkflowAction action,
        string? comment,
        Guid actorUserId,
        EffectivePermissions actor,
        IReadOnlySet<Role> actorRoles,
        CancellationToken ct = default);

    Task<WorkflowActResult> ReassignStepAsync(
        Guid stepId,
        Guid newUserId,
        Guid actorUserId,
        EffectivePermissions actor,
        CancellationToken ct = default);

    Task<WorkflowActResult> ReworkAsync(
        Guid companyId,
        string? comment,
        Guid actorUserId,
        EffectivePermissions actor,
        CancellationToken ct = default);

    Task<WorkflowActResult> DiscontinueAsync(
        Guid companyId,
        string comment,
        Guid actorUserId,
        EffectivePermissions actor,
        CancellationToken ct = default);

    Task<IReadOnlyList<StuckStepItemDto>> GetStuckStepsAsync(
        EffectivePermissions actor,
        CancellationToken ct = default);
}
