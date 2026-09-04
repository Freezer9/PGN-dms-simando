using Microsoft.EntityFrameworkCore;
using Simando.Application.Directory;
using Simando.Application.RecordHub;
using Simando.Application.Security;
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
internal sealed class CompanyDetailService(
    IDbContextFactory<SimandoDbContext> dbContextFactory,
    IBreakGlassService breakGlassService) : ICompanyDetailService
{
    public async Task<CompanyDetail?> GetDetailAsync(
        Guid companyId,
        Guid actorUserId,
        EffectivePermissions actor,
        IReadOnlySet<Role> actorRoles,
        CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var hasViewCapability = actor.HasCapability(Capability.ViewCompanyRecords);
        var hasActiveBreakGlass = false;
        if (!hasViewCapability)
        {
            hasActiveBreakGlass = await breakGlassService.HasActiveAccessAsync(actorUserId, companyId, ct);
            if (!hasActiveBreakGlass)
            {
                return null;
            }
        }

        var companyQuery = db.Companies.AsNoTracking();
        if (hasActiveBreakGlass)
        {
            companyQuery = companyQuery.IgnoreQueryFilters().Where(c => c.DeletedAt == null);
        }

        var company = await companyQuery.FirstOrDefaultAsync(c => c.Id == companyId, ct);
        if (company is null)
        {
            return null;
        }

        var areaQuery = db.Areas.AsNoTracking();
        if (hasActiveBreakGlass) areaQuery = areaQuery.IgnoreQueryFilters();
        var area = await areaQuery.FirstOrDefaultAsync(a => a.Id == company.AreaId, ct);
        var areaId = area?.Id ?? company.AreaId;
        var areaName = area?.Name ?? "Area";

        var regionQuery = db.Regions.AsNoTracking();
        if (hasActiveBreakGlass) regionQuery = regionQuery.IgnoreQueryFilters();
        var region = area is not null ? await regionQuery.FirstOrDefaultAsync(r => r.Id == area.RegionId, ct) : null;
        var regionId = region?.Id ?? Guid.Empty;
        var regionName = region?.Name ?? "Region";

        var industryTypesQuery = db.IndustryTypes.AsNoTracking();
        var usersQuery = db.Users.AsNoTracking();
        var villagesQuery = db.Villages.AsNoTracking();
        var districtsQuery = db.Districts.AsNoTracking();
        var regenciesQuery = db.Regencies.AsNoTracking();

        if (hasActiveBreakGlass)
        {
            industryTypesQuery = industryTypesQuery.IgnoreQueryFilters();
            usersQuery = usersQuery.IgnoreQueryFilters();
            villagesQuery = villagesQuery.IgnoreQueryFilters();
            districtsQuery = districtsQuery.IgnoreQueryFilters();
            regenciesQuery = regenciesQuery.IgnoreQueryFilters();
        }

        var industryTypeName = await industryTypesQuery
            .Where(t => t.Id == company.IndustryTypeId).Select(t => t.Name).FirstOrDefaultAsync(ct) ?? "Industri";
        var salesRepName = await usersQuery
            .Where(u => u.Id == company.CreatedBy).Select(u => u.FullName).FirstOrDefaultAsync(ct) ?? "Sales Representative";

        var village = await villagesQuery.FirstOrDefaultAsync(v => v.Id == company.VillageId, ct);
        var district = village is not null ? await districtsQuery.FirstOrDefaultAsync(d => d.Id == village.DistrictId, ct) : null;
        var regency = district is not null ? await regenciesQuery.FirstOrDefaultAsync(r => r.Id == district.RegencyId, ct) : null;
        var locationLabel = regency is not null ? $"{(regency.Type == RegencyType.Kota ? "Kota" : "Kabupaten")} {regency.Name}" : "Lokasi";

        var contactsQuery = db.CompanyContacts.AsNoTracking();
        if (hasActiveBreakGlass) contactsQuery = contactsQuery.IgnoreQueryFilters();

        var contacts = await contactsQuery
            .Where(c => c.CompanyId == companyId)
            .OrderByDescending(c => c.IsPrimary).ThenBy(c => c.SortOrder)
            .Select(c => new ContactSummary(c.Id, c.Nama, c.Jabatan, c.IsPrimary, c.Email, c.NoHp))
            .ToListAsync(ct);

        var instancesQuery = db.WorkflowInstances.AsNoTracking();
        if (hasActiveBreakGlass) instancesQuery = instancesQuery.IgnoreQueryFilters();

        var latestInstance = await instancesQuery
            .Where(i => i.CompanyId == companyId)
            .OrderByDescending(i => i.StartedAt)
            .FirstOrDefaultAsync(ct);

        WorkflowStep? currentStep = null;
        var currentKind = WorkflowStepAssignment.CurrentStepKind(company.Status);
        if (currentKind is not null && latestInstance is not null)
        {
            var stepsQuery = db.WorkflowSteps.AsNoTracking();
            if (hasActiveBreakGlass) stepsQuery = stepsQuery.IgnoreQueryFilters();

            currentStep = await stepsQuery
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
                holderName = await usersQuery
                    .Where(u => u.Id == assignedUserId).Select(u => u.FullName).FirstOrDefaultAsync(ct) ?? "Pengguna";
            }

            var predQuery = db.WorkflowSteps.AsNoTracking();
            if (hasActiveBreakGlass) predQuery = predQuery.IgnoreQueryFilters();

            var predecessor = await predQuery
                .Where(s => s.WorkflowInstanceId == latestInstance!.Id && s.StepOrder < currentStep.StepOrder)
                .OrderByDescending(s => s.StepOrder)
                .FirstOrDefaultAsync(ct);
            statusSince = predecessor?.ActedAt ?? latestInstance!.StartedAt;
        }
        else
        {
            var statusEventsQuery = db.StatusEvents.AsNoTracking();
            if (hasActiveBreakGlass) statusEventsQuery = statusEventsQuery.IgnoreQueryFilters();

            var lastEventAt = await statusEventsQuery
                .Where(e => e.CompanyId == companyId)
                .OrderByDescending(e => e.OccurredAt)
                .Select(e => (DateTimeOffset?)e.OccurredAt)
                .FirstOrDefaultAsync(ct);
            statusSince = lastEventAt ?? company.CreatedAt;
        }

        // Under break-glass, System Admin has read-only access: inScope is false, no actions allowed
        var inScope = !hasActiveBreakGlass && PermissionEvaluator.CanViewRecord(actor, company.AreaId, regionId);

        var canSubmit = inScope
            && company.CreatedBy == actorUserId
            && company.Status == RecordStatus.Draft
            && company.CurrentStage >= 6
            && PermissionEvaluator.CanAct(actor, Capability.SubmitForApproval, isUsersTurn: true);

        var canAct = inScope && (
            (currentStep is not null
                && WorkflowStepAssignment.IsAssignedToStep(currentStep, actorUserId, actorRoles)
                && PermissionEvaluator.CanAct(actor, Capability.ActOnApprovalStep, isUsersTurn: true)
                && !PermissionEvaluator.IsSelfApproval(actorUserId, company.CreatedBy, []))
            ||
            (company.Status == RecordStatus.Rejected
                && PermissionEvaluator.CanAct(actor, Capability.ReassignWorkflowStep, isUsersTurn: true))
        );

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

    public async Task<CompanyRecordDto?> GetCompanyRecordAsync(
        Guid companyId,
        Guid actorUserId,
        EffectivePermissions actor,
        IReadOnlySet<Role> actorRoles,
        CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var hasViewCapability = actor.HasCapability(Capability.ViewCompanyRecords);
        var hasActiveBreakGlass = false;
        if (!hasViewCapability)
        {
            hasActiveBreakGlass = await breakGlassService.HasActiveAccessAsync(actorUserId, companyId, ct);
            if (!hasActiveBreakGlass)
            {
                return null;
            }
        }

        var companyQuery = db.Companies.AsNoTracking();
        if (hasActiveBreakGlass)
        {
            companyQuery = companyQuery.IgnoreQueryFilters().Where(c => c.DeletedAt == null);
        }

        var company = await companyQuery.FirstOrDefaultAsync(c => c.Id == companyId, ct);
        if (company is null)
        {
            return null;
        }

        var detail = await GetDetailAsync(companyId, actorUserId, actor, actorRoles, ct);
        if (detail is null)
        {
            return null;
        }

        var villagesQuery = db.Villages.AsNoTracking();
        var districtsQuery = db.Districts.AsNoTracking();
        var regenciesQuery = db.Regencies.AsNoTracking();
        var provincesQuery = db.Provinces.AsNoTracking();
        var contactsQuery = db.CompanyContacts.AsNoTracking();

        if (hasActiveBreakGlass)
        {
            villagesQuery = villagesQuery.IgnoreQueryFilters();
            districtsQuery = districtsQuery.IgnoreQueryFilters();
            regenciesQuery = regenciesQuery.IgnoreQueryFilters();
            provincesQuery = provincesQuery.IgnoreQueryFilters();
            contactsQuery = contactsQuery.IgnoreQueryFilters();
        }

        var village = await villagesQuery.FirstOrDefaultAsync(v => v.Id == company.VillageId, ct);
        var district = village is not null ? await districtsQuery.FirstOrDefaultAsync(d => d.Id == village.DistrictId, ct) : null;
        var regency = district is not null ? await regenciesQuery.FirstOrDefaultAsync(r => r.Id == district.RegencyId, ct) : null;
        var province = regency is not null ? await provincesQuery.FirstOrDefaultAsync(p => p.Id == regency.ProvinceId, ct) : null;

        var contacts = await contactsQuery
            .Where(c => c.CompanyId == companyId)
            .OrderByDescending(c => c.IsPrimary).ThenBy(c => c.SortOrder)
            .Select(c => new ContactDetail(c.Id, c.Nama, c.Jabatan, c.Email, c.NoHp, c.LinkedIn, c.Instagram, c.Facebook, c.IsPrimary, c.SortOrder))
            .ToListAsync(ct);

        return new CompanyRecordDto(
            company.Id,
            company.Nomor,
            company.NamaPerusahaan,
            company.Website,
            company.Alamat,
            company.VillageId,
            village?.Name ?? "Desa/Kelurahan",
            district?.Id ?? Guid.Empty,
            district?.Name ?? "Kecamatan",
            regency?.Id ?? Guid.Empty,
            regency?.Name ?? "Kota/Kabupaten",
            province?.Id ?? Guid.Empty,
            province?.Name ?? "Provinsi",
            detail.LocationLabel,
            company.IndustryTypeId,
            detail.IndustryTypeName,
            company.Npwp,
            company.Email,
            company.KodePos,
            company.Telp,
            company.AreaId,
            detail.AreaName,
            detail.RegionId,
            detail.RegionName,
            company.CurrentStage,
            company.Status,
            company.CreatedBy,
            detail.SalesRepName,
            company.CreatedAt,
            company.UpdatedAt,
            company.Location?.Y,
            company.Location?.X,
            detail.HolderLabel,
            detail.HolderName,
            detail.StatusSince,
            detail.CurrentStepId,
            detail.CurrentStepKind,
            detail.WorkflowInstanceId,
            detail.CanSubmit,
            detail.CanAct,
            detail.CanChooseReviewers,
            contacts);
    }

    public async Task<IReadOnlyList<TimelineEntry>> GetTimelineAsync(Guid companyId, bool isBreakGlass = false, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var eventsQuery = db.StatusEvents.AsNoTracking();
        var stepsQuery = db.WorkflowSteps.AsNoTracking();
        var usersQuery = db.Users.AsNoTracking();

        if (isBreakGlass)
        {
            eventsQuery = eventsQuery.IgnoreQueryFilters();
            stepsQuery = stepsQuery.IgnoreQueryFilters();
            usersQuery = usersQuery.IgnoreQueryFilters();
        }

        var events = await eventsQuery
            .Where(e => e.CompanyId == companyId)
            .OrderByDescending(e => e.OccurredAt)
            .ToListAsync(ct);

        var stepIds = events.Where(e => e.WorkflowStepId is not null).Select(e => e.WorkflowStepId!.Value).ToHashSet();
        var stepKinds = await stepsQuery
            .Where(s => stepIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Kind, ct);

        var actorIds = events.Select(e => e.ActorId).ToHashSet();
        var actorNames = await usersQuery
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
