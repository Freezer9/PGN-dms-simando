namespace Simando.Domain.Workflow;

// docs/design/approval-workflow.md "The three transitions" + "Status state
// machine"; test IDs referenced below are docs/build/testing.md §4 (W1-W16).
// Pure functions, no I/O — Simando.Domain has no EF Core dependency so these
// rules run and are demonstrable without a database (AGENTS.md).
//
// This does not re-check who may call it (scope/capability/turn — see
// Simando.Domain.Security.PermissionEvaluator) or segregation of duties
// (creator/stage-7-editor self-approval). Those are the caller's
// responsibility: "Enforce server-side in the workflow service, not in the
// UI" applies to both layers, kept separate on purpose.
public static class WorkflowTransitions
{
    // W1: DRAFT -> AreaHead. "Chain snapshotted" refers to WorkflowStep rows
    // an Application-layer workflow service creates alongside this status
    // change — nothing to snapshot in a pure status transition.
    public static WorkflowTransitionResult Submit(RecordStatus current) =>
        current == RecordStatus.Draft
            ? WorkflowTransitionResult.Ok(RecordStatus.AreaHead)
            : WorkflowTransitionResult.Fail($"Only a {RecordStatus.Draft} record can be submitted.");

    // REJECTED -> DRAFT. Named in both source docs' state diagrams only as
    // "Regional Admin reworks / reassigns" — no action verb is given the way
    // SUBMIT/SETUJU/REVISI/TOLAK are, and docs/build/testing.md's W-series
    // doesn't cover it. Modelled as its own operation rather than folded into
    // WorkflowAction, since nothing in the sources says it shares that
    // vocabulary (no "turn" concept — Regional Admin owns the whole REJECTED
    // queue). Covers three of the four options on the "Kelola Record
    // Ditolak" screen (docs/design/frontend/08-tasks-and-approvals.md) —
    // "Kembalikan ke pembuat", "Perbaiki sendiri", and "Tetapkan ulang ke
    // sales lain" all land on the same DRAFT status here; they differ only
    // in who is assigned to edit it next, which is an ownership/assignment
    // concern outside this pure status transition. The `reason` requirement
    // mirrors that screen's single shared `Catatan *` field, required across
    // all four options including Discontinue below.
    public static WorkflowTransitionResult Rework(RecordStatus current, string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return WorkflowTransitionResult.Fail("Rework requires a reason.");
        }

