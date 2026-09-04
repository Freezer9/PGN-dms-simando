using Shouldly;
using Simando.Domain.Workflow;

namespace Simando.Domain.Tests.Workflow;

// Test IDs (W#) match docs/build/03-testing.md §4 so a failing test names
// the doc row it covers.
public class WorkflowTransitionsTests
{
    private const string Comment = "alasan revisi/tolak";

    [Fact(DisplayName = "W1: DRAFT submit -> Area Head")]
    public void W1()
    {
        var result = WorkflowTransitions.Submit(RecordStatus.Draft);

        result.Success.ShouldBeTrue();
        result.NewStatus.ShouldBe(RecordStatus.AreaHead);
    }

    [Fact(DisplayName = "W2: Area Head setuju -> Regional Admin")]
    public void W2()
    {
        var result = WorkflowTransitions.Apply(RecordStatus.AreaHead, WorkflowAction.Setuju, ReviewerCount.Two, null);

        result.Success.ShouldBeTrue();
        result.NewStatus.ShouldBe(RecordStatus.RegionalAdmin);
    }

    [Fact(DisplayName = "W3: Regional Admin setuju -> Reviewer 1")]
    public void W3()
    {
        var result = WorkflowTransitions.Apply(RecordStatus.RegionalAdmin, WorkflowAction.Setuju, ReviewerCount.Two, null);

        result.Success.ShouldBeTrue();
        result.NewStatus.ShouldBe(RecordStatus.Reviewer1);
    }

    [Fact(DisplayName = "W4: Reviewer 1 setuju -> Reviewer 2")]
    public void W4()
    {
        var result = WorkflowTransitions.Apply(RecordStatus.Reviewer1, WorkflowAction.Setuju, ReviewerCount.Three, null);

        result.Success.ShouldBeTrue();
        result.NewStatus.ShouldBe(RecordStatus.Reviewer2);
    }

    [Fact(DisplayName = "W5: last Reviewer setuju (3-reviewer chain) -> Division Head")]
    public void W5_ThreeReviewerChain()
    {
        var result = WorkflowTransitions.Apply(RecordStatus.Reviewer3, WorkflowAction.Setuju, ReviewerCount.Three, null);

        result.Success.ShouldBeTrue();
        result.NewStatus.ShouldBe(RecordStatus.Approval);
    }

    [Fact(DisplayName = "W6: Division Head setuju -> ISSUED_NOL")]
    public void W6()
    {
        var result = WorkflowTransitions.Apply(RecordStatus.Approval, WorkflowAction.Setuju, ReviewerCount.Two, null);

        result.Success.ShouldBeTrue();
        result.NewStatus.ShouldBe(RecordStatus.IssuedNol);
    }

    [Fact(DisplayName = "W7: Division Head tidak layak -> ISSUED_RL")]
    public void W7()
    {
        var result = WorkflowTransitions.Apply(RecordStatus.Approval, WorkflowAction.TidakLayak, ReviewerCount.Two, null);

        result.Success.ShouldBeTrue();
        result.NewStatus.ShouldBe(RecordStatus.IssuedRl);
    }

    [Fact(DisplayName = "W8: Reviewer 2 revisi -> Reviewer 1 (exactly one step back)")]
    public void W8()
    {
        var result = WorkflowTransitions.Apply(RecordStatus.Reviewer2, WorkflowAction.Revisi, ReviewerCount.Three, Comment);

        result.Success.ShouldBeTrue();
        result.NewStatus.ShouldBe(RecordStatus.Reviewer1);
    }

    [Fact(DisplayName = "W9: Area Head revisi -> DRAFT, back to creator")]
    public void W9()
    {
        var result = WorkflowTransitions.Apply(RecordStatus.AreaHead, WorkflowAction.Revisi, ReviewerCount.Two, Comment);

        result.Success.ShouldBeTrue();
        result.NewStatus.ShouldBe(RecordStatus.Draft);
    }

    [Fact(DisplayName = "W10: Reviewer 2 tolak -> Regional Admin, not the creator")]
    public void W10()
    {
        var result = WorkflowTransitions.Apply(RecordStatus.Reviewer2, WorkflowAction.Tolak, ReviewerCount.Three, Comment);

        result.Success.ShouldBeTrue();
        result.NewStatus.ShouldBe(RecordStatus.Rejected);
    }

    [Fact(DisplayName = "W11: Division Head tolak -> Regional Admin")]
    public void W11()
    {
        var result = WorkflowTransitions.Apply(RecordStatus.Approval, WorkflowAction.Tolak, ReviewerCount.Two, Comment);

        result.Success.ShouldBeTrue();
        result.NewStatus.ShouldBe(RecordStatus.Rejected);
    }

    [Fact(DisplayName = "W12: Regional Admin tolak -> own queue, still recorded")]
    public void W12()
    {
        var result = WorkflowTransitions.Apply(RecordStatus.RegionalAdmin, WorkflowAction.Tolak, ReviewerCount.Two, Comment);

        result.Success.ShouldBeTrue();
        result.NewStatus.ShouldBe(RecordStatus.Rejected);
    }

