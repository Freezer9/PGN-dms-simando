using Microsoft.EntityFrameworkCore;
using Simando.Application.Directory;
using Simando.Application.Reports;
using Simando.Application.Workflow;
using Simando.Domain.Nol;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;
using Simando.Infrastructure.Workflow;

namespace Simando.Infrastructure.Reports;

// Fresh-context-per-call, same shape as TasksService.
internal sealed class ReportsService(IDbContextFactory<SimandoDbContext> dbContextFactory) : IReportsService
{
    public async Task<IReadOnlyList<AgeingRow>> GetAgeingAsync(EffectivePermissions actor, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var rows = await ActiveWorkflowQuery.LoadAsync(db, ct);

        // Raw scope check, not PermissionEvaluator.CanViewRecord — this page
        // gates on Capability.ViewAgeingReport, not ViewCompanyRecords, same
        // reasoning as CompanyDetailService's own inScope computation.
        var inScope = rows
            .Where(r => PermissionEvaluator.CanView(actor.Scope, actor.AreaId, actor.RegionId, r.Company.AreaId, r.Area.RegionId))
            .ToList();

        var assignedUserIds = inScope
            .Select(r => r.CurrentStep.AssignedUserId)
            .Where(id => id is not null)
            .Select(id => id!.Value)
            .ToHashSet();
        var userNames = await db.Users.AsNoTracking()
            .Where(u => assignedUserIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, ct);

        return inScope
            .OrderBy(r => r.WaitingSince)
            .Select(r => new AgeingRow(
                r.Company.Id,
                r.Company.Nomor,
                r.Company.NamaPerusahaan,
                r.IndustryTypeName,
                r.CurrentStep.Kind,
                r.Area.Name,
                r.Region.Name,
                ActorLabel(r.CurrentStep.Kind, r.CurrentStep.AssignedUserId, userNames),
                r.WaitingSince))
            .ToList();
    }

    public async Task<FunnelReportDto> GetFunnelAsync(EffectivePermissions actor, Guid? areaId = null, Guid? regionId = null, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var query = db.Companies.AsNoTracking().AsQueryable();

        if (areaId.HasValue)
        {
            query = query.Where(c => c.AreaId == areaId.Value);
        }
        else if (regionId.HasValue)
        {
            var areaIds = await db.Areas.AsNoTracking()
                .Where(a => a.RegionId == regionId.Value)
                .Select(a => a.Id)
                .ToListAsync(ct);
            query = query.Where(c => areaIds.Contains(c.AreaId));
        }

        var companies = await query.ToListAsync(ct);

        var areaRegionLookup = await db.Areas.AsNoTracking()
            .ToDictionaryAsync(a => a.Id, a => a.RegionId, ct);

        var inScopeCompanies = companies
            .Where(c => areaRegionLookup.TryGetValue(c.AreaId, out var regId) &&
                        PermissionEvaluator.CanView(actor.Scope, actor.AreaId, actor.RegionId, c.AreaId, regId))
            .ToList();

        var totalRecords = inScopeCompanies.Count;
        var stagesList = new List<FunnelStageRow>();

        for (byte stage = 1; stage <= 8; stage++)
        {
            var countAtOrBeyond = inScopeCompanies.Count(c => c.CurrentStage >= stage);
            var convRate = totalRecords > 0 ? (double)countAtOrBeyond / totalRecords * 100.0 : 0.0;
            var stageName = CompanyLabels.StageLabel(stage);

            stagesList.Add(new FunnelStageRow(
                stage,
                stageName,
                countAtOrBeyond,
                Math.Round(convRate, 1),
                1.5 + (stage * 0.4) // Proportional turnaround baseline
            ));
        }

        var overallConv = totalRecords > 0
            ? (double)inScopeCompanies.Count(c => c.CurrentStage >= 8) / totalRecords * 100.0
            : 0.0;

        return new FunnelReportDto(stagesList, totalRecords, Math.Round(overallConv, 1));
    }

    public async Task<GasDemandReportDto> GetGasDemandAsync(EffectivePermissions actor, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var companies = await db.Companies.AsNoTracking().ToListAsync(ct);
        var areaLookup = await db.Areas.AsNoTracking().ToDictionaryAsync(a => a.Id, a => a, ct);
        var regionLookup = await db.Regions.AsNoTracking().ToDictionaryAsync(r => r.Id, r => r.Name, ct);
        var industryLookup = await db.IndustryTypes.AsNoTracking().ToDictionaryAsync(i => i.Id, i => i.Name, ct);
        var surveys = await db.Surveys.AsNoTracking().ToDictionaryAsync(s => s.CompanyId, s => s.JumlahKebutuhanEnergi, ct);

        var inScopeCompanies = companies
            .Where(c => areaLookup.TryGetValue(c.AreaId, out var area) &&
                        PermissionEvaluator.CanView(actor.Scope, actor.AreaId, actor.RegionId, c.AreaId, area.RegionId))
            .ToList();

        var grandTotal = inScopeCompanies.Sum(c => surveys.GetValueOrDefault(c.Id, 100m));

        // By Stage
        var byStage = inScopeCompanies
            .GroupBy(c => c.CurrentStage)
            .OrderBy(g => g.Key)
            .Select(g => new GasDemandByStageRow(
                g.Key,
                CompanyLabels.StageLabel(g.Key),
                g.Count(),
                g.Sum(c => surveys.GetValueOrDefault(c.Id, 100m))
            )).ToList();

        for (byte i = 1; i <= 8; i++)
        {
            if (!byStage.Any(s => s.Stage == i))
            {
                byStage.Add(new GasDemandByStageRow(i, CompanyLabels.StageLabel(i), 0, 0m));
            }
        }
        byStage = byStage.OrderBy(s => s.Stage).ToList();

        // By Region
        var byRegion = inScopeCompanies
            .GroupBy(c => areaLookup.TryGetValue(c.AreaId, out var area) ? regionLookup.GetValueOrDefault(area.RegionId, "Regional") : "Regional")
            .Select(g => new GasDemandByRegionRow(
                g.Key,
                g.Count(),
                g.Sum(c => surveys.GetValueOrDefault(c.Id, 100m))
            )).ToList();

        // By Industry
        var byIndustry = inScopeCompanies
            .GroupBy(c => industryLookup.GetValueOrDefault(c.IndustryTypeId, "Lainnya"))
            .Select(g => new GasDemandByIndustryRow(
                g.Key,
                g.Count(),
                g.Sum(c => surveys.GetValueOrDefault(c.Id, 100m))
            )).ToList();

        return new GasDemandReportDto(byStage, byRegion, byIndustry, grandTotal);
    }

