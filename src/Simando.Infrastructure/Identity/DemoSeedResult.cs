using Simando.Domain.Security;

namespace Simando.Infrastructure.Identity;

public readonly record struct DemoSeedResult
{
    public IReadOnlyList<string> Errors { get; }
    public IReadOnlyList<DemoSeedAccount> Accounts { get; }

    private DemoSeedResult(IReadOnlyList<DemoSeedAccount> accounts, IReadOnlyList<string> errors)
    {
        Accounts = accounts;
        Errors = errors;
    }

    public static DemoSeedResult Completed(IReadOnlyList<DemoSeedAccount> accounts) => new(accounts, []);

    public static DemoSeedResult Failed(IReadOnlyList<string> errors) => new([], errors);
}

public sealed record DemoSeedAccount(Role Role, string Username, bool WasCreated);
