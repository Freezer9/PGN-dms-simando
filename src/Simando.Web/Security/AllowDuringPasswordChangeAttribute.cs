namespace Simando.Web.Security;

// Declared on endpoints that MustChangePasswordMiddleware must let through even
// when must_change_password is set: the change-password page/action itself, and
// sign-out (a locked-out user must still be able to leave). Read from endpoint
// metadata the same way the framework's own IAllowAnonymous is.
public interface IAllowDuringPasswordChange;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AllowDuringPasswordChangeAttribute : Attribute, IAllowDuringPasswordChange;
