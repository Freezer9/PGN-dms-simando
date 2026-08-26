---
id: "web-record-hub-plotting-contacts-tabs-2026-08-07"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-09T00:00:00.000Z"
completedAt: "2026-08-09T00:00:00.000Z"
labels: ["record-hub", "phase-1"]
order: "a00h"
---

# Web: Record hub Plotting & Contacts tabs

The first two of the hub's nine tabs — stage 2 and stage 3 data, editable by Sales Area/Regional Admin, read-only for others.

`Plotting` tab: `Plotting By` (sales dropdown), `Posisi Pelanggan`, `Kawasan` — upsert, first save bumps `Company.CurrentStage` 1→2. A `[ Prospek ]` button (visible once stage 2, gate trivially satisfied since all three Plotting fields are DB-required) bumps 2→3 — `docs/end-to-end-walkthrough.md` and `docs/design/frontend/03-directory-plotting-map.md` both place this action on the hub's Plotting page, not only on the (separate, still-backlog) `/plotting` list. `Prospek` tab: full `CompanyContact` CRUD — Nama/Jabatan required, one contact always primary, deleting the last remaining contact blocked (`docs/design/frontend/05-prospect-and-survey.md`).

**Architecture fix bundled in**: `CompanyHub.razor` was a single `@page "/companies/{CompanyId:guid}"` route switching tabs via in-memory state, which doesn't match the canonical route design (`docs/design/frontend/01-shell-and-navigation.md`: *"Stage tabs are child routes... deep-linkable, back-button friendly"* — `/companies/{id}/plotting`, `/companies/{id}/prospect` etc. are real routes). Added `@page "/companies/{CompanyId:guid}/{Tab?}"`, drove `_activeTab` from the route via `OnParametersSet`, and switched both the stepper and the tab strip to `Nav.NavigateTo`. This fixes the routing mechanism for all 9 tabs, not just the 2 built here — the remaining 6 stay `BbEmpty` placeholders for their own future cards.

**Deviations**: `Posisi Pelanggan`/`Kawasan` render as `BbFormFieldRadioGroup` (matches the mockup) rather than free-text; contact add/edit uses a dialog rather than the mockup's always-inline-editable cards — simpler, consistent with every other CRUD screen already in this app (Directory's row-menu-plus-dialog pattern), still meets the CRUD/validation requirements.

New tests: `tests/Simando.Integration.Tests/Directory/CompanyPlottingContactTests.cs` (7 tests) — Plotting upsert + stage bump, Prospek promote gate, Draft-only write rejection, contact primary-flip, last-contact-delete block, contact edit.
