namespace Simando.Domain.Workflow;

// One per submission-to-terminal-state attempt on a Company — a rejected and
// reworked record resubmits into a NEW instance, so a Company can have more
// than one over its lifetime. Bare entity, not AuditableEntity: no
// soft-delete, and IUnitOfWorkScope.Repository<T> is constrained to
// AuditableEntity anyway — same reasoning as StatusEvent/Region/Area.
public sealed class WorkflowInstance
{
    public required Guid Id { get; init; }
    public required Guid CompanyId { get; init; }

    // Not known at StartAsync — Regional Admin picks 2 or 3 reviewers later,
    // via ChooseReviewersAsync, once the case reaches their step.
    public ReviewerCount? ReviewerCount { get; set; }

    public required DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; set; }
    public RecordStatus? FinalStatus { get; set; }
}
