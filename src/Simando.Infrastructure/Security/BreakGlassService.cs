using Microsoft.EntityFrameworkCore;
using Simando.Application.Notifications;
using Simando.Application.Security;
using Simando.Domain.Audit;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;

namespace Simando.Infrastructure.Security;

internal sealed class BreakGlassService(
    IDbContextFactory<SimandoDbContext> dbContextFactory,
    INotificationChannel notifications) : IBreakGlassService
{
    public async Task<bool> HasActiveAccessAsync(Guid userId, Guid companyId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var now = DateTimeOffset.UtcNow;

        return await db.BreakGlassAccesses.AsNoTracking()
            .AnyAsync(b => b.UserId == userId && b.CompanyId == companyId && b.ExpiresAt > now, ct);
    }

    public async Task<BreakGlassAccessDto?> RequestAccessAsync(
        Guid companyId,
        string reason,
        Guid userId,
        EffectivePermissions actor,
        CancellationToken ct = default)
    {
        if (!actor.HasCapability(Capability.BreakGlassRecordRead))
            return null;

        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == companyId, ct);
        if (company is null) return null;

        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(60);

        var entity = new BreakGlassAccess
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            UserId = userId,
            Reason = reason,
            RequestedAt = now,
            ExpiresAt = expires
        };

        db.BreakGlassAccesses.Add(entity);

        db.StatusEvents.Add(new StatusEvent
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            ActorId = userId,
            ToStage = company.CurrentStage,
            ToStatus = company.Status,
            Action = StatusEventAction.BreakGlass,
            Comment = $"Akses Darurat (Break-Glass): {reason}",
            OccurredAt = now
        });

        await db.SaveChangesAsync(ct);

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        var userName = user?.FullName ?? "Administrator";

        // Notify Regional Admin and Division Head
        var area = await db.Areas.AsNoTracking().FirstOrDefaultAsync(a => a.Id == company.AreaId, ct);
        if (area is not null)
        {
            var adminUserIds = await db.RoleAssignments.AsNoTracking()
                .Where(a => a.Active && (a.Role == Role.RegionalAdmin || a.Role == Role.DivisionHead) && (a.RegionId == area.RegionId || a.AreaId == company.AreaId))
                .Select(a => a.UserId)
                .ToListAsync(ct);

            foreach (var adminId in adminUserIds)
            {
                await notifications.SendAsync(adminId, companyId, $"[AKSES DARURAT] {userName} menggunakan break-glass untuk melihat {company.NamaPerusahaan}. Alasan: {reason}", ct);
            }
        }

        return new BreakGlassAccessDto(
            entity.Id,
            companyId,
            company.Nomor,
            company.NamaPerusahaan,
            userId,
            userName,
            reason,
            now,
            expires,
            true
        );
    }

    public async Task<IReadOnlyList<BreakGlassAccessDto>> GetAuditLogsAsync(EffectivePermissions actor, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var accesses = await db.BreakGlassAccesses.AsNoTracking()
            .OrderByDescending(b => b.RequestedAt)
            .Take(100)
            .ToListAsync(ct);

        var companyIds = accesses.Select(a => a.CompanyId).ToHashSet();
        var userIds = accesses.Select(a => a.UserId).ToHashSet();

        var companies = await db.Companies.IgnoreQueryFilters().AsNoTracking()
            .Where(c => companyIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, ct);

        var users = await db.Users.AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, ct);

        var now = DateTimeOffset.UtcNow;

        return accesses.Select(a =>
        {
            var comp = companies.GetValueOrDefault(a.CompanyId);
            return new BreakGlassAccessDto(
                a.Id,
                a.CompanyId,
                comp?.Nomor ?? "-",
                comp?.NamaPerusahaan ?? "Perusahaan",
                a.UserId,
                users.GetValueOrDefault(a.UserId, "User"),
                a.Reason,
                a.RequestedAt,
                a.ExpiresAt,
                a.ExpiresAt > now
            );
        }).ToList();
    }

    public async Task<Simando.Application.Common.PagedResult<BreakGlassAccessDto>> GetPagedAuditLogsAsync(
        EffectivePermissions actor, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var query = db.BreakGlassAccesses.AsNoTracking();
        var totalCount = await query.CountAsync(ct);

        var accesses = await query
            .OrderByDescending(b => b.RequestedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var companyIds = accesses.Select(a => a.CompanyId).ToHashSet();
        var userIds = accesses.Select(a => a.UserId).ToHashSet();

        var companies = await db.Companies.IgnoreQueryFilters().AsNoTracking()
            .Where(c => companyIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, ct);

        var users = await db.Users.AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, ct);

        var now = DateTimeOffset.UtcNow;

        var items = accesses.Select(a =>
        {
            var comp = companies.GetValueOrDefault(a.CompanyId);
            return new BreakGlassAccessDto(
                a.Id,
                a.CompanyId,
                comp?.Nomor ?? "-",
                comp?.NamaPerusahaan ?? "Perusahaan",
                a.UserId,
                users.GetValueOrDefault(a.UserId, "User"),
                a.Reason,
                a.RequestedAt,
                a.ExpiresAt,
                a.ExpiresAt > now
            );
        }).ToList();

        return new Simando.Application.Common.PagedResult<BreakGlassAccessDto>(items, totalCount, page, pageSize);
    }
}
