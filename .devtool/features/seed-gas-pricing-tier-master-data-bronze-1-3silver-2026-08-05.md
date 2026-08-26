---
id: "seed-gas-pricing-tier-master-data-bronze-1-3silver-2026-08-05"
status: "backlog"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-05T09:00:00.000Z"
completedAt: null
labels: ["epic-pricing", "backend", "seed-data"]
order: "aM"
---

# Seed gas pricing tier master data (Bronze 1-3/Silver/Gold/Platinum, B1-B3 segment codes, Harian/Bulanan/Tahunan USD rates)

Add the gas pricing tier reference table with the tier names, segment codes, and per-period USD rates shown in the spec.

## Context
Spec shows a fixed pricing table: Bronze 1/2/3 map to segment codes B1/B2/B3 at ~$10,000/$9.66/$9.5, and Silver/Gold/Platinum at ~$9.26/$9.18/$8.78, each with Harian/Bulanan/Tahunan variants. None of this exists — zero pricing data anywhere in the codebase.

## Acceptance Criteria
- [ ] `PricingTier` entity/table (TierName, SegmentCode, Period [Harian/Bulanan/Tahunan], RateUsd)
- [ ] Seeded with the rates shown in the spec (confirm current rates with business before going live — spec values may be stale examples)
- [ ] EF Core migration + seeder
