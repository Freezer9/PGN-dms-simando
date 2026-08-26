namespace Simando.Domain.Workflow;

// docs/domain/02-process-flow.md §"Status vocabulary" / docs/design/approval-workflow.md
// §"Action vocabulary". SAVE and SUBMIT aren't part of this set — SAVE is not
// a workflow action at all (no state change), and SUBMIT is Draft -> AreaHead,
// handled separately by WorkflowTransitions.Submit.
//
// TidakLayak ("not feasible") is the Division Head's fourth verb, distinct
// from Setuju in both source docs' state diagrams
// (APPROVAL --> ISSUED_RL : tidak layak) and docs/build/testing.md W7.
public enum WorkflowAction
{
    Setuju,
    Revisi,
    Tolak,
    TidakLayak,
}
