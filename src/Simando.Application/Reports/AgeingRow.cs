using Simando.Domain.Security;

namespace Simando.Application.Reports;

// One row per record currently in the approval workflow, sorted by wait
// time descending (oldest wait first) — docs/design/reporting.md "Ageing —
// the key report". ActorLabel is pre-composed ("Reviewer 2 (Dewi)" or
// plain "Regional Admin" for role-resolved kinds) so the page stays dumb.
public sealed record AgeingRow(
    Guid CompanyId,
    string Nomor,
    string NamaPerusahaan,
    string IndustryTypeName,
    WorkflowStepKind StepKind,
    string AreaName,
    string RegionName,
    string ActorLabel,
    DateTimeOffset WaitingSince);