        return current == RecordStatus.Rejected
            ? WorkflowTransitionResult.Ok(RecordStatus.Draft)
            : WorkflowTransitionResult.Fail($"Only a {RecordStatus.Rejected} record can be reworked.");
    }

    // REJECTED -> DISCONTINUED. The fourth "Kelola Record Ditolak" option,
    // "Hentikan — tandai tidak dilanjutkan" — see the sourcing note on
    // RecordStatus.Discontinued. Terminal, same as IssuedNol/IssuedRl: no
    // switch case in Apply() below means every further action is rejected.
    public static WorkflowTransitionResult Discontinue(RecordStatus current, string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return WorkflowTransitionResult.Fail("Discontinue requires a reason.");
        }

        return current == RecordStatus.Rejected
            ? WorkflowTransitionResult.Ok(RecordStatus.Discontinued)
            : WorkflowTransitionResult.Fail($"Only a {RecordStatus.Rejected} record can be discontinued.");
    }

    public static WorkflowTransitionResult Apply(
        RecordStatus current,
        WorkflowAction action,
        ReviewerCount reviewerCount,
        string? comment)
    {
        if (RequiresComment(action) && string.IsNullOrWhiteSpace(comment))
        {
            return WorkflowTransitionResult.Fail($"{action} requires a comment."); // W13, W14
        }

        // Tolak jumps to Regional Admin "regardless of current position" —
        // not "back to sender". Valid from every active approval step,
        // including Regional Admin's own step (W12) and Division Head
        // (W11): the state-diagram mermaid in both source docs only draws
        // this edge from AREA_HEAD and IN_REVIEW, omitting REGIONAL_ADMIN
        // and APPROVAL, but the prose and the W10-W12 test rows are explicit
        // that every step behaves the same way.
        if (action == WorkflowAction.Tolak)
        {
            return IsActiveApprovalStep(current)
                ? WorkflowTransitionResult.Ok(RecordStatus.Rejected)
                : Unsupported(current, action);
        }

        return current switch
        {
            RecordStatus.AreaHead => action switch
            {
                WorkflowAction.Setuju => WorkflowTransitionResult.Ok(RecordStatus.RegionalAdmin), // W2
                WorkflowAction.Revisi => WorkflowTransitionResult.Ok(RecordStatus.Draft), // W9
                _ => Unsupported(current, action),
            },

            RecordStatus.RegionalAdmin => action switch
            {
                WorkflowAction.Setuju => WorkflowTransitionResult.Ok(RecordStatus.Reviewer1), // W3
                WorkflowAction.Revisi => WorkflowTransitionResult.Ok(RecordStatus.AreaHead),
                _ => Unsupported(current, action),
            },

            RecordStatus.Reviewer1 => action switch
            {
                WorkflowAction.Setuju => WorkflowTransitionResult.Ok(RecordStatus.Reviewer2), // W4
                WorkflowAction.Revisi => WorkflowTransitionResult.Ok(RecordStatus.RegionalAdmin),
                _ => Unsupported(current, action),
            },

            RecordStatus.Reviewer2 => action switch
            {
                // W5/W16: a 2-reviewer chain skips slot 3 entirely.
                WorkflowAction.Setuju => WorkflowTransitionResult.Ok(
                    reviewerCount == ReviewerCount.Three ? RecordStatus.Reviewer3 : RecordStatus.Approval),
                WorkflowAction.Revisi => WorkflowTransitionResult.Ok(RecordStatus.Reviewer1), // W8
                _ => Unsupported(current, action),
            },

            RecordStatus.Reviewer3 => reviewerCount == ReviewerCount.Three
                ? action switch
                {
                    WorkflowAction.Setuju => WorkflowTransitionResult.Ok(RecordStatus.Approval), // W5
                    WorkflowAction.Revisi => WorkflowTransitionResult.Ok(RecordStatus.Reviewer2),
                    _ => Unsupported(current, action),
                }
                : WorkflowTransitionResult.Fail("Reviewer 3 does not exist in a 2-reviewer chain."),

            RecordStatus.Approval => action switch
            {
                WorkflowAction.Setuju => WorkflowTransitionResult.Ok(RecordStatus.IssuedNol), // W6
                WorkflowAction.TidakLayak => WorkflowTransitionResult.Ok(RecordStatus.IssuedRl), // W7
                // One step back to whichever reviewer slot was last in this
                // case's chain.
                WorkflowAction.Revisi => WorkflowTransitionResult.Ok(
                    reviewerCount == ReviewerCount.Three ? RecordStatus.Reviewer3 : RecordStatus.Reviewer2),
                _ => Unsupported(current, action),
            },

            // Draft, Rejected, IssuedNol, IssuedRl, Discontinued: no
            // Setuju/Revisi/TidakLayak is ever valid (W15 — terminal states
            // reject every action; Draft moves only via Submit; Rejected
            // moves only via Rework or Discontinue).
            _ => Unsupported(current, action),
        };
    }

    private static bool IsActiveApprovalStep(RecordStatus status) => status is
        RecordStatus.AreaHead or
        RecordStatus.RegionalAdmin or
        RecordStatus.Reviewer1 or
        RecordStatus.Reviewer2 or
        RecordStatus.Reviewer3 or
        RecordStatus.Approval;

    private static bool RequiresComment(WorkflowAction action) =>
        action is WorkflowAction.Revisi or WorkflowAction.Tolak;

    private static WorkflowTransitionResult Unsupported(RecordStatus current, WorkflowAction action) =>
        WorkflowTransitionResult.Fail($"{action} is not valid from {current}.");
}
