# Frontend 05 — Prospect & Survey (KK0)

Stages 3–4. Field definitions in
[04-prospect-survey.md](../../domain/04-prospect-survey.md).

---

## Prospek tab — contacts

Repeatable contact cards (PIC) with name and role required, social handles optional.

---

## Survei tab — the KK0 form (`SurveyForm.razor`)

The heaviest form in the system: ~60 fields plus four repeating groups. It maps to
**Lampiran 10 — *DATA SURVEY PASAR***.

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│  🔥 TOTAL KEBUTUHAN ENERGI (MMBTU/BULAN)                         1.850,00 MMBTU        │
│  Sticky Floating Live Summary Bar (top-16 z-20)                                        │
├──────────────────────┬─────────────────────────────────────────────────────────────────┤
│ ● 1 Identitas        │  4. PRODUK UTAMA                            [ + Tambah baris ]  │
│ ● 2 Produk Utama     │  ┌───┬────────────────────┬────────────┬──────────────┬─────┐  │
│ ● 3 Bahan Baku       │  │ 1 │ [ Kain sarung    ] │ [  12.000 ]│ Kaps/Tahun   │ (✕) │  │
│ ● 4 Orientasi Pasar  │  └───┴────────────────────┴────────────┴──────────────┴─────┘  │
│ ● 5 Operasional      │                                                                 │
│ ◐ 6 Kebutuhan Energi │  7. PERALATAN DAN PEMAKAIAN BAHAN BAKAR   [ + Tambah Peralatan ]│
│ ○ 7 Peralatan & Gas  │  ┌───┬──────────────┬────────────┬─────────────┬──────────────┐ │
│ ○ 8 Pipa Terdekat    │  │ 1 │ Boiler       │ Gas Bumi ▾ │ 1 Unit      │ 450,00 MMBTU │ │
│ ○ 9 Surveyor         │  └───┴──────────────┴────────────┴─────────────┴──────────────┘ │
└──────────────────────┴─────────────────────────────────────────────────────────────────┘
```

### Highlights & Implementation UX

1. **Sticky Top Live Energy Demand Summary Bar**:
   - Anchored at `top-16 z-20`, displaying real-time aggregated energy demand in **MMBTU/Bulan** as equipment rows are added or edited.
   - Replaces redundant static inline summary boxes.
2. **Anchor Scroll Offset Alignment (`scroll-mt-40` / `10rem`)**:
   - All 10 section anchor headers (`#survei-operasional`, `#survei-peralatan`, etc.) use `scroll-mt-40` (10rem / 160px).
   - Clicking section navigation sidebar links scrolls smoothly so section titles stop cleanly **below both the top navbar and floating energy bar** with zero visual overlap.
3. **Equipment Table Badges**:
   - Displays emerald energy badges (`bg-emerald-50 text-emerald-700`) for computed MMBTU values per equipment unit.
