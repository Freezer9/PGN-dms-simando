# Frontend 03 — Directory, Plotting & Map

Stages 1–2 & Spatial Map View. Field definitions in
[03-directory-plotting.md](../../domain/03-directory-plotting.md).

---

## Component Selection Standard for Filters & Forms

To ensure predictable UX and prevent over-engineering:
- **`BbSelect` (Native Select)**: Used for short, static option sets (< 10 items, no search required) such as `Tahap`, `Posisi Pelanggan`, `Kawasan`, `SortMode`, and `UnitDimension`.
- **`BbCombobox` (Searchable Select)**: Used for high-cardinality options requiring search filtering (e.g. `Provinsi`, `Kota/Kabupaten`, `Kecamatan`, `Kelurahan/Desa`, `Jenis Produksi`, `Pengguna Tujuan`). Button trigger labels enforce CSS text truncation (`white-space: nowrap; overflow: hidden; text-overflow: ellipsis;`).

---

## Directory list (`Directory.razor`)

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│  Direktori Industri                            [ ⊞ Tabel ] [ 🗺 Peta ]   [ + Tambah ] │
├──────────────────────────────────────────────────────────────────────────────────────┤
│  Provinsi ▾   Kota/Kab ▾   Kecamatan ▾   Kel/Desa ▾   Jenis Produksi ▾   Tahap ▾     │
│  Jawa Timur   Surabaya     Semua         Semua        Semua             Semua        │
│                                                            ( Reset )  [ Terapkan ]   │
├──────────────────────────────────────────────────────────────────────────────────────┤
│  124 perusahaan                              Urutkan: Terbaru ▾      [ ⬇ Ekspor ]    │
├──────┬────────────────────┬──────────────┬────────────────┬─────────┬───────────────┤
│ No.  │ Nama               │ Produksi     │ Lokasi         │ Tahap   │ Aksi          │
├──────┼────────────────────┼──────────────┼────────────────┼─────────┼───────────────┤
│ …001 │ PT Big Note        │ Kertas       │ Surabaya       │ ④ Survei│ ( … )         │
│ -35-78│                   │              │ Genteng        │         │               │
└──────┴────────────────────┴──────────────┴────────────────┴─────────┴───────────────┘
```

### Filters

Cascading: Provinsi → Kota/Kabupaten → Kecamatan → Kelurahan/Desa. Each carries a **`Semua`** option.

---

## Spatial Map View (`MapPage.razor` & `full-map.js`)

The `/map` spatial view provides an interactive decision support tool:

```
┌────────────────────────────────────────────────────────────────────────────────────────────┐
│ FILTER LOKASI & INDUSTRI   │  INTERACTIVE LEAFLET SPATIAL MAP                              │
│ Provinsi ▾ (Combobox)      │                                                               │
│ Kota/Kab ▾ (Combobox)      │  ● PT Industri Keramik Utama                                  │
│ Jenis Produksi ▾ (Combobox)│  ┌─────────────────────────────────────────────────────────┐  │
│ Posisi Pelanggan ▾ (Select)│  │ 🏬 PT Industri Keramik Utama                             │  │
│ Kawasan ▾ (Select)         │  │ 0042-35-78 · Area Surabaya · SOR-II                     │  │
│                            │  │ [ Tahap 4 — Survei KK0 ]                                │  │
│ LAPISAN PETA (TOGGLES)     │  │ Posisi: Jalur Existing  ·  Sales PIC: Budi Pratama      │  │
│ [✓] ● Tahap 1 — Direktori  │  │                                 [ 🔗 Buka Record Hub ]  │  │
│ [✓] ● Tahap 2 — Plotting   │  └─────────────────────────────────────────────────────────┘  │
│ [✓] ● Tahap 4 — Survei     │                                                               │
│ [✓] ● Tahap 8 — NOL Terbit │                                                               │
└────────────────────────────┴───────────────────────────────────────────────────────────────┘
```

### Key Features
1. **Stage Color-Coded Circle Markers**:
   - `Tahap 1 (Direktori)`: Slate `#94a3b8`
   - `Tahap 2 (Plotting)`: Blue `#60a5fa`
   - `Tahap 3 (Prospek)`: Sky `#38bdf8`
   - `Tahap 4 (Survei)`: Emerald `#34d399`
   - `Tahap 5 (A1)`: Amber `#fbbf24`
   - `Tahap 6 (Permohonan NOL)`: Orange `#fb923c`
   - `Tahap 8 (NOL Terbit)`: Green `#22c55e`
2. **Posisi Pelanggan Border**: Solid border = Jalur Existing, Dashed border = Pengembangan.
3. **Rich Popup Preview Cards**: Clicking a map marker opens a formatted Leaflet popup card displaying Company Name, Nomor, Industry Type, Stage Badge, Posisi, Sales PIC, and a direct `[ 🔗 Buka Record Hub ]` button link.
