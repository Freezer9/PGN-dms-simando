namespace Simando.Application.Security;

// Mirrors AdminSeedResult's shape (Simando.Infrastructure.Identity) — Identity's
// CreateAsync returns IdentityResult.Errors (e.g. duplicate username), not an
// exception, so the caller-facing result follows the same success/failed split
// rather than throwing.
public readonly record struct CreateUserResult
{
    public bool Succeeded { get; }
    public IReadOnlyList<string> Errors { get; }
    public Guid UserId { get; }
    public string? TemporaryPassword { get; }

    private CreateUserResult(bool succeeded, IReadOnlyList<string> errors, Guid userId, string? temporaryPassword)
    {
        Succeeded = succeeded;
        Errors = errors;
        UserId = userId;
        TemporaryPassword = temporaryPassword;
    }

    public static CreateUserResult Success(Guid userId, string temporaryPassword) =>
        new(true, [], userId, temporaryPassword);

    public static CreateUserResult Failed(IReadOnlyList<string> errors) =>
        new(false, errors, Guid.Empty, null);
}
