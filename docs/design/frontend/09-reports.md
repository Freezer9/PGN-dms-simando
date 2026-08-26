# Frontend 09 — Reports

Content and metrics in
[reporting.md](../reporting.md). This document covers the
screens.

Every report passes through the same scope filter as every other query
([roles-permissions §6](../roles-permissions.md#6-enforcement-checklist)). A report is the
easiest place to leak another region's pipeline.

---

## Reports hub

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│  Laporan                                                                             │
├──────────────────────────────────────────────────────────────────────────────────────┤
│  ┌────────────────────────┐  ┌────────────────────────┐  ┌────────────────────────┐ │
│  │ 📊 Corong Penjualan    │  │ ⏱ Penuaan              │  │ 🔥 Potensi Kebutuhan   │ │
│  │ Konversi antar tahap   │  │ Yang tertahan & pada   │  │ Σ kebutuhan gas per    │ │
│  │                        │  │ siapa                  │  │ tahap                  │ │
│  └────────────────────────┘  └────────────────────────┘  └────────────────────────┘ │
│  ┌────────────────────────┐  ┌────────────────────────┐                            │
│  │ 📋 Produktivitas Survei│  │ ✅ Hasil NOL / RL       │                            │
│  │ KK0 selesai per sales  │  │ Tingkat persetujuan &  │                            │
│  │ per bulan              │  │ alasan penolakan       │                            │
│  └────────────────────────┘  └────────────────────────┘                            │
└──────────────────────────────────────────────────────────────────────────────────────┘
```

Five reports, each answering a question someone actually asks
([reporting](../reporting.md#suggested-standard-reports)). No allocation report —
PGN tracks realisasi/alokasi themselves; this platform isn't a capacity-planning
tool.

---

## Shared report frame

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│  Laporan › Penuaan                                                                   │
├──────────────────────────────────────────────────────────────────────────────────────┤
│  Periode [ Agustus 2026 ▾ ]  Area [ Semua ▾ ]  Sales [ Semua ▾ ]  Tahap [ Semua ▾ ]  │
│                                              ( Reset )  [ Terapkan ]  [ ⬇ Excel ]    │
├──────────────────────────────────────────────────────────────────────────────────────┤
│                              report body                                             │
└──────────────────────────────────────────────────────────────────────────────────────┘
```

- Filter set is consistent across reports; irrelevant filters are hidden, not
  disabled.
- **Scope narrows the filters themselves** — an Area Head sees only their Area in
  the Area dropdown, with no "Semua" that spans the region.
- Filter state lives in the query string so a report view can be pasted into a
  message.
- `⬇ Excel` on every report — this organisation runs on spreadsheets and will want
  the data back out.

---

## Corong Penjualan

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│  Direktori        ████████████████████████████████████████████  1.240                │
│  Plotting         ████████████████████████░░░░░░░░░░░░░░░░░░░░    612   49,4 %        │
│  Prospek          ████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░    318   52,0 %        │
│  Survei           ██████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░    147   46,2 %        │
│  A1               ███░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░     63   42,9 %        │
│  Permohonan NOL   █░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░     28   44,4 %        │
│  Evaluasi         ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░     11   39,3 %        │
│  NOL Terbit       ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░      7   63,6 %        │
├──────────────────────────────────────────────────────────────────────────────────────┤
│  Rincian per Area                                                    [ ⬇ Excel ]     │
│  ┌──────────┬────────┬────────┬───────┬──────┬────┬────────┬────────┬──────┐        │
│  │ Area     │ Direkt │ Plott  │Prospek│Survei│ A1 │Permoh. │Evaluasi│ NOL  │        │
│  │ Surabaya │   540  │   280  │  145  │  68  │ 30 │   14   │    6   │   4  │        │
│  │ Sidoarjo │   410  │   201  │  108  │  49  │ 21 │    9   │    3   │   2  │        │
│  │ Gresik   │   290  │   131  │   65  │  30  │ 12 │    5   │    2   │   1  │        │
│  └──────────┴────────┴────────┴───────┴──────┴────┴────────┴────────┴──────┘        │
└──────────────────────────────────────────────────────────────────────────────────────┘
```

The percentage is **conversion from the previous stage**, not from the top —
that's the number that identifies where the pipeline actually leaks. Clicking any
bar or cell opens the Directory filtered to that stage and area.

---

## Penuaan — the key report

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
├────┬────────────────────┬────────────┬─────────────────────┬────────┬──────────────┤
│    │ Perusahaan         │ Tahap      │ Menunggu di         │Menunggu│              │
├────┼────────────────────┼────────────┼─────────────────────┼────────┼──────────────┤
│    │ PT Indah Kejora    │ Persetujuan│ Reviewer 2 · Dewi K.│ 11 hari│    ( › )     │
│    │ PT Larantuka       │ Ditolak    │ Admin Reg. · Sari W.│  6 hari│    ( › )     │
│    │ PT Big Note        │ Evaluasi   │ Admin Reg. · Sari W.│  4 hari│    ( › )     │
│    │ PT Kota Baru       │ Area Head  │ Area Head · Rudi H. │  1 hari│    ( › )     │
└────┴────────────────────┴────────────┴─────────────────────┴────────┴──────────────┘
```

Sorted by wait time descending — plain elapsed time, no per-step threshold or
colour-coded breach status.

This single screen is the deliverable that closes the gap in the client's problem
statement — *"tidak bisa memonitor workflownya statusnya dah sampai mana"*. It
should be the default landing report and reachable in one click from every
dashboard.

---

## Hasil NOL / RL

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│  Terbit periode ini    NOL 7    RL 2     Tingkat persetujuan 77,8 %                  │
│                                                                                      │
│  ALASAN PENOLAKAN / REVISI (12 kejadian)                                             │
│  ████████████  Data teknis tidak lengkap          5                                  │
│  ██████        Kelayakan finansial di bawah ambang 3                                 │
│  ████          Dokumen pendukung kurang            2                                 │
│  ██            Lainnya                             2                                 │
└──────────────────────────────────────────────────────────────────────────────────────┘
```

Reasons are free text (mandatory on `Revisi`/`Tolak`), so grouping needs either an
optional category dropdown alongside the comment, or manual tagging. Adding a
`reason_category` to the action dialog is cheap and makes this report possible —
worth proposing to PGN, but not inventing without their categories.

---

## Export

- `⬇ Excel` produces the **filtered** data, matching what is on screen.
- **PII is excluded by default.** Contact names, mobile numbers and social handles
  only appear for Regional Admin and Division Head, and every such export writes
  an audit row ([roles-permissions §3](../roles-permissions.md#reporting--administration)).
  UU 27/2022 applies.
- Exports carry a header row recording the filters, the scope and the generating
  user, so a spreadsheet circulating by email remains interpretable.
