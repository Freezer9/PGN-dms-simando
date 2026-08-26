---
id: "web-map-full-screen-map-2026-08-07"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-09T00:00:00.000Z"
completedAt: "2026-08-09T00:00:00.000Z"
labels: ["directory-plotting", "phase-1"]
order: "a00f"
---

# Web: /map full-screen map

Leaflet + OpenStreetMap via Blazor JS interop. Stage-based pin layers (Directory/Plotting/Surveyed/A1/NOL issued), pin colour = stage, size = Jumlah Kebutuhan Energi, badge = Posisi Pelanggan. No pipeline-network layer — PGN cannot supply pipe geometry. Scoped by area/region; SA and RA can drop/move pins, everyone else views only.

Closes phase-1 `directory-plotting` entirely. `MapPage.razor` (`/map`) reuses `GetListAsync`/`CompanyListItem` (extended with `Latitude`/`Longitude`) rather than a parallel query — same "Directory and Plotting are views of the same record" reasoning as a00e. 7 stage-layer checkboxes (Direktori/Plotting/Prospek/Survei/A1/Permohonan NOL/NOL Terbit) toggle client-side via Leaflet layer groups, no server round-trip. Scoping comes for free from `Company`'s existing row-level-security query filter. Drag/move gated by `Capability.DropMovePin` (SA/RA) + `Status == Draft`, via new `ICompanyService.UpdateLocationAsync`.

`wwwroot/js/full-map.js` (new, separate from `pin-map.js`'s single-picker form component, as that module's own comment already anticipated): pins render as `L.circleMarker` (not the default icon) specifically so size-encoding is a real radius formula, not a stub — every pin feeds `size: 1.0` today, and the doc comment in `MapPage.razor`'s pin-mapping code says exactly what changes once `Survey`/`domain-survey-entity-child-tables` (a00o) ships (a data join + swapping the constant — no JS changes). `circleMarker` has no native Leaflet drag support, so dragging is hand-rolled via map `mousemove`/`mouseup` rather than pulling in a third CDN plugin. Popup HTML is escaped (`NamaPerusahaan` is free-text user input).

**Deviations**:
- **Pin size and the popup's "Kebutuhan gas" line are uniform/omitted** — blocked on `Survey`/a00o not existing (`Jumlah Kebutuhan Energi` lives there). Not a silent default: the size-rendering mechanism itself is real, fed a placeholder constant, so future wiring is a data join + one constant swap.
- **`[ ⊞ Tabel ]` links to `/directory`** (full navigation) rather than a shared filter-state toggle across `/directory`/`/plotting`/`/map` — that bigger unification was already out-of-scope on both of those pages' own done-cards.
- No manual browser check yet (per standing preference — user checks himself). Worth a look: clustered pins load, layer checkboxes toggle, popup's "Buka Record" link works, and (as SA/RA) a dragged pin persists after reload.

New tests: 2 more cases in `tests/Simando.Integration.Tests/Directory/CompanyServiceTests.cs` (`GetListAsync` Latitude/Longitude, `UpdateLocationAsync` persist + Draft-only rejection).
