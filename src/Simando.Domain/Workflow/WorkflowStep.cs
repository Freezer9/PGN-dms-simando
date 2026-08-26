using Simando.Domain.Security;

namespace Simando.Domain.Workflow;

// One row per chain slot. No separate "current step" flag: the active step
// is the row whose Kind matches Company.Status (see
// WorkflowStepAssignment.CurrentStepKind) with ActedAt == null. Company.Status
// stays the single source of truth for chain position; these rows are
// structural/historical. Bare entity — same reasoning as WorkflowInstance.
public sealed class WorkflowStep
{
    public required Guid Id { get; init; }
    public required Guid WorkflowInstanceId { get; init; }
    public required int StepOrder { get; init; }
    public required WorkflowStepKind Kind { get; init; }

    // Null for role-resolved kinds (AreaHead/RegionalAdmin/DivisionHead —
    // any user holding that role+scope may act); set for Reviewer1-3 once
    // ChooseReviewersAsync assigns a specific chosen reviewer.
    public Guid? AssignedUserId { get; set; }

    public DateTimeOffset? ActedAt { get; set; }
    public Guid? ActedBy { get; set; }
    public WorkflowAction? Action { get; set; }
    public string? Comment { get; set; }
}