    [Theory(DisplayName = "W13: revisi with an empty comment is rejected, from any step")]
    [InlineData(RecordStatus.AreaHead)]
    [InlineData(RecordStatus.RegionalAdmin)]
    [InlineData(RecordStatus.Reviewer1)]
    [InlineData(RecordStatus.Approval)]
    public void W13(RecordStatus step)
    {
        WorkflowTransitions.Apply(step, WorkflowAction.Revisi, ReviewerCount.Three, null).Success.ShouldBeFalse();
        WorkflowTransitions.Apply(step, WorkflowAction.Revisi, ReviewerCount.Three, "").Success.ShouldBeFalse();
        WorkflowTransitions.Apply(step, WorkflowAction.Revisi, ReviewerCount.Three, "   ").Success.ShouldBeFalse();
    }

    [Theory(DisplayName = "W14: tolak with an empty comment is rejected, from any step")]
    [InlineData(RecordStatus.AreaHead)]
    [InlineData(RecordStatus.RegionalAdmin)]
    [InlineData(RecordStatus.Reviewer2)]
    [InlineData(RecordStatus.Approval)]
    public void W14(RecordStatus step)
    {
        WorkflowTransitions.Apply(step, WorkflowAction.Tolak, ReviewerCount.Three, null).Success.ShouldBeFalse();
        WorkflowTransitions.Apply(step, WorkflowAction.Tolak, ReviewerCount.Three, "").Success.ShouldBeFalse();
    }

    [Theory(DisplayName = "W15: ISSUED_NOL is terminal; every action is rejected")]
    [InlineData(WorkflowAction.Setuju)]
    [InlineData(WorkflowAction.Revisi)]
    [InlineData(WorkflowAction.Tolak)]
    [InlineData(WorkflowAction.TidakLayak)]
    public void W15(WorkflowAction action)
    {
        var comment = action is WorkflowAction.Revisi or WorkflowAction.Tolak ? Comment : null;
        var result = WorkflowTransitions.Apply(RecordStatus.IssuedNol, action, ReviewerCount.Two, comment);

        result.Success.ShouldBeFalse();
    }

    [Fact(DisplayName = "W15b: ISSUED_RL is terminal too")]
    public void W15_IssuedRl()
    {
        WorkflowTransitions.Apply(RecordStatus.IssuedRl, WorkflowAction.Setuju, ReviewerCount.Two, null)
            .Success.ShouldBeFalse();
    }

    [Fact(DisplayName = "W16: 2-reviewer chain — setuju at Reviewer 2 skips slot 3, goes straight to Division Head")]
    public void W16()
    {
        var result = WorkflowTransitions.Apply(RecordStatus.Reviewer2, WorkflowAction.Setuju, ReviewerCount.Two, null);

        result.Success.ShouldBeTrue();
        result.NewStatus.ShouldBe(RecordStatus.Approval);
    }

    // --- Additional edges from the state-diagram, not individually
    // numbered in docs/build/03-testing.md but present in both
    // docs/design/01-approval-workflow.md and docs/domain/02-process-flow.md's
    // mermaid diagrams. ---

    [Fact(DisplayName = "Regional Admin revisi -> Area Head")]
    public void RegionalAdmin_Revisi_ReturnsToAreaHead()
    {
        var result = WorkflowTransitions.Apply(RecordStatus.RegionalAdmin, WorkflowAction.Revisi, ReviewerCount.Two, Comment);

        result.Success.ShouldBeTrue();
        result.NewStatus.ShouldBe(RecordStatus.AreaHead);
    }

    [Fact(DisplayName = "Reviewer 3 revisi -> Reviewer 2, only in a 3-reviewer chain")]
    public void Reviewer3_Revisi_ReturnsToReviewer2()
    {
        var result = WorkflowTransitions.Apply(RecordStatus.Reviewer3, WorkflowAction.Revisi, ReviewerCount.Three, Comment);

        result.Success.ShouldBeTrue();
        result.NewStatus.ShouldBe(RecordStatus.Reviewer2);
    }

    [Fact(DisplayName = "Reviewer 3 does not exist in a 2-reviewer chain")]
    public void Reviewer3_InvalidInTwoReviewerChain()
    {
        var result = WorkflowTransitions.Apply(RecordStatus.Reviewer3, WorkflowAction.Setuju, ReviewerCount.Two, null);

        result.Success.ShouldBeFalse();
    }

    [Theory(DisplayName = "Division Head revisi -> the last reviewer slot for that case's chain")]
    [InlineData(ReviewerCount.Two, RecordStatus.Reviewer2)]
    [InlineData(ReviewerCount.Three, RecordStatus.Reviewer3)]
    public void Approval_Revisi_ReturnsToLastReviewer(ReviewerCount reviewerCount, RecordStatus expected)
    {
        var result = WorkflowTransitions.Apply(RecordStatus.Approval, WorkflowAction.Revisi, reviewerCount, Comment);

        result.Success.ShouldBeTrue();
        result.NewStatus.ShouldBe(expected);
    }

