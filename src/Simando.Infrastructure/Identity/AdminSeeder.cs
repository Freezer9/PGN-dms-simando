using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;

namespace Simando.Infrastructure.Identity;

// docs/design/roles-permissions.md §2.6 "Bootstrapping the first admin":
// with no directory to authenticate against, the first System Admin cannot
// be created through the app by anyone — it has to be seeded. Invoked by
// Simando.Web's `seed-admin` CLI command, never auto-run at startup.
// Idempotent: a second run after a real admin exists just no-ops, matching
// the go-live checklist item "seed account deactivated once a real System
// Admin exists" — that only makes sense if running this again afterward is
// safe rather than something that needs to be remembered to avoid.
public sealed class AdminSeeder(SimandoDbContext db, UserManager<ApplicationUser> userManager)
{
    public async Task<AdminSeedResult> SeedAsync(
        string username,
        string password,
        string fullName,
        string email,
        CancellationToken cancellationToken = default)
    {
        var hasActiveSystemAdmin = await db.RoleAssignments
            .AnyAsync(ra => ra.Role == Role.SystemAdmin && ra.Active, cancellationToken);

        if (hasActiveSystemAdmin)
        {
            return AdminSeedResult.AlreadyExists();
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = username,
            Email = email,
            FullName = fullName,
            MustChangePassword = true,
            Active = true,
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            return AdminSeedResult.Failed(createResult.Errors.Select(e => e.Description).ToArray());
        }

        db.RoleAssignments.Add(new RoleAssignment
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Role = Role.SystemAdmin,
            AreaId = null,
            RegionId = null,
            Active = true,
            AssignedBy = user.Id, // self-assigned at bootstrap -- nobody else exists yet
            AssignedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(cancellationToken);

        return AdminSeedResult.Created();
    }
}
