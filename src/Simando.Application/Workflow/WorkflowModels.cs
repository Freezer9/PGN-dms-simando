using Simando.Domain.Security;
using Simando.Domain.Workflow;

namespace Simando.Application.Workflow;

public sealed record StuckStepItemDto(
    Guid StepId,
    Guid WorkflowInstanceId,
    Guid CompanyId,
    string CompanyNomor,
    string CompanyName,
    Guid RegionId,
    string RegionName,
    Guid AreaId,
    string AreaName,
    WorkflowStepKind StepKind,
    Guid? AssignedUserId,
    string AssignedUserName,
    DateTimeOffset StartedAt,
    int ElapsedDays
);

public sealed record ReassignStuckStepRequest(Guid StepId, Guid TargetUserId);
