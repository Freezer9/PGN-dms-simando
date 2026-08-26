using Microsoft.EntityFrameworkCore;
using Simando.Domain.Directory;
using Simando.Domain.Organisation;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Persistence;

namespace Simando.Infrastructure.Workflow;

// Every currently-open instance's current step, joined with the company/
// area/region/industry-type data both TasksService (/tasks) and
// ReportsService (/reports/ageing) need — no submitter/holder name here,
// since that differs per caller (Tasks wants the creator, Ageing wants the
// current holder) and is cheap to layer on top.
public readonly record struct ActiveWorkflowRow(
    Company Company, Area Area, Region Region, string IndustryTypeName,
    WorkflowStep CurrentStep, DateTimeOffset WaitingSince);

internal static class ActiveWorkflowQuery
{
    public static async Task<List<ActiveWorkflowRow>> LoadAsync(SimandoDbContext db, CancellationToken ct)
    {
        var instances = await db.WorkflowInstances.AsNoTracking().Where(i => i.CompletedAt == null).ToListAsync(ct);
        var instanceIds = instances.Select(i => i.Id).ToHashSet();

        var steps = await db.WorkflowSteps.AsNoTracking()
            .Where(s => instanceIds.Contains(s.WorkflowInstanceId))
            .ToListAsync(ct);
        var stepsByInstance = steps.ToLookup(s => s.WorkflowInstanceId);

        var companyIds = instances.Select(i => i.CompanyId).ToHashSet();
        var companies = await db.Companies.AsNoTracking()
            .Where(c => companyIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, ct);

        var areaIds = companies.Values.Select(c => c.AreaId).ToHashSet();
        var areas = await db.Areas.AsNoTracking().Where(a => areaIds.Contains(a.Id)).ToDictionaryAsync(a => a.Id, ct);

        var regionIds = areas.Values.Select(a => a.RegionId).ToHashSet();
        var regions = await db.Regions.AsNoTracking().Where(r => regionIds.Contains(r.Id)).ToDictionaryAsync(r => r.Id, ct);

        var industryTypeIds = companies.Values.Select(c => c.IndustryTypeId).ToHashSet();
        var industryTypeNames = await db.IndustryTypes.AsNoTracking()
            .Where(t => industryTypeIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Name, ct);

        var result = new List<ActiveWorkflowRow>();
        foreach (var instance in instances)
        {
            if (!companies.TryGetValue(instance.CompanyId, out var company))
            {
                continue;
            }

            var currentKind = WorkflowStepAssignment.CurrentStepKind(company.Status);
            if (currentKind is null)
            {
                continue;
            }

            var instanceSteps = stepsByInstance[instance.Id].OrderBy(s => s.StepOrder).ToList();
            var currentStep = instanceSteps.FirstOrDefault(s => s.Kind == currentKind && s.ActedAt is null);
            if (currentStep is null)
            {
                continue;
            }

            var predecessor = instanceSteps
                .Where(s => s.StepOrder < currentStep.StepOrder)
                .OrderByDescending(s => s.StepOrder)
                .FirstOrDefault();
            var waitingSince = predecessor?.ActedAt ?? instance.StartedAt;

            var area = areas[company.AreaId];
            var region = regions[area.RegionId];
            var industryTypeName = industryTypeNames.GetValueOrDefault(company.IndustryTypeId, "");

            result.Add(new ActiveWorkflowRow(company, area, region, industryTypeName, currentStep, waitingSince));
        }

        return result;
    }
}
