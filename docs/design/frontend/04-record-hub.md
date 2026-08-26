# Frontend 04 — Record Hub

**The most important page in the application (`CompanyHub.razor`).** One record, all eight stages, and — critically — the answer to *"statusnya dah sampai mana?"* without asking anyone.

---

## Layout & Header Enhancements

```
┌────────────────────────────────────────────────────────────────────────────────────────────┐
│  Beranda › Direktori › PT Indonesia 1945                                                   │
├────────────────────────────────────────────────────────────────────────────────────────────┤
│  ⚡ TINDAKAN DIPERLUKAN                                                                     │
│  Berkas ini memerlukan tindakan persetujuan Anda sebagai Reviewer 2.                       │
│                                           ( Tolak ) ( Minta Revisi ) [ Setuju ]            │
├────────────────────────────────────────────────────────────────────────────────────────────┤
│  PT INDONESIA 1945                                       ┌───────────────────────────────┐ │
│  0000042-35-78 · Jasa Laundry · Kota Surabaya            │  MENUNGGU                     │ │
│  Sales: Budi S.  ·  Area Surabaya · SOR II               │  Reviewer 2 — Dewi K.         │ │
│                                                          │  ⏱ 11 hari                    │ │
│                                                          └───────────────────────────────┘ │
├────────────────────────────────────────────────────────────────────────────────────────────┤
│   ①───────②───────③───────④───────⑤───────⑥───────⑦───────⑧                              │
│  Direkt  Plott   Prosp   Survei    A1    Permoh  Evaluasi Persetj                          │
│   ✓       ✓       ✓       ✓        ✓       ✓       ✓      ◐ berjalan                       │
├──────────────────────────────────────────────────┬─────────────────────────────────────────┤
│ [Ringkasan] Plotting  Prospek  Survei  A1  NOL   │  LINI MASA                              │
│  Evaluasi  Penerbitan  Dokumen                   │                                         │
├──────────────────────────────────────────────────┤  ◐ 21/08 08:00  Reviewer 2 · Dewi K.    │
│  4 EXECUTIVE SUMMARY GRID CARDS                  │    ⏱ menunggu 11 hari                   │
│  ┌────────────┬────────────┬────────────┬──────┐ │                                         │
│  │Est. Gas Vol│Segmen      │Harga Gas   │Status│ │  ● 20/08 16:40  Reviewer 1 · Andi P.     │
│  │96.667 MMBTU│Gold        │USD 9,18    │Aktif │ │    Setuju — "Data teknis lengkap."       │
│  └────────────┴────────────┴────────────┴──────┘ │                                         │
└──────────────────────────────────────────────────┴─────────────────────────────────────────┘
```

### Highlights
1. **Sticky Top Action Banner (`Tindakan Diperlukan`)**: Appears when `CanAct` or `CanSubmit` is true, providing immediate access to approve, revise, or reject without scrolling to the bottom.
2. **Executive Summary Grid Cards**: Displays 4 high-readability KPI cards on the `Ringkasan` tab (Gas Volume, Segment, Gas Price, Status).
3. **Enhanced Visual Stepper (`StageStepper`)**: Uses emerald checkmark badges (`bg-emerald-600`) for completed stages and a pulsing active ring for the current stage.
