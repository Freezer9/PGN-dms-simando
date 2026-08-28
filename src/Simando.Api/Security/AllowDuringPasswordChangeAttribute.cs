namespace Simando.Api.Security;

public interface IAllowDuringPasswordChange;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AllowDuringPasswordChangeAttribute : Attribute, IAllowDuringPasswordChange;
