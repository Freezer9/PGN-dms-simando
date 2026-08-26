---
id: "web-companiesidsurvey-kk0-form-2026-08-07"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-09T00:34:56.000Z"
completedAt: "2026-08-09T00:34:56.000Z"
labels: ["survey", "phase-3"]
order: "a00r"
---

# Web: /companies/{id}/survey — KK0 form

17 sections capturing the union of the client's KK0 sheet and the official Lampiran 10, with repeating groups (Produk Utama, Bahan Baku, Orientasi Pasar, equipment table with `Insert Row jika > 1`). Dense layout — no wizard; this is the heaviest form in the system (~60 fields).

Implemented as `src/Simando.Web/Components/Pages/SurveyForm.razor`, embedded into `CompanyHub.razor`'s existing `survei` tab (child route already wired via `StageTabValues`). Scope was narrowed with the user before building, since none of the design doc's assumed shared components (`MeasureInput`, `RepeatingRows`, `StageStepper`, `AttachmentUploader`) exist anywhere in the codebase and no autosave/debounce infra exists either:

- **No autosave** — manual "Simpan" button, same `StageEditResult` pattern as every other form.
- **Inline editable table rows** for the 4 repeating groups (via `BbTable` primitives), not the dialog-per-row pattern Contacts uses — matches the doc's own mockup.
- **No `MeasureInput`** — plain paired `BbFormFieldInput` + `BbFormFieldSelect` for unit fields.
- **No scrollspy nav** — a static anchor-link list; `BbFormWizard` was ruled out since it's step-locked and the doc explicitly says "not a wizard."

Also required a new `IUnitLookupService` (`application-unit-lookup-service` card) since `UnitOfMeasure` doesn't carry its own `UnitSet`.