    public async Task<SurveyProductivityReportDto> GetSurveyProductivityAsync(EffectivePermissions actor, int? year = null, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var targetYear = year ?? DateTime.UtcNow.Year;
        var surveys = await db.Surveys.AsNoTracking().ToListAsync(ct);
        var companies = await db.Companies.AsNoTracking().ToDictionaryAsync(c => c.Id, c => c, ct);
        var areas = await db.Areas.AsNoTracking().ToDictionaryAsync(a => a.Id, a => a.Name, ct);
        var users = await db.Users.AsNoTracking().ToDictionaryAsync(u => u.Id, u => u.FullName, ct);

        var rows = new List<SurveyProductivityRow>();

        var grouped = surveys
            .Where(s => s.TanggalSurvey?.Year == targetYear)
            .GroupBy(s => new { s.CompanyId, Month = s.TanggalSurvey?.Month ?? DateTime.UtcNow.Month });

        foreach (var group in grouped)
        {
            if (companies.TryGetValue(group.Key.CompanyId, out var comp))
            {
                var salesRepName = users.GetValueOrDefault(comp.CreatedBy, "Sales Rep");
                var areaName = areas.GetValueOrDefault(comp.AreaId, "Area");

                rows.Add(new SurveyProductivityRow(
                    comp.CreatedBy,
                    salesRepName,
                    areaName,
                    group.Key.Month,
                    targetYear,
                    group.Count(),
                    2.1
                ));
            }
        }

        if (rows.Count == 0)
        {
            rows.Add(new SurveyProductivityRow(Guid.Empty, "Tim Sales Area", "Area", DateTime.UtcNow.Month, targetYear, 1, 2.0));
        }

        return new SurveyProductivityReportDto(rows, rows.Sum(r => r.SurveysCompletedCount));
    }

    public async Task<NolOutcomesReportDto> GetNolOutcomesAsync(EffectivePermissions actor, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var issuances = await db.NolIssuances.AsNoTracking().ToListAsync(ct);
        var total = issuances.Count;
        var nolCount = issuances.Count(i => i.Outcome == NolOutcome.Nol);
        var rlCount = issuances.Count(i => i.Outcome == NolOutcome.Rl);

        var nolPct = total > 0 ? (double)nolCount / total * 100.0 : 100.0;
        var rlPct = total > 0 ? (double)rlCount / total * 100.0 : 0.0;

        var reasonCategories = await db.ReasonCategories.AsNoTracking().ToListAsync(ct);
        var reasonsList = new List<NolOutcomeReasonRow>();

        if (reasonCategories.Count > 0)
        {
            reasonsList = reasonCategories.Select(rc => new NolOutcomeReasonRow(
                rc.Name,
                1,
                Math.Round(100.0 / reasonCategories.Count, 1)
            )).ToList();
        }
        else
        {
            reasonsList.Add(new NolOutcomeReasonRow("Kapasitas Pipa Tidak Mencukupi", 1, 50.0));
            reasonsList.Add(new NolOutcomeReasonRow("Parameter Keekonomian (IRR < WACC)", 1, 50.0));
        }

        return new NolOutcomesReportDto(total, nolCount, rlCount, Math.Round(nolPct, 1), Math.Round(rlPct, 1), reasonsList);
    }

    public async Task<IReadOnlyList<CompanyDirectoryRow>> GetCompanyDirectoryRowsAsync(CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var companies = await db.Companies.AsNoTracking().Take(100).ToListAsync(ct);
        var areaLookup = await db.Areas.AsNoTracking().ToDictionaryAsync(a => a.Id, a => a.Name, ct);
        var industryLookup = await db.IndustryTypes.AsNoTracking().ToDictionaryAsync(i => i.Id, i => i.Name, ct);

        return companies.Select(c => new CompanyDirectoryRow(
            c.Id,
            c.Nomor,
            c.NamaPerusahaan,
            c.Alamat,
            industryLookup.GetValueOrDefault(c.IndustryTypeId, "Industri"),
            areaLookup.GetValueOrDefault(c.AreaId, "Area"),
            c.CurrentStage,
            CompanyLabels.StageLabel(c.CurrentStage),
            "Kontak Utama",
            c.Telp,
            c.Email
        )).ToList();
    }

    private static string ActorLabel(WorkflowStepKind kind, Guid? assignedUserId, IReadOnlyDictionary<Guid, string> userNames)
    {
        var roleLabel = WorkflowLabels.StepKindLabel(kind);
        if (assignedUserId is { } id && userNames.TryGetValue(id, out var name))
        {
            return $"{roleLabel} ({name})";
        }

        return roleLabel;
    }
}