    [Fact(DisplayName = "A DRAFT record cannot be rejected — there is no approver yet")]
    public void Draft_Tolak_Invalid()
    {
        WorkflowTransitions.Apply(RecordStatus.Draft, WorkflowAction.Tolak, ReviewerCount.Two, Comment)
            .Success.ShouldBeFalse();
    }

    [Fact(DisplayName = "An already-REJECTED record cannot be tolak'd again; only Rework/Discontinue move it")]
    public void Rejected_Tolak_Invalid()
    {
        WorkflowTransitions.Apply(RecordStatus.Rejected, WorkflowAction.Tolak, ReviewerCount.Two, Comment)
            .Success.ShouldBeFalse();
    }

    [Fact(DisplayName = "Rework: REJECTED -> DRAFT")]
    public void Rework_RejectedToDraft()
    {
        var result = WorkflowTransitions.Rework(RecordStatus.Rejected, Comment);

        result.Success.ShouldBeTrue();
        result.NewStatus.ShouldBe(RecordStatus.Draft);
    }

    [Fact(DisplayName = "Rework: only valid from REJECTED")]
    public void Rework_OnlyValidFromRejected()
    {
        WorkflowTransitions.Rework(RecordStatus.Draft, Comment).Success.ShouldBeFalse();
        WorkflowTransitions.Rework(RecordStatus.AreaHead, Comment).Success.ShouldBeFalse();
        WorkflowTransitions.Rework(RecordStatus.IssuedNol, Comment).Success.ShouldBeFalse();
    }

    [Theory(DisplayName = "Rework: requires a reason, mirroring the Kelola Record Ditolak screen's shared Catatan field")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rework_RequiresReason(string? reason)
    {
        WorkflowTransitions.Rework(RecordStatus.Rejected, reason).Success.ShouldBeFalse();
    }

    [Fact(DisplayName = "Discontinue: REJECTED -> DISCONTINUED (\"Hentikan\")")]
    public void Discontinue_RejectedToDiscontinued()
    {
        var result = WorkflowTransitions.Discontinue(RecordStatus.Rejected, Comment);

        result.Success.ShouldBeTrue();
        result.NewStatus.ShouldBe(RecordStatus.Discontinued);
    }

    [Fact(DisplayName = "Discontinue: DRAFT -> DISCONTINUED (\"Hentikan Prospek\")")]
    public void Discontinue_DraftToDiscontinued()
    {
        var result = WorkflowTransitions.Discontinue(RecordStatus.Draft, Comment);

        result.Success.ShouldBeTrue();
        result.NewStatus.ShouldBe(RecordStatus.Discontinued);
    }

    [Fact(DisplayName = "Discontinue: invalid from active review steps and terminal statuses")]
    public void Discontinue_InvalidFromActiveSteps()
    {
        WorkflowTransitions.Discontinue(RecordStatus.AreaHead, Comment).Success.ShouldBeFalse();
        WorkflowTransitions.Discontinue(RecordStatus.RegionalAdmin, Comment).Success.ShouldBeFalse();
        WorkflowTransitions.Discontinue(RecordStatus.Reviewer1, Comment).Success.ShouldBeFalse();
        WorkflowTransitions.Discontinue(RecordStatus.IssuedNol, Comment).Success.ShouldBeFalse();
    }

    [Theory(DisplayName = "Discontinue: requires a reason")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Discontinue_RequiresReason(string? reason)
    {
        WorkflowTransitions.Discontinue(RecordStatus.Rejected, reason).Success.ShouldBeFalse();
    }

    [Theory(DisplayName = "DISCONTINUED is terminal too — every workflow action is rejected")]
    [InlineData(WorkflowAction.Setuju)]
    [InlineData(WorkflowAction.Revisi)]
    [InlineData(WorkflowAction.Tolak)]
    [InlineData(WorkflowAction.TidakLayak)]
    public void Discontinued_IsTerminal(WorkflowAction action)
    {
        var comment = action is WorkflowAction.Revisi or WorkflowAction.Tolak ? Comment : null;
        WorkflowTransitions.Apply(RecordStatus.Discontinued, action, ReviewerCount.Two, comment)
            .Success.ShouldBeFalse();
    }

    [Fact(DisplayName = "DISCONTINUED cannot be reworked or discontinued again")]
    public void Discontinued_CannotBeReworkedOrDiscontinuedAgain()
    {
        WorkflowTransitions.Rework(RecordStatus.Discontinued, Comment).Success.ShouldBeFalse();
        WorkflowTransitions.Discontinue(RecordStatus.Discontinued, Comment).Success.ShouldBeFalse();
    }

    [Fact(DisplayName = "Submit: only valid from DRAFT")]
    public void Submit_OnlyValidFromDraft()
    {
        WorkflowTransitions.Submit(RecordStatus.AreaHead).Success.ShouldBeFalse();
        WorkflowTransitions.Submit(RecordStatus.Rejected).Success.ShouldBeFalse();
    }

    [Fact(DisplayName = "Setuju needs no comment")]
    public void Setuju_NoCommentRequired()
    {
        WorkflowTransitions.Apply(RecordStatus.AreaHead, WorkflowAction.Setuju, ReviewerCount.Two, null)
            .Success.ShouldBeTrue();
    }
}
