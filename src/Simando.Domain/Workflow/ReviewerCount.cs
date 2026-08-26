namespace Simando.Domain.Workflow;

// docs/design/approval-workflow.md "Reviewer configuration": 2 or 3,
// chosen by Regional Admin per case via `Tetapkan Reviewer` — there is no
// workflow_template to derive this from (docs/domain/master-data.md §10),
// so it's an explicit input to every transition rather than something
// WorkflowTransitions looks up itself.
public enum ReviewerCount
{
    Two = 2,
    Three = 3,
}
