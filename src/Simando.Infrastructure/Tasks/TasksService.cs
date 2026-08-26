using Microsoft.EntityFrameworkCore;
using Simando.Application.Common;
using Simando.Application.Tasks;
using Simando.Domain.Security;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Persistence;
using Simando.Infrastructure.Workflow;

namespace Simando.Infrastructure.Tasks;

// Fresh-context-per-call, same shape as WorkflowService/OrganisationService.
// Read-only — no SaveChangesAsync anywhere in this file.
internal sealed class TasksService(IDbContextFactory<SimandoDbContext> dbContextFactory) : ITasksService
{
    public async Task<IReadOnlyList<TaskListItem>> GetMyTasksAsync(
        Guid actorUserId, EffectivePermissions actor, IReadOnlySet<Role> actorRoles, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var rows = await ActiveWorkflowQuery.LoadAsync(db, ct);
        var submitterNames = await LoadSubmitterNamesAsync(db, rows, ct);

        return rows
            .Where(r => WorkflowStepAssignment.IsAssignedToStep(r.CurrentStep, actorUserId, actorRoles))
            .Where(r => PermissionEvaluator.CanViewRecord(actor, r.Company.AreaId, r.Area.RegionId))
            .Select(r => ToTaskListItem(r, submitterNames))
            .ToList();
    }

    public async Task<IReadOnlyList<TaskListItem>> GetRegionTasksAsync(EffectivePermissions actor, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var rows = await ActiveWorkflowQuery.LoadAsync(db, ct);
        var submitterNames = await LoadSubmitterNamesAsync(db, rows, ct);

        return rows
            .Where(r => PermissionEvaluator.CanViewRecord(actor, r.Company.AreaId, r.Area.RegionId))
            .Select(r => ToTaskListItem(r, submitterNames))
            .ToList();
    }

    public async Task<IReadOnlyList<TaskListItem>> GetBlockedTasksAsync(EffectivePermissions actor, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var rows = await ActiveWorkflowQuery.LoadAsync(db, ct);
        var submitterNames = await LoadSubmitterNamesAsync(db, rows, ct);

        var threshold = DateTimeOffset.UtcNow.AddDays(-7);

        var activeBlocked = rows
            .Where(r => PermissionEvaluator.CanViewRecord(actor, r.Company.AreaId, r.Area.RegionId))
            .Where(r => r.WaitingSince <= threshold || r.CurrentStep.AssignedUserId == null)
            .Select(r => ToTaskListItem(r, submitterNames))
            .ToList();

        var rejectedCompanies = await db.Companies.AsNoTracking()
            .Where(c => c.Status == RecordStatus.Rejected)
            .ToListAsync(ct);

        var rejectedCompanyItems = new List<TaskListItem>();
        if (rejectedCompanies.Count > 0)
        {
            var areaIds = rejectedCompanies.Select(c => c.AreaId).Distinct().ToList();
            var areas = await db.Areas.AsNoTracking()
                .Where(a => areaIds.Contains(a.Id))
                .ToListAsync(ct);
            var areaMap = areas.ToDictionary(a => a.Id);

            var regionIds = areas.Select(a => a.RegionId).Distinct().ToList();
            var regions = await db.Regions.AsNoTracking()
                .Where(r => regionIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, r => r.Name, ct);

            var industryTypeIds = rejectedCompanies.Select(c => c.IndustryTypeId).Distinct().ToList();
            var industryTypeMap = await db.IndustryTypes.AsNoTracking()
                .Where(i => industryTypeIds.Contains(i.Id))
                .ToDictionaryAsync(i => i.Id, i => i.Name, ct);

            var creatorIds = rejectedCompanies.Select(c => c.CreatedBy).ToHashSet();
            var rejectedSubmitters = await db.Users.AsNoTracking()
                .Where(u => creatorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName, ct);

            foreach (var c in rejectedCompanies)
            {
                if (areaMap.TryGetValue(c.AreaId, out var area) &&
                    PermissionEvaluator.CanViewRecord(actor, c.AreaId, area.RegionId))
                {
                    var regionName = regions.GetValueOrDefault(area.RegionId, "");
                    var industryName = industryTypeMap.GetValueOrDefault(c.IndustryTypeId, "");

                    rejectedCompanyItems.Add(new TaskListItem(
                        c.Id,
                        c.Nomor,
                        c.NamaPerusahaan,
                        industryName,
                        Guid.Empty,
                        WorkflowStepKind.RegionalAdmin,
                        area.Name,
                        regionName,
                        rejectedSubmitters.GetValueOrDefault(c.CreatedBy, ""),
                        c.UpdatedAt ?? c.CreatedAt));
                }
            }
        }

        return activeBlocked.Concat(rejectedCompanyItems).ToList();
    }

    public async Task<IReadOnlyList<TaskHistoryItem>> GetHistoryAsync(Guid actorUserId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var events = await db.StatusEvents.AsNoTracking()
            .Where(e => e.ActorId == actorUserId && e.WorkflowStepId != null)
            .OrderByDescending(e => e.OccurredAt)
            .ToListAsync(ct);

        var companyIds = events.Select(e => e.CompanyId).ToHashSet();
        var companies = await db.Companies.AsNoTracking()
            .Where(c => companyIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, ct);

        return events
            .Where(e => companies.ContainsKey(e.CompanyId))
            .Select(e =>
            {
                var company = companies[e.CompanyId];
                return new TaskHistoryItem(company.Id, company.Nomor, company.NamaPerusahaan, e.Action, e.ToStatus, e.Comment, e.OccurredAt);
            })
            .ToList();
    }

    public async Task<PagedResult<TaskHistoryItem>> GetPagedHistoryAsync(Guid actorUserId, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var query = db.StatusEvents.AsNoTracking()
            .Where(e => e.ActorId == actorUserId && e.WorkflowStepId != null);

        var totalCount = await query.CountAsync(ct);

        var events = await query
            .OrderByDescending(e => e.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var companyIds = events.Select(e => e.CompanyId).ToHashSet();
        var companies = await db.Companies.AsNoTracking()
            .Where(c => companyIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, ct);

        var items = events
            .Where(e => companies.ContainsKey(e.CompanyId))
            .Select(e =>
            {
                var company = companies[e.CompanyId];
                return new TaskHistoryItem(company.Id, company.Nomor, company.NamaPerusahaan, e.Action, e.ToStatus, e.Comment, e.OccurredAt);
            })
            .ToList();

        return new PagedResult<TaskHistoryItem>(items, totalCount, page, pageSize);
    }

    private static TaskListItem ToTaskListItem(ActiveWorkflowRow r, IReadOnlyDictionary<Guid, string> submitterNames) => new(
        r.Company.Id, r.Company.Nomor, r.Company.NamaPerusahaan, r.IndustryTypeName,
        r.CurrentStep.Id, r.CurrentStep.Kind, r.Area.Name, r.Region.Name,
        submitterNames.GetValueOrDefault(r.Company.CreatedBy, ""), r.WaitingSince);

    private static async Task<Dictionary<Guid, string>> LoadSubmitterNamesAsync(
        SimandoDbContext db, IEnumerable<ActiveWorkflowRow> rows, CancellationToken ct)
    {
        var creatorIds = rows.Select(r => r.Company.CreatedBy).ToHashSet();
        return await db.Users.AsNoTracking()
            .Where(u => creatorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, ct);
    }
}
