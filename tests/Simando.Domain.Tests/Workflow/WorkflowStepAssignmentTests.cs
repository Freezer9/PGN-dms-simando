using Shouldly;
using Simando.Domain.Security;
using Simando.Domain.Workflow;

namespace Simando.Domain.Tests.Workflow;

public class WorkflowStepAssignmentTests
{
    private static WorkflowStep Step(WorkflowStepKind kind, Guid? assignedUserId = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            WorkflowInstanceId = Guid.NewGuid(),
            StepOrder = 1,
            Kind = kind,
            AssignedUserId = assignedUserId,
        };

    [Theory(DisplayName = "CurrentStepKind: active-chain statuses map to their step kind")]
    [InlineData(RecordStatus.AreaHead, WorkflowStepKind.AreaHead)]
    [InlineData(RecordStatus.RegionalAdmin, WorkflowStepKind.RegionalAdmin)]
    [InlineData(RecordStatus.Reviewer1, WorkflowStepKind.Reviewer1)]
    [InlineData(RecordStatus.Reviewer2, WorkflowStepKind.Reviewer2)]
    [InlineData(RecordStatus.Reviewer3, WorkflowStepKind.Reviewer3)]
    [InlineData(RecordStatus.Approval, WorkflowStepKind.DivisionHead)]
    public void CurrentStepKind_ActiveChain_MapsToStepKind(RecordStatus status, WorkflowStepKind expected)
    {
        WorkflowStepAssignment.CurrentStepKind(status).ShouldBe(expected);
    }

    [Theory(DisplayName = "CurrentStepKind: statuses outside the active chain have no current step")]
    [InlineData(RecordStatus.Draft)]
    [InlineData(RecordStatus.Rejected)]
    [InlineData(RecordStatus.IssuedNol)]
    [InlineData(RecordStatus.IssuedRl)]
    [InlineData(RecordStatus.Discontinued)]
    public void CurrentStepKind_OutsideChain_Null(RecordStatus status)
    {
        WorkflowStepAssignment.CurrentStepKind(status).ShouldBeNull();
    }

    [Theory(DisplayName = "RequiredRole: role-resolved kinds map to their role")]
    [InlineData(WorkflowStepKind.AreaHead, Role.AreaHead)]
    [InlineData(WorkflowStepKind.RegionalAdmin, Role.RegionalAdmin)]
    [InlineData(WorkflowStepKind.DivisionHead, Role.DivisionHead)]
    [InlineData(WorkflowStepKind.Reviewer1, Role.Reviewer)]
    [InlineData(WorkflowStepKind.Reviewer2, Role.Reviewer)]
    [InlineData(WorkflowStepKind.Reviewer3, Role.Reviewer)]
    public void RequiredRole_MapsCorrectly(WorkflowStepKind kind, Role expected)
    {
        WorkflowStepAssignment.RequiredRole(kind).ShouldBe(expected);
    }

    [Fact(DisplayName = "IsAssignedToStep: role-resolved kind matches any user holding the role")]
    public void IsAssignedToStep_RoleResolvedKind_MatchesHolder()
    {
        var step = Step(WorkflowStepKind.AreaHead);
        var actorId = Guid.NewGuid();

        WorkflowStepAssignment.IsAssignedToStep(step, actorId, new HashSet<Role> { Role.AreaHead }).ShouldBeTrue();
        WorkflowStepAssignment.IsAssignedToStep(step, actorId, new HashSet<Role> { Role.Reviewer }).ShouldBeFalse();
    }

    [Fact(DisplayName = "IsAssignedToStep: reviewer kind matches only the specific chosen user")]
    public void IsAssignedToStep_ReviewerKind_MatchesOnlyAssignedUser()
    {
        var assignedUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var step = Step(WorkflowStepKind.Reviewer2, assignedUserId);

        WorkflowStepAssignment.IsAssignedToStep(step, assignedUserId, new HashSet<Role> { Role.Reviewer }).ShouldBeTrue();
        // Holding the Reviewer role isn't enough — must be the specific chosen reviewer.
        WorkflowStepAssignment.IsAssignedToStep(step, otherUserId, new HashSet<Role> { Role.Reviewer }).ShouldBeFalse();
    }
}
