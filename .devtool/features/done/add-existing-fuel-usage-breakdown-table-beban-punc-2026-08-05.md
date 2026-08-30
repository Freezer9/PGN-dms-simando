---
id: "add-existing-fuel-usage-breakdown-table-beban-punc-2026-08-05"
status: "done"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-30T00:00:00.000Z"
completedAt: "2026-08-30T00:00:00.000Z"
labels: ["epic-kk0", "backend", "data-model"]
order: "aV"
---
# Add existing fuel usage breakdown table + beban puncak + rencana pemanfaatan gas fields to KK0

Add the KK0-specific detail tables: current fuel/equipment breakdown (Steam Boiler/Thermal Oil Heater/Melting Furnace with capacity, operating pattern, fuel type/cost/consumption/gas-conversion), peak load hours (Beban Puncak start/end), and planned gas utilization categories (Bahan Baku/Bahan Bakar/Pembangkit Listrik/CNG/Transportasi Gas/other).

## Context
This table looks superficially similar to the Energy Needs equipment table (epic-energy) but is a separate survey-stage snapshot with its own fields (e.g. current fuel cost, existing supplier name) — confirm with business whether these should actually be the same underlying entity reused across A1 and KK0, or genuinely separate records.

## Acceptance Criteria
- [x] Clarify with stakeholders whether this duplicates or extends the Energy Needs equipment table from epic-energy
- [x] `Kk0FuelUsage` child entity (or reuse) covering existing equipment/fuel breakdown
- [x] `BebanPuncakMulai`/`BebanPuncakSelesai` fields (supports 2 shift windows per spec)
- [x] `RencanaPemanfaatanGas` flags/entity (Bahan Baku/Bahan Bakar/Pembangkit Listrik/CNG/Transportasi Gas/Lainnya with free-text)

## Overlaps With
- **Add per-equipment energy table** (epic-energy) — resolve during implementation whether KK0's fuel usage table reuses that entity or is genuinely a separate survey-stage snapshot; don't build both independently without checking.
