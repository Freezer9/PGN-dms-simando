---
id: "admin-ui-masterorganisation-2026-08-07"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-08T11:41:05.000Z"
completedAt: "2026-08-08T11:41:05.000Z"
labels: ["admin-ui", "phase-0"]
order: "a00j"
---

# Admin UI: /master/organisation

Region (SOR) & Area CRUD. Deleting an Area that holds records is blocked (deactivate instead); moving an Area between Regions changes record visibility and requires confirmation + audit log.

Shipped as a bespoke `IOrganisationService` (not `IEntityService<T>` — Region/Area aren't `AuditableEntity`, no soft-delete). Hard delete is attempted and Postgres's `ON DELETE RESTRICT` FK rejects it when the row is still referenced (`23001`/`23503`, translated to `EntityInUseException` via a new shared `PersistenceExceptionTranslator`), so no manual "has children" check was needed. Area→Region reassignment requires a confirm dialog explaining the visibility change; the "audit log" half of the original ask was deliberately dropped from scope — no generic admin-action audit log exists in this codebase yet, and building one from scratch for this one screen was disproportionate. Tracked separately.
