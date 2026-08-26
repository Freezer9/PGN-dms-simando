---
id: "application-survey-icompanyservice-extension-2026-08-08"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-08T23:44:50.000Z"
modified: "2026-08-08T23:44:50.000Z"
completedAt: "2026-08-08T23:44:50.000Z"
labels: ["survey", "phase-3", "application"]
order: "a01a"
---

# Application: Survey ICompanyService extension

New scope discovered mid-implementation of the Survey/KK0 stage-4 form work
— not covered by `domain-survey-entity-child-tables` or
`infra-survey-children-ef-configs-migration` (both already done), and a
prerequisite for `web-companiesidsurvey-kk0-form`.

Extended `ICompanyService` (not a new `ISurveyService` — matches the
Plotting/Contact precedent) with 15 methods: `GetSurveyAsync` (one
aggregate `SurveyDetail` — header + all four child lists, since the KK0
form is a single scrolling page, not tabs), `SaveSurveyAsync` (header
upsert, bumps `CurrentStage` 3→4 on first save), and Add/Update/Delete ×4
child tables (Product/RawMaterial/Market capped at 4 rows per the source
doc, Equipment uncapped). Gated on `Capability.EditSurvey` via the existing
`CanEditAsync` helper.

Along the way, corrected `Survey.PipaTerdekatJarakM` from `required decimal`
to `decimal?` — the KK0 form is autosave/non-wizard, so a NOT NULL column
would break an early autosave. Migration regenerated in place (no data
existed against it). See commit history for the fix.

Also implements `domain-equipment-table-derived-sum`: every equipment
Add/Update/Delete recomputes `Survey.JumlahKebutuhanEnergi` as a live sum,
persisted (not computed on read) since it appears on signed documents.

Out of scope, left for their own cards: signed KK0 upload + A1 gate
(`web-signed-kk0-upload-flow`), Beban Puncak
(`web-beban-puncak-peak-load-capture`), KK0 `.docx` generation
(`documents-lampiran-10-kk0-docx-template-merge`), and the Razor form
itself (`web-companiesidsurvey-kk0-form`).
