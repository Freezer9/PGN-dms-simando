---
id: "web-directorynew-create-company-form-2026-08-07"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-09T00:00:00.000Z"
completedAt: "2026-08-09T00:00:00.000Z"
labels: ["directory-plotting", "phase-1"]
order: "a00d"
---

# Web: /directory/new create-company form

Nama, Website, cascading geography, Jenis Produksi, manual pin-drop on a map (available from this stage, not geocoded from the address).

Built together with `web-directory-list-filters-map-mode-2026-08-07` — see that card for the combined notes and deviations. `PinDropMap.razor` + `wwwroot/js/pin-map.js` are the app's first JS interop: a single draggable Leaflet marker (CDN-loaded Leaflet/OSM tiles), click-to-place, drag-to-move, bound back into the form's `Latitude`/`Longitude` via `@bind-`. Manually verified in a real browser (Playwright against the running dev stack) since a green build/test run doesn't prove JS interop actually renders — map loads real OSM tiles, click drops a pin, coordinates update live, no console errors.

**Regional Admin gets an Area field the wireframe doesn't show** — `Company.AreaId` is required and Regional Admin (`AccessScope.Region`) has no single Area, but `RoleCapabilities.cs` grants them `Capability.CreateCompany` too. Sales Area's own Area is hidden/pre-filled; Regional Admin gets a required select scoped to their Region's Areas.

**Deviations**: no auto-centring the map on the selected Kelurahan/Desa (no village in this domain model carries coordinates — nothing to centre on); no `Cari dari alamat` geocode or `Gunakan lokasi saya` geolocation (both additive conveniences per the doc, not the mandatory path).

New tests: `tests/Simando.Integration.Tests/Directory/CompanyServiceTests.cs` (5 tests).
