using Microsoft.EntityFrameworkCore;
using Simando.Application.RecordHub;
using Simando.Application.Workflow;
using Simando.Domain.Audit;
using Simando.Domain.Geography;
using Simando.Domain.Security;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Persistence;

namespace Simando.Infrastructure.RecordHub;

// Fresh-context-per-call, read-only — same shape as TasksService. A single
// record's worth of joins, so plain sequential queries rather than
// TasksService's flat-list-plus-dictionary style (that style pays off across
// many rows; here it would just be dictionaries of size one).
internal sealed class CompanyDetailService(IDbContextFactory<SimandoDbContext> dbContextFactory) : ICompanyDetailService
{
    public async Task<CompanyDetail?> GetDetailAsync(
        Guid companyId,
        Guid actorUserId,
        EffectivePermissions actor,
        IReadOnlySet<Role> actorRoles,
        CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == companyId, ct);
        if (company is null)
        {
            return null;
        }

        var area = await db.Areas.AsNoTracking().FirstOrDefaultAsync(a => a.Id == company.AreaId, ct);
        var areaId = area?.Id ?? company.AreaId;
        var areaName = area?.Name ?? "Area";

        var region = area is not null ? await db.Regions.AsNoTracking().FirstOrDefaultAsync(r => r.Id == area.RegionId, ct) : null;
        var regionId = region?.Id ?? Guid.Empty;
        var regionName = region?.Name ?? "Region";

        var industryTypeName = await db.IndustryTypes.AsNoTracking()
            .Where(t => t.Id == company.IndustryTypeId).Select(t => t.Name).FirstOrDefaultAsync(ct) ?? "Industri";
        var salesRepName = await db.Users.AsNoTracking()
            .Where(u => u.Id == company.CreatedBy).Select(u => u.FullName).FirstOrDefaultAsync(ct) ?? "Sales Representative";

        var village = await db.Villages.AsNoTracking().FirstOrDefaultAsync(v => v.Id == company.VillageId, ct);
        var district = village is not null ? await db.Districts.AsNoTracking().FirstOrDefaultAsync(d => d.Id == village.DistrictId, ct) : null;
        var regency = district is not null ? await db.Regencies.AsNoTracking().FirstOrDefaultAsync(r => r.Id == district.RegencyId, ct) : null;
        var locationLabel = regency is not null ? $"{(regency.Type == RegencyType.Kota ? "Kota" : "Kabupaten")} {regency.Name}" : "Lokasi";

        var contacts = await db.CompanyContacts.AsNoTracking()
            .Where(c => c.CompanyId == companyId)
            .OrderByDescending(c => c.IsPrimary).ThenBy(c => c.SortOrder)
            .Select(c => new ContactSummary(c.Id, c.Nama, c.Jabatan, c.IsPrimary, c.Email, c.NoHp))
            .ToListAsync(ct);

        var latestInstance = await db.WorkflowInstances.AsNoTracking()
            .Where(i => i.CompanyId == companyId)
            .OrderByDescending(i => i.StartedAt)
            .FirstOrDefaultAsync(ct);

        WorkflowStep? currentStep = null;
        var currentKind = WorkflowStepAssignment.CurrentStepKind(company.Status);
        if (currentKind is not null && latestInstance is not null)
        {
            currentStep = await db.WorkflowSteps.AsNoTracking()
                .FirstOrDefaultAsync(s => s.WorkflowInstanceId == latestInstance.Id && s.Kind == currentKind && s.ActedAt == null, ct);
        }

        string? holderLabel = null;
        string? holderName = null;
        DateTimeOffset statusSince;

        if (currentStep is not null)
        {
            holderLabel = WorkflowLabels.StepKindLabel(currentStep.Kind);
            if (currentStep.AssignedUserId is { } assignedUserId)
            {
                holderName = await db.Users.AsNoTracking()
                    .Where(u => u.Id == assignedUserId).Select(u => u.FullName).FirstOrDefaultAsync(ct) ?? "Pengguna";
            }

            var predecessor = await db.WorkflowSteps.AsNoTracking()
                .Where(s => s.WorkflowInstanceId == latestInstance!.Id && s.StepOrder < currentStep.StepOrder)
                .OrderByDescending(s => s.StepOrder)
                .FirstOrDefaultAsync(ct);
            statusSince = predecessor?.ActedAt ?? latestInstance!.StartedAt;
        }
        else
        {
            var lastEventAt = await db.StatusEvents.AsNoTracking()
                .Where(e => e.CompanyId == companyId)
                .OrderByDescending(e => e.OccurredAt)
                .Select(e => (DateTimeOffset?)e.OccurredAt)
                .FirstOrDefaultAsync(ct);
            statusSince = lastEventAt ?? company.CreatedAt;
        }

        var inScope = PermissionEvaluator.CanView(actor.Scope, actor.AreaId, actor.RegionId, company.AreaId, regionId);

        var canSubmit = inScope
            && company.CreatedBy == actorUserId
            && company.Status == RecordStatus.Draft
            && company.CurrentStage >= 6
            && PermissionEvaluator.CanAct(actor, Capability.SubmitForApproval, isUsersTurn: true);

        var canAct = inScope
            && currentStep is not null
            && WorkflowStepAssignment.IsAssignedToStep(currentStep, actorUserId, actorRoles)
            && PermissionEvaluator.CanAct(actor, Capability.ActOnApprovalStep, isUsersTurn: true)
            && !PermissionEvaluator.IsSelfApproval(actorUserId, company.CreatedBy, []);

        var canChooseReviewers = canAct
            && company.Status == RecordStatus.RegionalAdmin
            && actor.HasCapability(Capability.ChooseReviewers);

        return new CompanyDetail(
            company.Id,
            company.Nomor,
            company.NamaPerusahaan,
            industryTypeName,
            locationLabel,
            company.CreatedBy,
            salesRepName,
            areaId,
            areaName,
            regionId,
            regionName,
            company.CurrentStage,
            company.Status,
            holderLabel,
            holderName,
            statusSince,
            currentStep?.Id,
            currentStep?.Kind,
            latestInstance?.Id,
            canSubmit,
            canAct,
            canChooseReviewers,
            contacts);
    }

    public async Task<IReadOnlyList<TimelineEntry>> GetTimelineAsync(Guid companyId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var events = await db.StatusEvents.AsNoTracking()
            .Where(e => e.CompanyId == companyId)
            .OrderByDescending(e => e.OccurredAt)
            .ToListAsync(ct);

        var stepIds = events.Where(e => e.WorkflowStepId is not null).Select(e => e.WorkflowStepId!.Value).ToHashSet();
        var stepKinds = await db.WorkflowSteps.AsNoTracking()
            .Where(s => stepIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Kind, ct);

        var actorIds = events.Select(e => e.ActorId).ToHashSet();
        var actorNames = await db.Users.AsNoTracking()
            .Where(u => actorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, ct);

        return events
            .Select(e =>
            {
                var roleLabel = e.Action == StatusEventAction.Submit
                    ? "Sales Area"
                    : e.WorkflowStepId is { } stepId && stepKinds.TryGetValue(stepId, out var kind)
                        ? WorkflowLabels.StepKindLabel(kind)
                        : "";
                return new TimelineEntry(
                    e.Id, e.Action, e.ToStatus, roleLabel,
                    actorNames.GetValueOrDefault(e.ActorId, ""), e.Comment, e.OccurredAt);
            })
            .ToList();
    }
}
