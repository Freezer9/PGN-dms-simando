namespace Simando.Infrastructure.Options;

public sealed class AuthOptions
{
    public PasswordPolicyOptions Password { get; set; } = new();
    public LockoutPolicyOptions Lockout { get; set; } = new();
    public int SessionTimeoutMinutes { get; set; } = 60;
}

public sealed class PasswordPolicyOptions
{
    public int MinLength { get; set; } = 12;
    public bool RequireMixed { get; set; } = true;
    public int HistoryCount { get; set; } = 3;
    public int ExpiryDays { get; set; } = 0;
}

public sealed class LockoutPolicyOptions
{
    public int MaxAttempts { get; set; } = 10;
    public int Minutes { get; set; } = 15;
}
