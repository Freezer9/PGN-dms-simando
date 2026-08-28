---
id: "epic-8-dashboards-analytics-reporting-2026-08-28"
status: "backlog"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-28T00:00:00.000Z"
modified: "2026-08-28T00:00:00.000Z"
completedAt: null
labels: ["backend", "frontend", "dashboard", "reports", "excel"]
order: "a7"
---

# Epic 8: Dashboards, Analytics & Reporting

Deliver role-adaptive dashboard views, analytical report screens (Funnel, Gas Demand, Survey Productivity, NOL Outcomes, Ageing), and multi-tab Excel export via ClosedXML.

## User Stories & Scope

- [ ] **Story 8.1:** Backend Dashboard Stats API (`GET /api/dashboard/stats`): Aggregated metrics tailored by role (Sales Area, Regional Admin, Approver, System Admin).
- [ ] **Story 8.2:** Frontend Role-Adaptive Dashboard (`/`): KPI summary cards, active pipeline distribution, pending action alerts, and recent company activities.
- [ ] **Story 8.3:** Backend Report APIs (`/api/reports/funnel`, `/api/reports/gas-demand`, `/api/reports/survey-productivity`, `/api/reports/nol-outcomes`, `/api/reports/ageing`, plus `/export` xlsx endpoints).
- [ ] **Story 8.4:** Frontend Reports Hub & Report Screens (`/reports/*`): Interactive visual charts, data tables with TanStack Table, date/area filters, and Excel download actions.

## Acceptance Criteria

1. Dashboard renders relevant KPIs according to the authenticated user's active role.
2. All 5 report views display accurate aggregated metrics with area/region filters.
3. Excel exports stream valid formatted `.xlsx` files generated with ClosedXML.
4. PDP Law (UU 27/2022) compliance is enforced on directory export: PII contact fields are masked unless user has `Capability.ExportContactDataPii`.
