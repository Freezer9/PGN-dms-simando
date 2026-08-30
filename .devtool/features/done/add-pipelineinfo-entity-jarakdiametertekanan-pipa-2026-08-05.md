---
id: "add-pipelineinfo-entity-jarakdiametertekanan-pipa-2026-08-05"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-30T00:00:00.000Z"
completedAt: "2026-08-30T00:00:00.000Z"
labels: ["epic-pipeline", "backend", "data-model"]
order: "aH"
---
# Add PipelineInfo entity (jarak/diameter/tekanan, pipa induk/service, MRS spec)

Model nearest-pipeline distance/diameter/pressure (survey-stage data) and the final capex connection specs (pipa induk/service length+diameter, MRS G.Size/Tekanan/Maks Flowrate).

## Context
Spec captures this in two places: an early "Pipa Gas Terdekat" estimate (distance/diameter/pressure) and a later "Capex - data Final" section (pipa induk/service length in meters + inch/mm diameter, MRS spec). Neither exists in the codebase today.

## Note
This entity is the single source for both the early Survey-stage pipeline estimate *and* the final Capex data (pipa induk/service, MRS) used by the feasibility calculation — a separate "Capex final" entity was considered and folded in here to avoid modeling the same data twice.

## Acceptance Criteria
- [x] `PipelineInfo` entity covering both the early estimate and final capex fields, FK to `Subscription`
- [x] EF Core migration created and applied
- [x] DTOs added to `Shared/Models.cs`
