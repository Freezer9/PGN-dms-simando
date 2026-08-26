---
id: "add-per-equipment-energy-table-steam-boilerthermal-2026-08-05"
status: "backlog"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-05T09:00:00.000Z"
completedAt: null
labels: ["epic-energy", "backend", "data-model"]
order: "aF"
---

# Add per-equipment energy table (Steam Boiler/Thermal Oil Heater/Dryer, fuel source, gas-conversion volume)

Model the detailed per-equipment energy table: capacity/unit, operating pattern (jam/hari), fuel source, monthly consumption, and gas-conversion volume — a variable-row table in the spec (insert row if more than one unit of a given equipment type).

## Context
This is the most detailed missing data structure in the spec (Steam Boiler, Thermal Oil Heater, Dryer rows shown as examples, but the table supports arbitrary equipment/rows). Zero equivalent exists today.

## Acceptance Criteria
- [ ] `EnergyEquipment` entity (EquipmentName, CapacityPerUnit, CapacityUnit, JamPerHari, HariPerMinggu, FuelSource, MonthlyConsumption, ConsumptionUnit, SubscriptionId FK)
- [ ] Supports arbitrary number of rows per subscription (not hardcoded to the 3 example equipment types)
- [ ] EF Core migration created and applied
