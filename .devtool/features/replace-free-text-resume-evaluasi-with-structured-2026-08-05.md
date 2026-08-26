---
id: "replace-free-text-resume-evaluasi-with-structured-2026-08-05"
status: "backlog"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-05T09:00:00.000Z"
completedAt: null
labels: ["epic-summary", "backend", "data-model"]
order: "ab"
---

# Replace free-text Resume Evaluasi with structured Executive Summary entity (Kontrak Min/Maks, Rencana Pemakaian, Produk & Sub Produk table)

Replace (or supplement) the current free-text `ResumeEvaluasi` field with a structured executive-summary table matching the spec's fields.

## Context
`EvaluationController.cs` currently exposes only a free-text evaluation note (`ResumeEvaluasi` entity/table). Spec calls for a structured table: Kontrak Minimum, Kontrak Maksimum, Rencana Pemakaian, Produk dan Sub Produk.

## Acceptance Criteria
- [ ] `ExecutiveSummary` entity added with structured fields, FK to `Subscription`
- [ ] Decide whether this replaces or complements the existing free-text `ResumeEvaluasi` (recommend keeping free-text as a supplementary notes field)
- [ ] EF Core migration created and applied
