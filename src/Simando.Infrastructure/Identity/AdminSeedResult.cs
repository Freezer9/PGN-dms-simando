namespace Simando.Infrastructure.Identity;

public enum AdminSeedOutcome
{
    Created,
    AlreadyExists,
    Failed,
}

public readonly record struct AdminSeedResult
{
    public AdminSeedOutcome Outcome { get; }
    public IReadOnlyList<string> Errors { get; }

    private AdminSeedResult(AdminSeedOutcome outcome, IReadOnlyList<string> errors)
    {
        Outcome = outcome;
        Errors = errors;
    }

    public static AdminSeedResult Created() => new(AdminSeedOutcome.Created, []);

    public static AdminSeedResult AlreadyExists() => new(AdminSeedOutcome.AlreadyExists, []);

    public static AdminSeedResult Failed(IReadOnlyList<string> errors) => new(AdminSeedOutcome.Failed, errors);
}
