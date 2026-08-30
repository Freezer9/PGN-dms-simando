---
id: "add-company-operational-fields-jumlah-karyawan-shi-2026-08-05"
status: "done"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-30T00:00:00.000Z"
completedAt: "2026-08-30T00:00:00.000Z"
labels: ["epic-company-ops", "backend", "data-model"]
order: "aC"
---
# Add company operational fields (Jumlah Karyawan, Shift, Jam Kerja/Hari per Minggu)

Add headcount and working-hours fields to the subscription entity.

## Context
Form A1 captures Jumlah Karyawan (headcount), Shift count, Jam Kerja per hari, and Hari per Minggu. None of these exist in `Entities.cs` today.

## Acceptance Criteria
- [x] `JumlahKaryawan`, `JumlahShift`, `JamKerjaPerHari`, `HariKerjaPerMinggu` fields added to `Subscription` (or a `CompanyProfile` child entity, if that pattern is adopted for other cards in this epic set)
- [x] EF Core migration created and applied
