using Simando.Domain.Audit;
using Simando.Domain.Security;
using Simando.Domain.Workflow;

namespace Simando.Application.Tasks;

// "Riwayat Tindakan" — the actor's own past workflow decisions, sourced from
// StatusEvent rather than WorkflowStep, so it survives even if the step it
// was made on later becomes part of a completed/rejected instance. Action is
// StatusEventAction (not WorkflowAction) since a Submit row belongs here too
// when the actor is also the creator.
public sealed record TaskHistoryItem(
    Guid CompanyId,
    string Nomor,
    string NamaPerusahaan,
    StatusEventAction Action,
    RecordStatus ToStatus,
    string? Comment,
    DateTimeOffset ActedAt);
