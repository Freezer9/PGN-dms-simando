---
id: "master-data-administrative-geography-provinceregen-2026-08-07"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T20:03:28.000Z"
completedAt: "2026-08-07T20:03:28.000Z"
labels: ["master-data", "phase-0"]
order: "a000"
---

# Master data: administrative geography (Province/Regency/District/Village)

Four-level Indonesian hierarchy with BPS/Kemendagri codes; `regency` covers both Kota and Kabupaten (`type` enum). Seed from the official Kemendagri list (Permendagri 72/2019+), not typed by hand. Blocks company creation and the `Nomor` format. See `docs/domain/master-data.md#4-administrative-geography`.
