# Frontend 08 — Tasks & Approvals

The workflow surface (`Tasks.razor`). Mechanics in
[approval-workflow.md](../approval-workflow.md).

---

## Tugas Saya — the inbox (`Tasks.razor`)

For Area Head, Regional Admin, Reviewer, and Division Head.

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│  Tugas Saya                                                                            │
├────────────────────────────────────────────────────────────────────────────────────────┤
│  [ ⚡ Perlu Tindakan Saya (7) ]  ( ⌛ Dalam Proses di Region )  ( ✅ Riwayat Persetujuan )│
│                                                                                        │
│  Tahap ▾ Semua (Combobox)  Area ▾ Semua (Combobox)  Urutkan ▾ Terlama (Select)         │
├────┬──────────────────────┬────────────┬───────────┬─────────────┬────────┬───────────┤
│    │ Perusahaan           │ Tahap      │ Area      │ Diajukan    │Durasi  │ Aksi      │
├────┼──────────────────────┼────────────┼───────────┼─────────────┼────────┼───────────┤
│    │ PT Indah Kejora      │ Evaluasi   │ Surabaya  │ Budi S.     │🔴 11d  │[ Tinjau ] │
│    │ 0000038-35-78 · Kimia│            │           │ 10/08/2026  │        │           │
│    │ PT Big Note          │ Evaluasi   │ Surabaya  │ Budi S.     │🟡  4d  │[ Tinjau ] │
│    │ PT Kota Baru         │ Persetujuan│ Sidoarjo  │ Rina A.     │🟢  1d  │[ Tinjau ] │
└────┴──────────────────────┴────────────┴───────────┴─────────────┴────────┴───────────┘
```

### Highlights & Rules

1. **Segmented Filter Tabs**: Quick navigation between `Perlu Tindakan Saya`, `Dalam Proses di Region`, and `Riwayat Persetujuan`.
2. **Color-Coded SLA Clock Badges**:
   - 🟢 **Normal (< 3 days)**: `bg-emerald-50 text-emerald-700`
   - 🟡 **Warning (3–7 days)**: `bg-amber-50 text-amber-700`
   - 🔴 **Urgent / SLA Exceeded (> 7 days)**: `bg-rose-50 text-rose-700 animate-pulse`
3. **Sorted by Wait Time Descending by Default**: Prioritizes cases waiting longest.
4. **Dropdown Filter Components**:
   - `Tahap` & `Area`: `BbCombobox`
   - `Urutkan` (SortMode): `BbSelect` (2 static options: `Terlama menunggu`, `Nama Perusahaan (A-Z)`).
