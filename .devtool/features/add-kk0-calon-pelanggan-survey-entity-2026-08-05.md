---
id: "add-kk0-calon-pelanggan-survey-entity-2026-08-05"
status: "backlog"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-05T09:00:00.000Z"
completedAt: null
labels: ["epic-kk0", "backend", "data-model"]
order: "aU"
---

# Add KK0 (calon pelanggan survey) entity

Add the KK0 survey entity — a separate, more detailed prospective-customer survey form distinct from the main A1 registration, entirely absent from the codebase today.

## Context
The spec's second worksheet tab ("KK0 PT PERUSAHAAN GAS NEGARA...") is a standalone survey form with overlapping but distinct fields from A1 (e.g. separate office/installation-location address, tagging, ownership status). It appears to be used at the Survey workflow stage.

## Acceptance Criteria
- [ ] `Kk0Survey` entity added, FK to `Subscription`, covering company/contact/address fields distinct from A1's structure
- [ ] EF Core migration created and applied
- [ ] DTOs added to `Shared/Models.cs`
