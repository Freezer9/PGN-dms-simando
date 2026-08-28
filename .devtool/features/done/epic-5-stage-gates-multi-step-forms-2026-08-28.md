---
id: "epic-5-stage-gates-multi-step-forms-2026-08-28"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-28T00:00:00.000Z"
modified: "2026-08-28T17:25:00.000Z"
completedAt: "2026-08-28T17:25:00.000Z"
labels: ["backend", "frontend", "stage-gates", "tanstack-form", "workflow"]
order: "a4"
---

# Epic 5: Stage Gates Multi-Step Forms & Submissions

Build full forms and submission endpoints for Stages 4–8 (KK0 Survey, Formulir A1, Permohonan NOL, Evaluasi NOL & Resume, Penerbitan Surat NOL) with TanStack Form and Zod validation.

## User Stories & Scope

- [x] **Story 5.1:** Stage 4 (KK0 Survey Form): Backend API (`GET/PUT /api/companies/{id}/survey`, `POST /api/companies/{id}/survey/submit`) & Frontend form using **TanStack Form** with repeating array fields (Equipment, Products, Fuel Costs, Gas Conversion calculation display).
- [x] **Story 5.2:** Stage 5 (Formulir A1 Registration): Backend API (`GET/PUT /api/companies/{id}/registration`, `POST /api/companies/{id}/registration/submit`) & Frontend form using **TanStack Form** (Legal identities, billing, tax data, required attachment upload slots).
- [x] **Story 5.3:** Stage 6 (Permohonan NOL): Backend API (`GET/PUT /api/companies/{id}/nol-request`, `POST /api/companies/{id}/nol-request/submit`) & Frontend form using **TanStack Form** (Gas demand specs, pressure/volume, reviewer selection).
- [x] **Story 5.4:** Stage 7 (Evaluasi NOL & Resume): Backend API (`GET/PUT /api/companies/{id}/nol-evaluation`, `POST /api/companies/{id}/nol-evaluation/submit`) & Frontend review form using **TanStack Form** with technical/commercial feasibility assessment.
- [x] **Story 5.5:** Stage 8 (Penerbitan Surat NOL): Backend API (`GET/PUT /api/companies/{id}/nol-issuance`, `POST /api/companies/{id}/nol-issuance/issue`) & Frontend form using **TanStack Form** (Final terms, conditions, document issuance number, signing).

## Acceptance Criteria

1. [x] Survey Form (KK0) validates ~60 fields and dynamic repeating equipment/product/material rows; computes total gas conversion volume accurately.
2. [x] A1 Registration Form validates customer legal data and enforces required file attachment slots before submission.
3. [x] Permohonan NOL captures gas volume periods and dynamically selects 2 or 3 reviewers based on Regional Admin settings.
4. [x] Evaluasi NOL allows evaluators to score feasibility scenarios and compile the evaluation resume.
5. [x] Penerbitan Surat NOL issues the official document number and updates company status to Active/Issued.
