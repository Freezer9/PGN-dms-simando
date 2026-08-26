namespace Simando.Domain.Workflow;

// docs/domain/02-process-flow.md#status-vocabulary / docs/design/approval-workflow.md
// "Status state machine". IN_REVIEW's three reviewer slots (R1/R2/R3) are
// flattened into their own members here rather than kept as a nested
// composite state — same states, easier to switch over.
public enum RecordStatus
{
    Draft,
    AreaHead,
    RegionalAdmin,
    Reviewer1,
    Reviewer2,
    Reviewer3,
    Approval, // Division Head
    Rejected,
    IssuedNol,
    IssuedRl,

    // Not in any canonical doc's state diagram (approval-workflow.md,
    // 02-process-flow.md) or the W-series test table
    // (docs/build/testing.md). Inferred from three independent, mutually
    // corroborating sources instead: docs/design/frontend/08-tasks-and-approvals.md's
    // "Kelola Record Ditolak" screen ("Hentikan — tandai tidak dilanjutkan"),
    // its cross-reference in docs/design/frontend/14-role-navigation-guide.md,
    // and docs/end-to-end-walkthrough.md's prose/diagram ("kill the case
    // outright"). Flagged here the way the docs mark an [ASSUMPTION] — real
    // enough to model, never confirmed by a canonical source.
    Discontinued,
}
