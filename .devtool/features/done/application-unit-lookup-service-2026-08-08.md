---
id: "application-unit-lookup-service-2026-08-08"
status: "done"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-09T00:21:04.000Z"
modified: "2026-08-09T00:21:04.000Z"
completedAt: "2026-08-09T00:21:04.000Z"
labels: ["survey", "phase-3", "application", "master-data"]
order: "a01b"
---

# Application: unit lookup service (UnitSet-scoped)

New scope discovered while planning the Survey/KK0 form
(`web-companiesidsurvey-kk0-form`) — its unit-bearing fields (kapasitas_unit,
pemakaian_unit, satuan, konsumsi_unit) need a dropdown scoped by `UnitSet`
(Capacity, EnergyUsage, RawMaterial, FuelConsumption, …), but `UnitOfMeasure`
doesn't carry its own `UnitSet` — that only lives on the `UnitSetMember`
join table, which the generic `IEntityService<T>` (single-entity, no joins;
`UnitSetMember` isn't even an `AuditableEntity`) can't express.

Added `IUnitLookupService.GetUnitsAsync(UnitSet set)` →
`IReadOnlyList<UnitOption>` (`Simando.Application/MasterData/`), implemented
in `Simando.Infrastructure/MasterData/UnitLookupService.cs` as a join over
`UnitSetMembers`/`UnitsOfMeasure` ordered by `SortOrder`, registered in
`DependencyInjection.cs` alongside the other Application services.
`CountryId`/`FuelTypeId` dropdowns elsewhere on the Survey form don't need
this — both are `AuditableEntity`, already covered by
`IEntityService<Country>`/`IEntityService<FuelType>`.
