---
id: "web-plotting-list-filters-prospect-promote-2026-08-07"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-09T00:00:00.000Z"
completedAt: "2026-08-09T00:00:00.000Z"
labels: ["directory-plotting", "phase-1"]
order: "a00e"
---

# Web: /plotting list + filters + Prospect promote

Two extra filters vs Directory: Posisi Pelanggan, Kawasan. `Prospect` action promotes stage 2→3 (worksheet calls it `Potensi`; UI uses `Prospect` per the stage name).

Pure UI on top of `web-record-hub-plotting-contacts-tabs-2026-08-07`'s already-built, already-tested `SavePlottingAsync`/`PromoteToProspekAsync` — no new service write logic. `Plotting.razor`: same cascading-geography-filter shape as `Directory.razor`, plus Posisi Pelanggan/Kawasan filters. `ICompanyService.GetListAsync`/`CompanyListItem` extended (not a new query) with nullable `SalesUserId`/`SalesUserName`/`PosisiPelanggan`/`Kawasan`, since Directory and Plotting are views of the same record (`docs/domain/03-directory-plotting.md`).

Row action is state-driven per the mockup (`docs/design/frontend/03-directory-plotting-map.md`): `( Lengkapi )` when no Plotting row yet, `[ Prospek ]` (direct promote, confirm dialog) once stage 2 and complete, `( Ubah )` once past stage 2 — all three link into or act on the already-built `/companies/{id}/plotting` hub tab. Non-editors get a `Lihat` link instead.

Extracted `CompanyLabels.cs` (`TruncatedNomor`, `PosisiPelangganLabel`, `KawasanLabel`) once this page needed the same label logic `Directory.razor`/`CompanyHub.razor` already had — both switched to the shared version. Also fixed `Directory.razor`'s `Plotting` row-menu link, which pointed at the bare hub (`/companies/{id}`) back when the Plotting tab was still a placeholder — now points at `/companies/{id}/plotting`.

**Deviations**: no inline editing of the three Plotting fields directly in the list row — the mockup calls it *"worth supporting"*, not mandatory, and `Directory.razor` has no inline-edit precedent either; `( Lengkapi )`/`( Ubah )` reuse the already-tested hub form instead. No map mode (`web-map-full-screen-map`, a00f, untouched). No Excel export (not shown on this screen's own mockup).

This closes out phase-1 `directory-plotting` except `web-map-full-screen-map` (a00f), a materially larger, separate piece of work.

New tests: 2 more cases in `tests/Simando.Integration.Tests/Directory/CompanyServiceTests.cs` (`GetListAsync` Plotting-field population, Posisi Pelanggan/Kawasan filtering).
