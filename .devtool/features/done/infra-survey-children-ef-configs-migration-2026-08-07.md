---
id: "infra-survey-children-ef-configs-migration-2026-08-07"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-08T23:24:59.000Z"
completedAt: "2026-08-08T23:24:59.000Z"
labels: ["survey", "phase-3", "infra"]
order: "a00q"
---

# Infra: Survey + children EF configs + migration

EntityTypeConfiguration for the header and four child tables, plus the migration.

5 configs added (`SurveyConfiguration`, `SurveyProductConfiguration`, `SurveyRawMaterialConfiguration`, `SurveyMarketConfiguration`, `SurveyEquipmentConfiguration`), same shape as `PlottingConfiguration`/`CompanyContactConfiguration` — cascade FK to Company, Restrict on unit/fuel/country/user FKs, enums via `.HasConversion<string>()`, decimals via `.HasPrecision()` matching `MeterSizeConfiguration`. DbSets + RLS query filters (company-visibility only) wired into `SimandoDbContext`. Migration `AddSurveyAndChildren` generated and verified against a full solution build.
