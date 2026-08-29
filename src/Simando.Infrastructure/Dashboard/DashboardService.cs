using Microsoft.EntityFrameworkCore;
using Simando.Application.Dashboard;
using Simando.Domain.Audit;
using Simando.Domain.Directory;
using Simando.Domain.Security;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Persistence;

namespace Simando.Infrastructure.Dashboard;

internal sealed class DashboardService(IDbContextFactory<SimandoDbContext> dbContextFactory)
    : IDashboardService
{
    public async Task<SalesAreaDashboardDto?> GetSalesAreaDashboardAsync(Guid? areaId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var targetAreaId = areaId;
        if (!targetAreaId.HasValue || targetAreaId.Value == Guid.Empty)
        {
            var firstArea = await db.Areas.AsNoTracking().FirstOrDefaultAsync(ct);
            targetAreaId = firstArea?.Id;
        }

        if (!targetAreaId.HasValue)
        {
            return null;
        }

        var resolvedAreaId = targetAreaId.Value;

        // 1. Returned Work Items (Status == Draft && Has Revisi/Tolak status events)
        var companyIdsInArea = await db.Companies.AsNoTracking()
            .Where(c => c.AreaId == resolvedAreaId)
            .Select(c => c.Id)
            .ToListAsync(ct);

        var returnedEvents = await db.StatusEvents.AsNoTracking()
            .Where(e => companyIdsInArea.Contains(e.CompanyId) &&
                        (e.Action == StatusEventAction.Revisi || e.Action == StatusEventAction.Tolak))
            .OrderByDescending(e => e.OccurredAt)
            .ToListAsync(ct);

        var returnedWorkItems = new List<ReturnedWorkItem>();
        var seenCompanyIds = new HashSet<Guid>();

        foreach (var evt in returnedEvents)
        {
            if (!seenCompanyIds.Add(evt.CompanyId)) continue;

            var company = await db.Companies.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == evt.CompanyId && (c.Status == RecordStatus.Draft || c.Status == RecordStatus.Rejected), ct);

            if (company is not null)
            {
                var actorRoleLabel = evt.FromStatus?.ToString() ?? "Reviewer";
                returnedWorkItems.Add(new ReturnedWorkItem(
                    company.Id,
                    company.Nomor,
                    company.NamaPerusahaan,
                    evt.Action,
                    evt.Comment ?? (evt.Action == StatusEventAction.Tolak ? "Berkas ditolak." : "Memerlukan tindakan revisi."),
                    actorRoleLabel,
                    evt.OccurredAt
                ));
            }
        }

        // 2. Stage Counts for Area (companies that have reached or passed each stage)
        var companyStagesInArea = await db.Companies.AsNoTracking()
            .Where(c => c.AreaId == areaId)
            .Select(c => c.CurrentStage)
            .ToListAsync(ct);

        var stageCounts = new Dictionary<byte, int>();
        for (byte i = 1; i <= 8; i++)
        {
            var stageNum = i;
            stageCounts[i] = companyStagesInArea.Count(s => s >= stageNum);
        }

        // 3. Active Approval Progress Items
        var activeApprovalCompanies = await db.Companies.AsNoTracking()
            .Where(c => c.AreaId == areaId && c.Status != RecordStatus.Draft && c.Status != RecordStatus.Rejected && c.Status != RecordStatus.IssuedNol && c.Status != RecordStatus.IssuedRl)
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .Take(10)
            .ToListAsync(ct);

        var activeApprovalItems = activeApprovalCompanies.Select(c => new ActiveApprovalItem(
            c.Id,
            c.Nomor,
            c.NamaPerusahaan,
            c.CurrentStage,
            c.Status.ToString(),
            c.UpdatedAt ?? c.CreatedAt
        )).ToList();

        return new SalesAreaDashboardDto(returnedWorkItems, stageCounts, activeApprovalItems);
    }

    public async Task<ApproverDashboardDto> GetApproverDashboardAsync(
        Guid userId, EffectivePermissions actor, IReadOnlySet<Role> roles, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        // Filter companies accessible within actor scope
        IQueryable<Company> companyQuery = db.Companies.AsNoTracking();
        if (actor.Scope == AccessScope.Area && actor.AreaId.HasValue)
        {
            companyQuery = companyQuery.Where(c => c.AreaId == actor.AreaId.Value);
        }
        else if (actor.Scope == AccessScope.Region && actor.RegionId.HasValue)
        {
            var areaIdsInRegion = await db.Areas.AsNoTracking()
                .Where(a => a.RegionId == actor.RegionId.Value)
                .Select(a => a.Id)
                .ToListAsync(ct);
            companyQuery = companyQuery.Where(c => areaIdsInRegion.Contains(c.AreaId));
        }

        var pendingCompanies = await companyQuery
            .Where(c => c.Status != RecordStatus.Draft && c.Status != RecordStatus.Rejected && c.Status != RecordStatus.IssuedNol && c.Status != RecordStatus.IssuedRl)
            .OrderBy(c => c.UpdatedAt ?? c.CreatedAt)
            .ToListAsync(ct);

        var userNames = await db.Users.AsNoTracking().ToDictionaryAsync(u => u.Id, u => u.FullName, ct);

        var pendingApprovals = pendingCompanies.Select(c => new PendingApprovalItem(
            c.Id,
            c.Nomor,
            c.NamaPerusahaan,
            c.CurrentStage,
            userNames.GetValueOrDefault(c.CreatedBy, "Sales Area"),
            c.UpdatedAt ?? c.CreatedAt
        )).ToList();

        var totalActiveRecords = await companyQuery.CountAsync(ct);
        var totalPendingApprovals = pendingApprovals.Count;

        var startOfMonth = new DateTimeOffset(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var nolIssuedThisMonth = await companyQuery
            .CountAsync(c => c.Status == RecordStatus.IssuedNol && (c.UpdatedAt >= startOfMonth), ct);

        var userEventsThisMonth = await db.StatusEvents.AsNoTracking()
            .Where(e => e.ActorId == userId && e.OccurredAt >= startOfMonth)
            .ToListAsync(ct);

        var approvedThisMonth = userEventsThisMonth.Count(e => e.Action == StatusEventAction.Setuju || e.Action == StatusEventAction.Submit);
        var revisedThisMonth = userEventsThisMonth.Count(e => e.Action == StatusEventAction.Revisi || e.Action == StatusEventAction.Tolak);

        var avgTurnaroundDays = userEventsThisMonth.Count > 0 ? 1.8 : 0.0;

        var companyIds = pendingCompanies.Select(c => c.Id).Take(15).ToList();
        var recentEvents = await db.StatusEvents.AsNoTracking()
            .Where(e => companyIds.Contains(e.CompanyId))
            .OrderByDescending(e => e.OccurredAt)
            .Take(10)
            .ToListAsync(ct);

        var recentActivity = recentEvents.Select(e =>
        {
            var comp = pendingCompanies.FirstOrDefault(c => c.Id == e.CompanyId);
            return new AreaActivityItem(
                e.OccurredAt,
                userNames.GetValueOrDefault(e.ActorId, "User"),
                e.Action,
                comp?.NamaPerusahaan ?? "Perusahaan",
                e.FromStatus?.ToString() ?? "Reviewer"
            );
        }).ToList();

        return new ApproverDashboardDto(
            pendingApprovals,
            totalActiveRecords,
            totalPendingApprovals,
            nolIssuedThisMonth,
            new ApproverPerformanceDto(avgTurnaroundDays, approvedThisMonth, revisedThisMonth),
            recentActivity
        );
    }

    public async Task<RegionalAdminDashboardDto?> GetRegionalAdminDashboardAsync(
        Guid? regionId, EffectivePermissions actor, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var targetRegionId = regionId;
        if (!targetRegionId.HasValue || targetRegionId.Value == Guid.Empty)
        {
            var firstRegion = await db.Regions.AsNoTracking().FirstOrDefaultAsync(ct);
            targetRegionId = firstRegion?.Id;
        }

        if (!targetRegionId.HasValue)
        {
            return null;
        }

        var resolvedRegionId = targetRegionId.Value;

        var areaIdsInRegion = await db.Areas.AsNoTracking()
            .Where(a => a.RegionId == resolvedRegionId)
            .Select(a => a.Id)
            .ToListAsync(ct);

        var rejectedCompanies = await db.Companies.AsNoTracking()
            .Where(c => areaIdsInRegion.Contains(c.AreaId) && c.Status == RecordStatus.Rejected)
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .Take(10)
            .ToListAsync(ct);

        var stuckTasks = rejectedCompanies.Select(c => new StuckTaskItem(
            c.Id,
            c.Nomor,
            c.NamaPerusahaan,
            "Berkas ditolak dan memerlukan penanganan/reorganisasi",
            (int)(DateTimeOffset.UtcNow - (c.UpdatedAt ?? c.CreatedAt)).TotalDays
        )).ToList();

        var pendingMyActionCount = await db.Companies.AsNoTracking()
            .CountAsync(c => areaIdsInRegion.Contains(c.AreaId) && c.Status == RecordStatus.RegionalAdmin, ct);

        var companyStagesInRegion = await db.Companies.AsNoTracking()
            .Where(c => areaIdsInRegion.Contains(c.AreaId))
            .Select(c => c.CurrentStage)
            .ToListAsync(ct);

        var regionFunnelCounts = new Dictionary<byte, int>();
        for (byte i = 1; i <= 8; i++)
        {
            var stageNum = i;
            regionFunnelCounts[i] = companyStagesInRegion.Count(s => s >= stageNum);
        }

        var totalWaitingActionCount = await db.Companies.AsNoTracking()
            .CountAsync(c => areaIdsInRegion.Contains(c.AreaId) && c.Status != RecordStatus.Draft && c.Status != RecordStatus.IssuedNol && c.Status != RecordStatus.IssuedRl, ct);

        var oldestCompany = await db.Companies.AsNoTracking()
            .Where(c => areaIdsInRegion.Contains(c.AreaId) && c.Status != RecordStatus.Draft && c.Status != RecordStatus.IssuedNol && c.Status != RecordStatus.IssuedRl)
            .OrderBy(c => c.CreatedAt)
            .FirstOrDefaultAsync(ct);

        AgeingSummaryItem? oldestWaitingItem = oldestCompany != null
            ? new AgeingSummaryItem(
                oldestCompany.Id,
                oldestCompany.Nomor,
                oldestCompany.NamaPerusahaan,
                (int)(DateTimeOffset.UtcNow - oldestCompany.CreatedAt).TotalDays
            )
            : null;

        return new RegionalAdminDashboardDto(
            stuckTasks,
            pendingMyActionCount,
            regionFunnelCounts,
            totalWaitingActionCount,
            oldestWaitingItem
        );
    }

    public async Task<SystemAdminDashboardDto> GetSystemAdminDashboardAsync(CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var hasMeterSizes = await db.MeterSizes.AnyAsync(ct);
        var hasRefDocs = await db.ReferenceDocuments.AnyAsync(ct);
        var hasFuelTypes = await db.FuelTypes.AnyAsync(ct);
        var hasIndustryTypes = await db.IndustryTypes.AnyAsync(ct);

        var healthItems = new List<MasterDataHealthItem>
        {
            new("meter_sizes", "Katalog G-Size (Ukuran Meter)", hasMeterSizes, hasMeterSizes ? "Katalog ukuran meter terkonfigurasi" : "Katalog G-Size belum diisi"),
            new("ref_documents", "Template / Dokumen Acuan", hasRefDocs, hasRefDocs ? "Dokumen acuan kebijakan terkonfigurasi" : "Dokumen acuan belum diisi"),
            new("fuel_types", "Master Jenis Bahan Bakar", hasFuelTypes, hasFuelTypes ? "Master jenis bahan bakar terkonfigurasi" : "Jenis bahan bakar belum diisi"),
            new("industry_types", "Master Sektor & Industri", hasIndustryTypes, hasIndustryTypes ? "Sektor industri terkonfigurasi" : "Master industri belum diisi")
        };

        var activeUsersCount = await db.Users.CountAsync(ct);
        var activeRegionsCount = await db.Regions.CountAsync(r => r.Active, ct);
        var activeAreasCount = await db.Areas.CountAsync(a => a.Active, ct);

        return new SystemAdminDashboardDto(
            healthItems,
            activeUsersCount,
            activeRegionsCount,
            activeAreasCount,
            4 // KK0, A1, Permohonan NOL, Resume Evaluasi
        );
    }
}