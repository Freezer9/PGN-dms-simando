---
id: "add-nolapplication-entity-periode-kontrak-minimumm-2026-08-05"
status: "backlog"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-05T09:00:00.000Z"
completedAt: null
labels: ["epic-nol", "backend", "data-model"]
order: "aP"
---

# Add NolApplication entity (periode, kontrak minimum/maksimum, permohonan bulan mulai, OUP%/UMP%)

Add a dedicated entity for the NOL (Nota Operasi Layanan) application, which today only exists as the `PermohonanNOL` status label with no underlying data at all.

## Context
This is the highest-impact gap: `PermohonanNOL` is a real workflow stage subscriptions already pass through, but there is no form or entity backing it — sales can advance a subscription to this stage with nothing recorded. Spec fields: periode (start/end), "sama dengan A1" toggle vs. manual entry, rata-rata/kontrak minimum/kontrak maksimum, permohonan bulan mulai (calendar picker), pricing period + type + segment (duplicated from A1 but can differ for the NOL request specifically).

## Acceptance Criteria
- [ ] `NolApplication` entity added, FK to `Subscription` (1:1 or 1:N if re-applications are allowed)
- [ ] "Sama dengan A1" flag that copies from the A1-stage usage data vs. manual override entry
- [ ] EF Core migration created and applied
- [ ] DTOs added to `Shared/Models.cs`
