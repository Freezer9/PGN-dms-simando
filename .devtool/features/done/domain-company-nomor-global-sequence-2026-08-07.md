---
id: "domain-company-nomor-global-sequence-2026-08-07"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T20:34:19.000Z"
completedAt: "2026-08-07T20:34:19.000Z"
labels: ["company", "phase-1"]
order: "a00X"
---

# Domain: Company Nomor global sequence

`{seq:0000000}-{bps_prov}-{bps_kab}` rendered from a plain PostgreSQL `SEQUENCE` (atomic, lock-free, non-transactional gaps accepted). Re-renders the suffix while `DRAFT`; frozen once the record leaves `DRAFT` because it appears on signed documents. See `docs/domain/03-directory-plotting.md#the-nomor-format`.
