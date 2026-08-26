using Microsoft.AspNetCore.Identity;

namespace Simando.Infrastructure.Identity;

// No formal `user` table exists in docs/design/data-model.md — these
// extra fields are synthesized from prose scattered through
// docs/design/roles-permissions.md §5, not a sourced schema table.
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public required string FullName { get; set; }

    // Forced to /change-password on first sign-in after creation or an
    // admin-issued reset — docs/design/roles-permissions.md §5 "The
    // account lifecycle".
    public bool MustChangePassword { get; set; } = true;

    // Deliberately separate from Identity's own LockoutEnd: lockout is
    // auto-expiring (failed sign-in attempts), this is a permanent
    // administrative flag until an admin flips it back — "Admin sets
    // active = false. Sessions are terminated" (§5 "The account lifecycle").
    public bool Active { get; set; } = true;

    // The SSO seam: "The user table already carries an external_id column,
    // null for now, for the eventual mapping" (§5 "Keep the SSO seam").
    public string? ExternalId { get; set; }

    // "Surface last_login_at in the user list so dormant accounts are
    // visible" (§5 "Risks worth stating plainly").
    public DateTimeOffset? LastLoginAt { get; set; }
}
