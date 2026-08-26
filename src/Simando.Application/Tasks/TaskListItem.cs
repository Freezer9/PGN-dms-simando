using Simando.Domain.Security;

namespace Simando.Application.Tasks;

// Shared shape for the "Menunggu Saya" and "Semua di Region" tabs — same
// columns in docs/design/frontend/08-tasks-and-approvals.md, they differ only
// in whether the turn check is applied. WaitingDays is computed by the caller
// from WaitingSince, not baked in here — presentation concern.
public sealed record TaskListItem(
    Guid CompanyId,
    string Nomor,
    string NamaPerusahaan,
    string IndustryTypeName,
    Guid StepId,
    WorkflowStepKind StepKind,
    string AreaName,
    string RegionName,
    string SubmittedByName,
    DateTimeOffset WaitingSince);
