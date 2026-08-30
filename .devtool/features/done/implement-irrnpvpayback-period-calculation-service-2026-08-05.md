---
id: "implement-irrnpvpayback-period-calculation-service-2026-08-05"
status: "done"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-30T00:00:00.000Z"
completedAt: "2026-08-30T00:00:00.000Z"
labels: ["epic-feasibility", "backend", "service"]
order: "aS"
---
# Implement IRR/NPV/Payback Period calculation service

Build the financial feasibility calculation (Internal Rate of Return, Net Present Value, Payback Period) currently shown as blank result fields in the spec with no backing logic anywhere in the codebase.

## Context
Zero matches for `irr`, `npv`, `payback`, or `capex` exist in the current implementation — this is a pure greenfield calculation service. Needs business input on the cash-flow model (revenue assumptions from the pricing tier selection, cost basis from capex data, discount rate assumption).

## Acceptance Criteria
- [x] Clarify cash-flow model/assumptions with business stakeholders before implementation
- [x] `IFeasibilityService` (or similar) computing IRR, NPV, and Payback Period from capex + pricing + expected usage data
- [x] Unit tests covering the calculation logic with known reference values

## Depends On
- **Add PipelineInfo entity** (epic-pipeline) — supplies the final capex figures (pipa induk/service, MRS) this calculation consumes; no separate capex entity is planned.
