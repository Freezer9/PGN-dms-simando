---
id: "add-pricing-selection-logic-regulersigasbersyarat-2026-08-05"
status: "backlog"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-05T09:00:00.000Z"
completedAt: null
labels: ["epic-pricing", "backend", "api"]
order: "aN"
---

# Add pricing selection logic (Reguler/SiGas/Bersyarat with justification, Kode Harga) on subscription

Let a subscription select a pricing tier/type and record the required justification for conditional pricing.

## Context
Spec has three pricing modes: Reguler, SiGas, Bersyarat (conditional — requires stating terms and reason), plus Harian/Bulanan/Tahunan selection and OUP%/UMP% fields. None of this exists today.

## Acceptance Criteria
- [ ] `PricingSelection` entity (SubscriptionId, PricingTierId, Mode [Reguler/SiGas/Bersyarat], Justification (required when Bersyarat), OupPercent, UmpPercent)
- [ ] API endpoint to set/update the pricing selection
- [ ] Validation: `Justification` required when Mode = Bersyarat
