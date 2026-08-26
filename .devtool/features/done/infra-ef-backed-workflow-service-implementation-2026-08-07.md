---
id: "infra-ef-backed-workflow-service-implementation-2026-08-07"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-08T15:30:00.000Z"
completedAt: "2026-08-08T15:30:00.000Z"
labels: ["workflow", "nol", "phase-5", "infra"]
order: "a00n"
---

# Infra: EF-backed workflow service implementation

Concrete `IWorkflowService`, plus `document_number_counter` allocation for the Nota Dinas number.

Shipped as `WorkflowService` (`src/Simando.Infrastructure/Workflow/WorkflowService.cs`), fresh-`IDbContextFactory`-context-per-call same as `OrganisationService` — each method's writes land in one `SaveChangesAsync`, which EF Core already wraps in one implicit transaction, so no explicit `IUnitOfWork` was needed. Migration `AddWorkflowInstanceAndStep` adds `workflow_instance`/`workflow_step` tables and a nullable `status_event.workflow_step_id` column, applied to dev Postgres.

**`document_number_counter` allocation not built** — there's nowhere to persist a Nota Dinas number yet (`NolIssuance` entity doesn't exist). Deferred to whichever task builds `NolIssuance`; the counter table itself already exists and is untouched.
