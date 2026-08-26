namespace Simando.Domain.Audit;

// docs/design/data-model.md#status_event--the-visibility-engine. Broader
// than Workflow.WorkflowAction (Setuju/Revisi/Tolak/TidakLayak) — this is
// the audit log's own vocabulary, covering non-decision events too.
public enum StatusEventAction
{
    Create,
    Save,
    Submit,
    Setuju,
    Revisi,
    Tolak,
    Issue,
    BreakGlass,
    Reassign,
    Rework,
    Discontinue,
}
