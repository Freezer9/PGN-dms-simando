---
id: "master-data-wilayah-seeder-2026-08-08"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-08T00:00:00.000Z"
modified: "2026-08-09T00:00:00.000Z"
completedAt: "2026-08-09T00:00:00.000Z"
labels: ["master-data", "infra"]
order: "a01q"
---

# Master data: Wilayah seeder (Province/Regency/District/Village)

`province`/`regency`/`district`/`village` tables exist (`master-data-administrative-geography-provinceregen`, done) but are empty — no seeder was actually built alongside the entities, and there's deliberately no admin UI to type ~84,000 rows by hand (see `docs/domain/master-data.md#4-administrative-geography` — "No admin UI for this table"). Company creation and the `Nomor` format both depend on this data existing.

Seed from the official Kemendagri list (Permendagri 72/2019 and successors), not the worksheet's sample values — those are dummy data (`docs/domain/master-data.md#4-administrative-geography` notes the mock's `Kendal` under `Jawa Timur` is wrong; Kendal is in Jawa Tengah). Likely shape: a one-time seed command in the same family as `AdminSeeder`/`seed-admin` (`Simando.Infrastructure/Identity/AdminSeeder.cs`, `Simando.Web/Cli/SeedAdminCommand.cs`), reading a bundled Kemendagri dataset rather than a hardcoded list — needs sourcing the actual dataset file (CSV/JSON) as a prerequisite.

## Shipped

Sourced from [cahyadsn/wilayah](https://github.com/cahyadsn/wilayah)'s `db/wilayah.sql`
(MIT licensed, current as of Kepmendagri No 300.2.2-2138/2025) — the same dataset
the wilayah.id API is itself built from. Started as a live wilayah.id API pull, but
a burst of concurrent requests got the CDN to throttle/block this environment
mid-fetch; the GitHub dataset is one clean download instead of ~7,900 API calls,
so switched to it entirely.

Parsed into four `code|name` files (one per level, code = dotted Kemendagri code,
parent = same code minus its last segment) at
`src/Simando.Infrastructure/Persistence/SeedData/Geography/`, embedded as resources
so the import works regardless of working directory. `GeographySeeder` streams them
line-by-line and batches inserts — never loads a level whole. `seed-master-data` CLI
command runs it (idempotent — skips if `province` already has rows). Verified: 38
provinces, 514 regencies, 7,285 districts, 83,762 villages, zero orphaned parent
references, every district has at least one village.

`RegencyType`/`VillageType` derived from the source data itself: regency name
prefix ("Kota "/"Kabupaten ") and village code's local-segment first digit
('1'=Kelurahan, '2'=Desa, a rare '3' for 14 Papua "Desa Adat" rows falls back to
Desa) — no separate type column needed from the source.
