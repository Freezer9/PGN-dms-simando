---
id: "add-plotting-data-model-jalur-pipa-status-kawasan-2026-08-05"
status: "done"
priority: "low"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-30T00:00:00.000Z"
completedAt: "2026-08-30T00:00:00.000Z"
labels: ["epic-plotting", "backend", "data-model"]
order: "aY"
---
# Add Plotting data model (jalur pipa status, kawasan industri flag) linked to subscription

Add the fields needed to track a prospect's pipeline-proximity status (Potensi/Existing/etc.) and kawasan industri classification for the Plotting view.

## Context
`SubscriptionStatus.Plotting` exists as a stage name only; the underlying "Jalur Pipa" (Potensi/Existing/Save/Edit action states shown in the spec) and Kawasan Industri classification used to filter/group prospects are not modeled.

## Acceptance Criteria
- [x] `JalurPipaStatus` enum/field and `KawasanIndustri` boolean/enum added to `Subscription` (or wherever plotting-stage data belongs)
- [x] EF Core migration created and applied
