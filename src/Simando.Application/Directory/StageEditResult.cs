namespace Simando.Application.Directory;

// Same Success()/Rejected(error) factory shape as SoftDeleteResult — covers
// every stage 1-3 write (Plotting save, Prospek promote, contact CRUD).
public readonly record struct StageEditResult
{
    public bool Succeeded { get; }
    public string? Error { get; }

    private StageEditResult(bool succeeded, string? error)
    {
        Succeeded = succeeded;
        Error = error;
    }

    public static StageEditResult Success() => new(true, null);
    public static StageEditResult Rejected(string error) => new(false, error);
}
