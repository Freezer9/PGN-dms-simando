using Simando.Domain.Workflow;

namespace Simando.Domain.Audit;

// Append-only — the visibility engine. Drives the timeline and every
// ageing metric. Current status is a projection of this log; if they ever
// disagree, the log is right. docs/design/data-model.md#status_event.
// Enforced append-only by a database trigger, not just application
// discipline — see the AddStatusEventAuditTrail migration.
public sealed class StatusEvent
{
    public required Guid Id { get; init; }
    public required Guid CompanyId { get; init; }

    // Null on Create — there is no prior stage/status yet.
    public byte? FromStage { get; init; }
    public required byte ToStage { get; init; }
    public RecordStatus? FromStatus { get; init; }
    public required RecordStatus ToStatus { get; init; }

    public required Guid ActorId { get; init; }
    public required StatusEventAction Action { get; init; }

    // Set when this row is a workflow decision (Submit/Setuju/Revisi/Tolak/
    // Issue acting on a specific WorkflowStep); null for events with no step
    // to point at.
    public Guid? WorkflowStepId { get; init; }

    // NOT NULL enforced for Revisi/Tolak at the application layer — see
    // StatusEventConfiguration.
    public string? Comment { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }
}
