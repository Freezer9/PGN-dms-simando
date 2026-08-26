namespace Simando.Domain.Workflow;

public readonly record struct WorkflowTransitionResult
{
    public bool Success { get; }
    public RecordStatus? NewStatus { get; }
    public string? Error { get; }

    private WorkflowTransitionResult(bool success, RecordStatus? newStatus, string? error)
    {
        Success = success;
        NewStatus = newStatus;
        Error = error;
    }

    public static WorkflowTransitionResult Ok(RecordStatus newStatus) => new(true, newStatus, null);

    public static WorkflowTransitionResult Fail(string error) => new(false, null, error);
}
