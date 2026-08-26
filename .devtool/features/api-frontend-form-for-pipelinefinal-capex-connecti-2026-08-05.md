---
id: "api-frontend-form-for-pipelinefinal-capex-connecti-2026-08-05"
status: "backlog"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-05T09:00:00.000Z"
completedAt: null
labels: ["epic-pipeline", "api", "frontend"]
order: "aI"
---

# API + frontend form for pipeline/final capex connection data

Expose and render the pipeline/connection data entry, split across the Survey stage (early estimate) and the NOL/Capex stage (final data), matching where each appears in the spec's workflow.

## Context
Depends on the entity card above. Note the spec's field is entered twice at different workflow stages with different precision — the UI should make clear which is the early estimate vs. the final surveyed value.

## Acceptance Criteria
- [ ] API endpoints to read/update `PipelineInfo` fields, scoped by workflow stage
- [ ] Survey-stage form section: jarak/diameter/tekanan estimate
- [ ] NOL/Capex-stage form section: pipa induk/service length+diameter, MRS spec
