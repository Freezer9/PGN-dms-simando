using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;

namespace Simando.Infrastructure.Identity;

// Seeds one demo/test account per non-SystemAdmin role, plus the minimal
// Region/Area org data those roles' scopes require. SystemAdmin is
// deliberately excluded -- that role is seed-admin's job (AdminSeeder), and
// recreating it here would either collide with or duplicate its "one active
// SysAdmin" idempotency check for no benefit. Invoked by Simando.Web's
// `seed-demo-users` CLI command; never auto-run at startup, same as
// AdminSeeder. Idempotent by design (find-or-create on Region/Area Code and
// user UserName), so it's safe to rerun after a partial failure or DB reset.
public sealed class DemoUserSeeder(SimandoDbContext db, UserManager<ApplicationUser> userManager)
{
    private static readonly (Role Role, string Username, string FullName, bool AreaScoped)[] DemoAccounts =
    [
        (Role.SalesArea, "demo.salesarea", "Demo Sales Area", true),
        (Role.AreaHead, "demo.areahead", "Demo Area Head", true),
        (Role.RegionalAdmin, "demo.regionaladmin", "Demo Regional Admin", false),
        (Role.Reviewer, "demo.reviewer", "Demo Reviewer", false),
        (Role.DivisionHead, "demo.divisionhead", "Demo Division Head", false),
    ];

    public async Task<DemoSeedResult> SeedAsync(string password, CancellationToken ct = default)
    {
        var region = await db.Regions.FirstOrDefaultAsync(r => r.Code == "DEMO", ct);
        if (region is null)
        {
            region = new Region { Id = Guid.NewGuid(), Code = "DEMO", Name = "Demo Region", Active = true };
            db.Regions.Add(region);
            await db.SaveChangesAsync(ct);
        }

        var area = await db.Areas.FirstOrDefaultAsync(a => a.Code == "DEMO" && a.RegionId == region.Id, ct);
        if (area is null)
        {
            area = new Area { Id = Guid.NewGuid(), RegionId = region.Id, Code = "DEMO", Name = "Demo Area", Active = true };
            db.Areas.Add(area);
            await db.SaveChangesAsync(ct);
        }

        var accounts = new List<DemoSeedAccount>();
        foreach (var (role, username, fullName, areaScoped) in DemoAccounts)
        {
            var existing = await db.Users.FirstOrDefaultAsync(u => u.UserName == username, ct);
            if (existing is not null)
            {
                accounts.Add(new DemoSeedAccount(role, username, WasCreated: false));
                continue;
            }

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = username,
                // Deliberately not @pgn.co.id -- these should never read as a
                // real PGN staff address in a list or export.
                Email = $"{username}@simando.local",
                FullName = fullName,
                // Shared demo password, on purpose: forcing a change on first
                // login would mean the second person to sign in with the
                // printed password gets locked out by a change only the
                // first person knows about, defeating the shared-password
                // point of a demo account.
                MustChangePassword = false,
                Active = true,
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                return DemoSeedResult.Failed(createResult.Errors.Select(e => e.Description).ToArray());
            }

            db.RoleAssignments.Add(new RoleAssignment
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Role = role,
                AreaId = areaScoped ? area.Id : null,
                RegionId = areaScoped ? null : region.Id,
                Active = true,
                AssignedBy = user.Id, // no real actor in a seed context, same as AdminSeeder's bootstrap admin
                AssignedAt = DateTimeOffset.UtcNow,
            });
            await db.SaveChangesAsync(ct);

            accounts.Add(new DemoSeedAccount(role, username, WasCreated: true));
        }

        return DemoSeedResult.Completed(accounts);
    }
}
