using Simando.Domain.Workflow;

namespace Simando.Application.Workflow;

// Mirrors CreateUserResult/RoleAssignmentResult's shape — WorkflowTransitions.Apply
// already returns a result rather than throwing for user-facing rejections
// (wrong comment, terminal state, not your turn), so ActAsync follows suit.
public readonly record struct WorkflowActResult
{
    public bool Succeeded { get; }
    public string? Error { get; }
    public RecordStatus? NewStatus { get; }

    private WorkflowActResult(bool succeeded, string? error, RecordStatus? newStatus)
    {
        Succeeded = succeeded;
        Error = error;
        NewStatus = newStatus;
    }

    public static WorkflowActResult Ok(RecordStatus newStatus) => new(true, null, newStatus);

    public static WorkflowActResult Rejected(string error) => new(false, error, null);
}
