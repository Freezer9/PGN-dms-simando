namespace Simando.Application.Security;

// AddRoleAssignmentAsync can be rejected by a guard rail (PermissionEvaluator.
// CanAssignRole / IsSelfRoleModification) rather than a persistence failure —
// same not-an-exception reasoning as CreateUserResult.
public readonly record struct RoleAssignmentResult
{
    public bool Succeeded { get; }
    public string? Error { get; }

    private RoleAssignmentResult(bool succeeded, string? error)
    {
        Succeeded = succeeded;
        Error = error;
    }

    public static RoleAssignmentResult Success() => new(true, null);

    public static RoleAssignmentResult Rejected(string error) => new(false, error);
}
