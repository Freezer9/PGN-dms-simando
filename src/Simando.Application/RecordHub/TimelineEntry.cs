using Simando.Domain.Audit;
using Simando.Domain.Workflow;

namespace Simando.Application.RecordHub;

// One persisted StatusEvent row, newest-first — docs/design/frontend/04-record-hub.md
// "Timeline". RoleLabel is the actor's role AT the time of the event (Sales
// Area on Submit, the acted-on WorkflowStep's Kind on Setuju/Revisi/Tolak/
// Issue), not a role looked up from their current assignments. No synthetic
// "reviewer ditetapkan" entry — ChooseReviewersAsync doesn't write a
// StatusEvent, so that mockup detail isn't reproducible from real data.
public sealed record TimelineEntry(
    Guid Id,
    StatusEventAction Action,
    RecordStatus ToStatus,
    string RoleLabel,
    string ActorName,
    string? Comment,
    DateTimeOffset OccurredAt);
