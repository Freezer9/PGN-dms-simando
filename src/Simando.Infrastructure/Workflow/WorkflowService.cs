using Microsoft.EntityFrameworkCore;
using Simando.Application.Notifications;
using Simando.Application.Security;
using Simando.Application.Workflow;
using Simando.Domain.Audit;
using Simando.Domain.Directory;
using Simando.Domain.Security;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Persistence;

namespace Simando.Infrastructure.Workflow;

// Fresh-context-per-call, same shape as OrganisationService — each method's
// mutations land in one SaveChangesAsync call, which EF Core already wraps
// in one implicit transaction, so no explicit IUnitOfWork is needed here.
internal sealed class WorkflowService(
    IDbContextFactory<SimandoDbContext> dbContextFactory,
    INotificationChannel notifications) : IWorkflowService
{
    public async Task<SubmitResult> StartAsync(
        Guid companyId,
        Guid actorUserId,
        EffectivePermissions actor,
        IReadOnlySet<Role> actorRoles,
        CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies.IgnoreQueryFilters().FirstAsync(c => c.Id == companyId, ct);

        if (company.CreatedBy != actorUserId)
        {
            return SubmitResult.Rejected("Hanya pembuat berkas yang dapat mengajukan untuk persetujuan.");
        }

        if (!PermissionEvaluator.CanAct(actor, Capability.SubmitForApproval, isUsersTurn: true))
        {
            return SubmitResult.Rejected("Anda tidak berwenang mengajukan berkas untuk persetujuan.");
        }

        if (!await IsScopedToCompanyAsync(db, actor, company.AreaId, ct))
        {
            return SubmitResult.Rejected("Berkas ini berada di luar wilayah Anda.");
        }

        if (company.CurrentStage < 6)
        {
            return SubmitResult.Rejected("Pengajuan persetujuan hanya dapat dilakukan pada Tahap 6 (Permohonan NOL).");
        }

        var existingAttachmentKinds = await db.Attachments.IgnoreQueryFilters().AsNoTracking()
            .Where(a => a.CompanyId == companyId)
            .Select(a => a.Kind)
            .ToListAsync(ct);
        var gateCheck = StageGateEvaluator.EvaluateNolRequestToSubmitGate(existingAttachmentKinds);
        if (!gateCheck.IsPassed)
        {
            return SubmitResult.Rejected(gateCheck.MissingPrerequisites[0]);
        }

        var submit = WorkflowTransitions.Submit(company.Status);
        if (!submit.Success)
        {
            return SubmitResult.Rejected(submit.Error!);
        }

        var instance = new WorkflowInstance
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            StartedAt = DateTimeOffset.UtcNow,
        };

        var areaHeadStep = new WorkflowStep
        {
            Id = Guid.NewGuid(),
            WorkflowInstanceId = instance.Id,
            StepOrder = 1,
            Kind = WorkflowStepKind.AreaHead,
        };
        var regionalAdminStep = new WorkflowStep
        {
            Id = Guid.NewGuid(),
            WorkflowInstanceId = instance.Id,
            StepOrder = 2,
            Kind = WorkflowStepKind.RegionalAdmin,
        };

        db.Add(instance);
        db.AddRange(areaHeadStep, regionalAdminStep);

        var fromStatus = company.Status;
        company.Status = submit.NewStatus!.Value;

        db.Add(new StatusEvent
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            FromStage = company.CurrentStage,
            ToStage = company.CurrentStage,
            FromStatus = fromStatus,
            ToStatus = company.Status,
            ActorId = actorUserId,
            Action = StatusEventAction.Submit,
            OccurredAt = DateTimeOffset.UtcNow,
            WorkflowStepId = areaHeadStep.Id,
        });

        await db.SaveChangesAsync(ct);

        await NotifyStepHoldersAsync(db, company, instance.Id, WorkflowStepKind.AreaHead, $"Menunggu persetujuan Anda — {company.NamaPerusahaan}", ct);

        return SubmitResult.Success(instance.Id);
    }

    public async Task<RoleAssignmentResult> ChooseReviewersAsync(
        Guid workflowInstanceId,
        IReadOnlyList<Guid> reviewerUserIds,
        Guid actorUserId,
        EffectivePermissions actor,
        IReadOnlySet<Role> actorRoles,
        CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var instance = await db.WorkflowInstances.IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => (i.Id == workflowInstanceId || i.CompanyId == workflowInstanceId) && i.CompletedAt == null, ct)
            ?? await db.WorkflowInstances.IgnoreQueryFilters().FirstOrDefaultAsync(i => i.Id == workflowInstanceId, ct);

        if (instance is null)
        {
            return RoleAssignmentResult.Rejected("Alur kerja tidak ditemukan.");
        }

        var company = await db.Companies.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == instance.CompanyId, ct);
        if (company is null)
        {
            return RoleAssignmentResult.Rejected("Berkas tidak ditemukan.");
        }

        if (company.Status != RecordStatus.RegionalAdmin)
        {
            return RoleAssignmentResult.Rejected("Reviewer hanya dapat dipilih saat berkas berada di langkah Admin Regional.");
        }

        if (!actorRoles.Contains(Role.RegionalAdmin) || !actor.HasCapability(Capability.ChooseReviewers))
        {
            return RoleAssignmentResult.Rejected("Anda tidak berwenang memilih reviewer.");
        }

        if (!await IsScopedToCompanyAsync(db, actor, company.AreaId, ct))
        {
            return RoleAssignmentResult.Rejected("Berkas ini berada di luar wilayah Anda.");
        }

        if (reviewerUserIds.Count is < 2 or > 3)
        {
            return RoleAssignmentResult.Rejected("Pilih 2 atau 3 reviewer.");
        }

        if (reviewerUserIds.Contains(company.CreatedBy))
        {
            return RoleAssignmentResult.Rejected("Reviewer tidak boleh sama dengan pembuat berkas.");
        }

        instance.ReviewerCount = reviewerUserIds.Count == 3 ? Domain.Workflow.ReviewerCount.Three : Domain.Workflow.ReviewerCount.Two;

        WorkflowStepKind[] reviewerKinds = [WorkflowStepKind.Reviewer1, WorkflowStepKind.Reviewer2, WorkflowStepKind.Reviewer3];
        for (var i = 0; i < reviewerUserIds.Count; i++)
        {
            db.Add(new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = instance.Id,
                StepOrder = 3 + i,
                Kind = reviewerKinds[i],
                AssignedUserId = reviewerUserIds[i],
            });
        }

        db.Add(new WorkflowStep
        {
            Id = Guid.NewGuid(),
            WorkflowInstanceId = instance.Id,
            StepOrder = 3 + reviewerUserIds.Count,
            Kind = WorkflowStepKind.DivisionHead,
        });

        await db.SaveChangesAsync(ct);

        foreach (var reviewerUserId in reviewerUserIds)
        {
            await notifications.SendAsync(reviewerUserId, company.Id, $"Anda ditugaskan sebagai reviewer — {company.NamaPerusahaan}", ct);
        }

        return RoleAssignmentResult.Success();
    }

    public async Task<WorkflowActResult> ActAsync(
        Guid stepId,
        WorkflowAction action,
        string? comment,
        Guid actorUserId,
        EffectivePermissions actor,
        IReadOnlySet<Role> actorRoles,
        CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var step = await db.WorkflowSteps.IgnoreQueryFilters().FirstAsync(s => s.Id == stepId, ct);
        var instance = await db.WorkflowInstances.IgnoreQueryFilters().FirstAsync(i => i.Id == step.WorkflowInstanceId, ct);
        var company = await db.Companies.IgnoreQueryFilters().FirstAsync(c => c.Id == instance.CompanyId, ct);

        if (WorkflowStepAssignment.CurrentStepKind(company.Status) != step.Kind)
        {
            return WorkflowActResult.Rejected("Bukan giliran langkah ini.");
        }

        if (step.ActedAt is not null)
        {
            return WorkflowActResult.Rejected("Langkah ini sudah diproses.");
        }

        if (!WorkflowStepAssignment.IsAssignedToStep(step, actorUserId, actorRoles))
        {
            return WorkflowActResult.Rejected("Anda tidak ditugaskan pada langkah ini.");
        }

        if (!PermissionEvaluator.CanAct(actor, Capability.ActOnApprovalStep, isUsersTurn: true))
        {
            return WorkflowActResult.Rejected("Anda tidak berwenang bertindak pada langkah persetujuan.");
        }

        if (!await IsScopedToCompanyAsync(db, actor, company.AreaId, ct))
        {
            return WorkflowActResult.Rejected("Berkas ini berada di luar wilayah Anda.");
        }

        var stage7Editors = await db.NolEvaluations.IgnoreQueryFilters().AsNoTracking()
            .Where(e => e.NolRequestId == company.Id && e.EvaluatedBy.HasValue)
            .Select(e => e.EvaluatedBy!.Value)
            .ToListAsync(ct);

        if (actorUserId == company.CreatedBy)
        {
            return WorkflowActResult.Rejected("Tidak dapat bertindak pada berkas yang Anda buat sendiri.");
        }

        if (step.Kind != WorkflowStepKind.RegionalAdmin && stage7Editors.Contains(actorUserId))
        {
            return WorkflowActResult.Rejected("Tidak dapat bertindak pada berkas evaluasi yang Anda sunting.");
        }

        if (step.Kind == WorkflowStepKind.RegionalAdmin && action == WorkflowAction.Setuju)
        {
            var reviewersChosen = await db.WorkflowSteps.IgnoreQueryFilters()
                .AnyAsync(s => s.WorkflowInstanceId == instance.Id && s.Kind == WorkflowStepKind.Reviewer1, ct);
            if (!reviewersChosen)
            {
                return WorkflowActResult.Rejected("Pilih reviewer terlebih dahulu.");
            }
        }

        var transition = WorkflowTransitions.Apply(company.Status, action, instance.ReviewerCount ?? Domain.Workflow.ReviewerCount.Two, comment);
        if (!transition.Success)
        {
            return WorkflowActResult.Rejected(transition.Error!);
        }

        var fromStatus = company.Status;
        var now = DateTimeOffset.UtcNow;

        step.ActedAt = now;
        step.ActedBy = actorUserId;
        step.Action = action;
        step.Comment = comment;

        company.Status = transition.NewStatus!.Value;

        if (company.Status is RecordStatus.IssuedNol or RecordStatus.IssuedRl or RecordStatus.Rejected)
        {
            instance.CompletedAt = now;
            instance.FinalStatus = company.Status;
        }

        db.Add(new StatusEvent
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            FromStage = company.CurrentStage,
            ToStage = company.CurrentStage,
            FromStatus = fromStatus,
            ToStatus = company.Status,
            ActorId = actorUserId,
            Action = ToStatusEventAction(action),
            Comment = comment,
            OccurredAt = now,
            WorkflowStepId = step.Id,
        });

        await db.SaveChangesAsync(ct);

        await NotifyTransitionAsync(db, company, instance.Id, ct);

        return WorkflowActResult.Ok(company.Status);
    }

    public async Task<WorkflowActResult> ReassignStepAsync(
        Guid stepId,
        Guid newUserId,
        Guid actorUserId,
        EffectivePermissions actor,
        CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var step = await db.WorkflowSteps.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == stepId, ct);
        if (step is null)
            return WorkflowActResult.Rejected("Langkah tidak ditemukan.");

        var instance = await db.WorkflowInstances.IgnoreQueryFilters().FirstOrDefaultAsync(i => i.Id == step.WorkflowInstanceId, ct);
        if (instance is null)
            return WorkflowActResult.Rejected("Workflow instance tidak ditemukan.");

        var company = await db.Companies.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == instance.CompanyId, ct);
        if (company is null)
            return WorkflowActResult.Rejected("Berkas perusahaan tidak ditemukan.");

        if (!PermissionEvaluator.CanAct(actor, Capability.ReassignWorkflowStep, isUsersTurn: true))
        {
            return WorkflowActResult.Rejected("Anda tidak memiliki hak akses reassign langkah persetujuan.");
        }

        if (!await IsScopedToCompanyAsync(db, actor, company.AreaId, ct))
        {
            return WorkflowActResult.Rejected("Berkas ini berada di luar wilayah Anda.");
        }

        step.AssignedUserId = newUserId;

        var newUserName = await db.Users.AsNoTracking()
            .Where(u => u.Id == newUserId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(ct) ?? "User";

        db.StatusEvents.Add(new StatusEvent
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            ActorId = actorUserId,
            ToStage = company.CurrentStage,
            ToStatus = company.Status,
            Action = StatusEventAction.Reassign,
            WorkflowStepId = step.Id,
            Comment = $"Langkah {step.Kind} ditugaskan kembali kepada {newUserName}.",
            OccurredAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(ct);
        await notifications.SendAsync(newUserId, company.Id, $"Langkah {step.Kind} untuk {company.NamaPerusahaan} telah ditugaskan kepada Anda.", ct);

        return WorkflowActResult.Ok(company.Status);
    }

    public async Task<WorkflowActResult> ReworkAsync(
        Guid companyId,
        string? comment,
        Guid actorUserId,
        EffectivePermissions actor,
        CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == companyId, ct);
        if (company is null)
            return WorkflowActResult.Rejected("Berkas perusahaan tidak ditemukan.");

        if (!PermissionEvaluator.CanAct(actor, Capability.ReassignWorkflowStep, isUsersTurn: true))
        {
            return WorkflowActResult.Rejected("Anda tidak memiliki hak akses untuk mengembalikan berkas ditolak.");
        }

        if (!await IsScopedToCompanyAsync(db, actor, company.AreaId, ct))
        {
            return WorkflowActResult.Rejected("Berkas ini berada di luar wilayah Anda.");
        }

        var transition = WorkflowTransitions.Rework(company.Status, comment);
        if (!transition.Success)
        {
            return WorkflowActResult.Rejected(transition.Error!);
        }

        var fromStatus = company.Status;
        company.Status = transition.NewStatus!.Value;

        db.StatusEvents.Add(new StatusEvent
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            ActorId = actorUserId,
            FromStage = company.CurrentStage,
            ToStage = company.CurrentStage,
            FromStatus = fromStatus,
            ToStatus = company.Status,
            Action = StatusEventAction.Rework,
            Comment = comment,
            OccurredAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(ct);
        await notifications.SendAsync(company.CreatedBy, company.Id, $"Berkas {company.NamaPerusahaan} telah dikembalikan ke Draft untuk diperbaiki.", ct);

        return WorkflowActResult.Ok(company.Status);
    }

    public async Task<WorkflowActResult> DiscontinueAsync(
        Guid companyId,
        string comment,
        Guid actorUserId,
        EffectivePermissions actor,
        CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == companyId, ct);
        if (company is null)
            return WorkflowActResult.Rejected("Berkas perusahaan tidak ditemukan.");

        if (!PermissionEvaluator.CanAct(actor, Capability.ReassignWorkflowStep, isUsersTurn: true))
        {
            return WorkflowActResult.Rejected("Anda tidak memiliki hak akses untuk menghentikan berkas ditolak.");
        }

        if (!await IsScopedToCompanyAsync(db, actor, company.AreaId, ct))
        {
            return WorkflowActResult.Rejected("Berkas ini berada di luar wilayah Anda.");
        }

        var transition = WorkflowTransitions.Discontinue(company.Status, comment);
        if (!transition.Success)
        {
            return WorkflowActResult.Rejected(transition.Error!);
        }

        var fromStatus = company.Status;
        var now = DateTimeOffset.UtcNow;
        company.Status = transition.NewStatus!.Value;

        var instance = await db.WorkflowInstances.IgnoreQueryFilters().FirstOrDefaultAsync(i => i.CompanyId == companyId && i.CompletedAt == null, ct);
        if (instance is not null)
        {
            instance.CompletedAt = now;
            instance.FinalStatus = company.Status;
        }

        db.StatusEvents.Add(new StatusEvent
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            ActorId = actorUserId,
            FromStage = company.CurrentStage,
            ToStage = company.CurrentStage,
            FromStatus = fromStatus,
            ToStatus = company.Status,
            Action = StatusEventAction.Discontinue,
            Comment = comment,
            OccurredAt = now
        });

        await db.SaveChangesAsync(ct);
        await notifications.SendAsync(company.CreatedBy, company.Id, $"Berkas {company.NamaPerusahaan} telah dihentikan.", ct);

        return WorkflowActResult.Ok(company.Status);
    }

    public async Task<IReadOnlyList<StuckStepItemDto>> GetStuckStepsAsync(
        EffectivePermissions actor,
        CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var instances = await db.WorkflowInstances.IgnoreQueryFilters().AsNoTracking()
            .Where(i => i.CompletedAt == null)
            .ToListAsync(ct);

        var instanceIds = instances.Select(i => i.Id).ToHashSet();
        var companyIds = instances.Select(i => i.CompanyId).ToHashSet();

        var steps = await db.WorkflowSteps.IgnoreQueryFilters().AsNoTracking()
            .Where(s => instanceIds.Contains(s.WorkflowInstanceId) && s.ActedAt == null)
            .ToListAsync(ct);

        var companies = await db.Companies.IgnoreQueryFilters().AsNoTracking()
            .Where(c => companyIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, ct);

        var areaIds = companies.Values.Select(c => c.AreaId).ToHashSet();
        var areas = await db.Areas.IgnoreQueryFilters().AsNoTracking()
            .Where(a => areaIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, ct);

        var regionIds = areas.Values.Select(a => a.RegionId).ToHashSet();
        var regions = await db.Regions.IgnoreQueryFilters().AsNoTracking()
            .Where(r => regionIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => r.Name, ct);

        var assignedUserIds = steps.Where(s => s.AssignedUserId.HasValue).Select(s => s.AssignedUserId!.Value).ToHashSet();
        var users = await db.Users.AsNoTracking()
            .Where(u => assignedUserIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, ct);

        var instanceDict = instances.ToDictionary(i => i.Id);
        var now = DateTimeOffset.UtcNow;

        var result = new List<StuckStepItemDto>();
        foreach (var step in steps)
        {
            if (!instanceDict.TryGetValue(step.WorkflowInstanceId, out var inst)) continue;
            if (!companies.TryGetValue(inst.CompanyId, out var comp)) continue;

            var area = areas.GetValueOrDefault(comp.AreaId);
            var regionName = area is not null ? regions.GetValueOrDefault(area.RegionId, "-") : "-";
            var areaName = area?.Name ?? "-";
            var regionId = area?.RegionId ?? Guid.Empty;

            if (actor.Scope == AccessScope.Region && actor.RegionId.HasValue && regionId != actor.RegionId.Value)
            {
                continue;
            }
            if (actor.Scope == AccessScope.Area && actor.AreaId.HasValue && comp.AreaId != actor.AreaId.Value)
            {
                continue;
            }

            var startedAt = inst.StartedAt;
            var elapsedDays = Math.Max(0, (int)(now - startedAt).TotalDays);

            var assignedName = step.AssignedUserId.HasValue
                ? users.GetValueOrDefault(step.AssignedUserId.Value, "Ditugaskan")
                : $"Peran: {WorkflowLabels.StepKindLabel(step.Kind)}";

            result.Add(new StuckStepItemDto(
                step.Id,
                inst.Id,
                comp.Id,
                comp.Nomor,
                comp.NamaPerusahaan,
                regionId,
                regionName,
                comp.AreaId,
                areaName,
                step.Kind,
                step.AssignedUserId,
                assignedName,
                startedAt,
                elapsedDays));
        }

        return result;
    }

    private async Task NotifyTransitionAsync(SimandoDbContext db, Company company, Guid instanceId, CancellationToken ct)
    {
        switch (company.Status)
        {
            case RecordStatus.Draft:
                await notifications.SendAsync(company.CreatedBy, company.Id, $"Dikembalikan untuk revisi — {company.NamaPerusahaan}", ct);
                break;
            case RecordStatus.Rejected:
                var rejectedArea = await db.Areas.FirstOrDefaultAsync(a => a.Id == company.AreaId, ct);
                if (rejectedArea is not null)
                {
                    await NotifyRoleHoldersAsync(db, Role.RegionalAdmin, rejectedArea.RegionId, company.Id, $"Ditolak : {company.NamaPerusahaan}", ct);
                }
                break;
            case RecordStatus.IssuedNol:
                await notifications.SendAsync(company.CreatedBy, company.Id, $"NOL terbit — {company.NamaPerusahaan}", ct);
                break;
            case RecordStatus.IssuedRl:
                await notifications.SendAsync(company.CreatedBy, company.Id, $"RL terbit — {company.NamaPerusahaan}", ct);
                break;
            default:
                var kind = WorkflowStepAssignment.CurrentStepKind(company.Status);
                if (kind is not null)
                {
                    await NotifyStepHoldersAsync(db, company, instanceId, kind.Value, $"Menunggu persetujuan Anda — {company.NamaPerusahaan}", ct);
                }

                break;
        }
    }

    private async Task NotifyStepHoldersAsync(SimandoDbContext db, Company company, Guid instanceId, WorkflowStepKind kind, string message, CancellationToken ct)
    {
        if (kind is WorkflowStepKind.Reviewer1 or WorkflowStepKind.Reviewer2 or WorkflowStepKind.Reviewer3)
        {
            var step = await db.WorkflowSteps.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.WorkflowInstanceId == instanceId && s.Kind == kind, ct);
            if (step?.AssignedUserId is { } assignedUserId)
            {
                await notifications.SendAsync(assignedUserId, company.Id, message, ct);
            }

            return;
        }

        var role = WorkflowStepAssignment.RequiredRole(kind);
        var area = await db.Areas.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == company.AreaId, ct);
        var scopeId = kind == WorkflowStepKind.AreaHead
            ? company.AreaId
            : (area?.RegionId ?? Guid.Empty);

        await NotifyRoleHoldersAsync(db, role, scopeId, company.Id, message, ct);
    }

    private async Task NotifyRoleHoldersAsync(SimandoDbContext db, Role role, Guid scopeId, Guid companyId, string message, CancellationToken ct)
    {
        var recipientUserIds = await db.RoleAssignments.IgnoreQueryFilters()
            .Where(a => a.Active && a.Role == role && (a.AreaId == scopeId || a.RegionId == scopeId))
            .Select(a => a.UserId)
            .ToListAsync(ct);

        foreach (var recipientUserId in recipientUserIds)
        {
            await notifications.SendAsync(recipientUserId, companyId, message, ct);
        }
    }

    private static async Task<bool> IsScopedToCompanyAsync(SimandoDbContext db, EffectivePermissions actor, Guid companyAreaId, CancellationToken ct)
    {
        var area = await db.Areas.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == companyAreaId, ct);
        if (area is null) return false;
        return PermissionEvaluator.CanView(actor.Scope, actor.AreaId, actor.RegionId, companyAreaId, area.RegionId);
    }

    private static StatusEventAction ToStatusEventAction(WorkflowAction action) => action switch
    {
        WorkflowAction.Setuju => StatusEventAction.Setuju,
        WorkflowAction.Revisi => StatusEventAction.Revisi,
        WorkflowAction.Tolak => StatusEventAction.Tolak,
        WorkflowAction.TidakLayak => StatusEventAction.Issue,
        _ => throw new ArgumentOutOfRangeException(nameof(action)),
    };
}
