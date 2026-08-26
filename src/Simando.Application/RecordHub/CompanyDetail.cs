using Simando.Domain.Security;
using Simando.Domain.Workflow;

namespace Simando.Application.RecordHub;

// The /companies/{id} page's whole read model — one flat DTO rather than a
// tab-per-query split, since every section (header, status card, stepper,
// action bar) needs overlapping fields and the page loads them all at once
// anyway. CanSubmit/CanAct/CanChooseReviewers are precomputed server-side
// (same scope+capability+turn+self-approval checks WorkflowService itself
// re-enforces on the actual call) purely for button visibility — same
// precompute-for-display pattern as TasksService.GetMyTasksAsync's
// IsAssignedToStep filtering.
public sealed record CompanyDetail(
    Guid CompanyId,
    string Nomor,
    string NamaPerusahaan,
    string IndustryTypeName,
    string LocationLabel,
    Guid CreatedBy,
    string SalesRepName,
    Guid AreaId,
    string AreaName,
    Guid RegionId,
    string RegionName,
    byte CurrentStage,
    RecordStatus Status,
    string? HolderLabel,
    string? HolderName,
    DateTimeOffset StatusSince,
    Guid? CurrentStepId,
    WorkflowStepKind? CurrentStepKind,
    Guid? WorkflowInstanceId,
    bool CanSubmit,
    bool CanAct,
    bool CanChooseReviewers,
    IReadOnlyList<ContactSummary> Contacts);
