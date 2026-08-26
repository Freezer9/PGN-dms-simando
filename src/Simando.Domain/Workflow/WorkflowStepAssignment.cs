using Simando.Domain.Security;

namespace Simando.Domain.Workflow;

// Turn-checking primitives, pure functions, no I/O — same reasoning as
// WorkflowTransitions and PermissionEvaluator (AGENTS.md: demonstrable
// without a database). Kept separate from WorkflowTransitions because these
// are about WHO may act on a step, not which status an action produces.
public static class WorkflowStepAssignment
{
    // RecordStatus and WorkflowStepKind are named 1:1 for every chain
    // position except Division Head's: its step kind is DivisionHead, but
    // the record status it corresponds to is Approval (see RecordStatus.cs —
    // IN_REVIEW's slots keep their own names, but the final approver's
    // status was named for what happens there, not who does it). Statuses
    // outside the active chain (Draft, Rejected, IssuedNol, IssuedRl,
    // Discontinued) have no current step.
    public static WorkflowStepKind? CurrentStepKind(RecordStatus status) => status switch
    {
        RecordStatus.AreaHead => WorkflowStepKind.AreaHead,
        RecordStatus.RegionalAdmin => WorkflowStepKind.RegionalAdmin,
        RecordStatus.Reviewer1 => WorkflowStepKind.Reviewer1,
        RecordStatus.Reviewer2 => WorkflowStepKind.Reviewer2,
        RecordStatus.Reviewer3 => WorkflowStepKind.Reviewer3,
        RecordStatus.Approval => WorkflowStepKind.DivisionHead,
        _ => null,
    };

    public static Role RequiredRole(WorkflowStepKind kind) => kind switch
    {
        WorkflowStepKind.AreaHead => Role.AreaHead,
        WorkflowStepKind.RegionalAdmin => Role.RegionalAdmin,
        WorkflowStepKind.DivisionHead => Role.DivisionHead,
        WorkflowStepKind.Reviewer1 or WorkflowStepKind.Reviewer2 or WorkflowStepKind.Reviewer3 => Role.Reviewer,
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    // Reviewer slots are gated to the specific chosen user; every other kind
    // is role-resolved — any user holding that role (in scope) may act.
    public static bool IsAssignedToStep(WorkflowStep step, Guid actorUserId, IReadOnlySet<Role> actorRoles) =>
        step.Kind is WorkflowStepKind.Reviewer1 or WorkflowStepKind.Reviewer2 or WorkflowStepKind.Reviewer3
            ? step.AssignedUserId == actorUserId
            : actorRoles.Contains(RequiredRole(step.Kind));
}
