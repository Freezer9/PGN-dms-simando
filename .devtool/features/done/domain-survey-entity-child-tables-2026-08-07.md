---
id: "domain-survey-entity-child-tables-2026-08-07"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-08T23:13:44.000Z"
completedAt: "2026-08-08T23:13:44.000Z"
labels: ["survey", "phase-3"]
order: "a00o"
---

# Domain: Survey entity + child tables

Header (`survey`) plus `survey_product`, `survey_raw_material`, `survey_market`, `survey_equipment` repeating child tables — never numbered columns. See `docs/design/data-model.md#survey--stage-4-kk0-header`.

Implemented in `src/Simando.Domain/Survey/`: `Survey.cs` (header, CompanyId PK, no surrogate id — mirrors `Directory.Plotting`), `SurveyProduct.cs`, `SurveyRawMaterial.cs`, `SurveyMarket.cs`, `SurveyEquipment.cs`, plus flags enums `KebutuhanEnergiJenis`, `BahanBakarEksisting`, `RencanaPemanfaatanGas`, and `Asal`. Unit columns (`kapasitas_unit`, `pemakaian_unit`, `satuan`, `konsumsi_unit`) are `Guid?` FKs into `UnitOfMeasure`, scoped by `UnitSet`, not enums — matches the existing master-data unit-set pattern. `beban_puncak` intentionally omitted (own backlog card). `Satuan`/`Periode` columns on product/raw-material rows omitted where the source value never varies.
