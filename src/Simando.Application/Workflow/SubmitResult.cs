namespace Simando.Application.Workflow;

// Mirrors WorkflowActResult/RoleAssignmentResult's shape — StartAsync's
// authorization checks (creator, capability, scope) are user-facing
// rejections, not exceptional failures, same reasoning as ActAsync.
public readonly record struct SubmitResult
{
    public bool Succeeded { get; }
    public string? Error { get; }
    public Guid? WorkflowInstanceId { get; }

    private SubmitResult(bool succeeded, string? error, Guid? workflowInstanceId)
    {
        Succeeded = succeeded;
        Error = error;
        WorkflowInstanceId = workflowInstanceId;
    }

    public static SubmitResult Success(Guid workflowInstanceId) => new(true, null, workflowInstanceId);

    public static SubmitResult Rejected(string error) => new(false, error, null);
}
