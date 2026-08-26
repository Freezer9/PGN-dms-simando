---
id: "web-directory-list-filters-map-mode-2026-08-07"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-09T00:00:00.000Z"
completedAt: "2026-08-09T00:00:00.000Z"
labels: ["directory-plotting", "phase-1"]
order: "a00c"
---

# Web: /directory list + filters + map mode

Cascading region filters (Prov→Kota/Kab→Kec→Kel/Desa) + Jenis Produksi, every filter with `Semua`. Same dataset renderable as a table or as pins. Row actions: Plotting / Edit / Delete (soft-delete, blocked once ever submitted).

Built together with `web-directorynew-create-company-form-2026-08-07` — a Draft company created via the form would otherwise be unreachable except via the URL the creator lands on right after saving, so the two shipped as one task. `Directory.razor`: cascading Prov→Kota/Kab→Kec→Kel/Desa + Jenis Produksi + Tahap filters, `ICompanyService.GetListAsync` leans entirely on `Company`'s own row-level-security query filter (`SimandoDbContext.OnModelCreating`) for scope + soft-delete rather than reimplementing it. Row menu: `Buka` (record hub), `Plotting` (record hub's Plotting tab, still a placeholder), `Hapus` (soft-delete, Draft-only, `Capability.SoftDeleteCompany`).

**Deviations**:
- **No `🗺 Peta` map-mode toggle.** That's the same multi-pin, clustered, stage-coloured/sized/bordered map as `/map` (`web-map-full-screen-map-2026-08-07`, a00f) — a materially bigger component than this card's own scope. Building it here would be building a00f under a different card's name. a00f stays in backlog, untouched.
- **No `Ubah` (Edit) row action**, despite being named in this card's own text. Editing needs a pre-populated form, `Capability.EditStages1To3`, and the doc's Nomor-re-render-on-address-change behaviour — real additional scope beyond create+list+delete that wasn't part of the plan presented for this task. Noted here rather than silently expanded into.

New tests: `tests/Simando.Integration.Tests/Directory/CompanyServiceTests.cs` (shared with a00d, 5 tests).
