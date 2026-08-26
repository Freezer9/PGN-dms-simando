# Frontend 02 — Dashboard

The landing page. **Different roles get different dashboards**, because the
question each one arrives with is different:

| Role | Arrives asking |
|---|---|
| Sales Area | *What do I need to work on, and what came back to me?* |
| Area Head / Reviewer / Division Head | *What is waiting on me?* |
| Regional Admin | *What is stuck?* |
| System Admin | *What is the system configuration & health status?* |

Build one page with role-conditional widget composition, not four pages.

All dashboards share unified design system elements:
- **Hero Quick Action Bar**: Direct button shortcuts at the top right of each dashboard for high-frequency actions (`+ Tambah Perusahaan Baru`, `Peta Explorer`, `Kelola Tugas Tertahan`, etc.).
- **Top Row Unified KPI Stat Cards (`BbStatTile`)**: 4 standardized metric tiles displaying core totals, warning alerts, and status indicators.
- **Title Case Card Headers**: All dashboard card titles use standardized Title Case for consistent visual hierarchy.

---

## Sales Area (`SalesAreaDashboard.razor`)

```
┌────────────────────────────────────────────────────────────────────────────────┐
│  Beranda Sales Area — Area Surabaya                         Agustus 2026       │
│  Area: Surabaya · Ringkasan tugas, pipeline sales...                          │
│                                 [ + Tambah Perusahaan Baru ] [ Peta Explorer ] │
├────────────────────────────────────────────────────────────────────────────────┤
│  ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐ ┌────────────┐  │
│  │ Total Pipeline   │ │ Perlu Tindakan   │ │ Dalam Persetuj.  │ │Terbit NOL  │  │
│  │     124          │ │     2 (Revisi)   │ │     5            │ │    2       │  │
│  └──────────────────┘ └──────────────────┘ └──────────────────┘ └────────────┘  │
│                                                                                │
│  ⚠️  PERLU TINDAKAN ANDA                                                        │
│  ┌──────────────────────────────────────────────────────────────────────────┐ │
│  │ 🔴 PT Indah Kejora      Dikembalikan (Revisi) oleh Area Head · 3 hari     │ │
│  │    "Kapasitas boiler tidak sesuai dengan lampiran KK0"                    │ │
│  │                                                          [ Buka Record ]  │ │
│  │ 🟡 PT Kota Baru         Ditolak · dikembalikan Admin Regional · 1 hari    │ │
│  │                                                          [ Buka Record ]  │ │
│  └──────────────────────────────────────────────────────────────────────────┘ │
│                                                                                │
│  PIPELINE PENJUALAN SAYA                                                       │
│  ┌────────┬────────┬────────┬────────┬────────┬────────┬────────┬────────┐    │
│  │Direkt. │Plotting│Prospek │Survei  │  A1    │Permoh. │Evaluasi│  NOL   │    │
│  │  124   │   61   │   31   │   14   │   6    │   3    │   1    │   2    │    │
│  └────────┴────────┴────────┴────────┴────────┴────────┴────────┴────────┘    │
│         ↑ klik untuk memfilter Direktori pada tahap tersebut                   │
│                                                                                │
│  DALAM PROSES PERSETUJUAN                                                     │
│  ┌──────────────────────────────────────────────────────────────────────────┐ │
│  │  PT Big Note      Reviewer 2   ⏱ 11h                                     │ │
│  │  PT Desa Makmur   Admin Reg.   ⏱ 4h                                      │ │
│  │  PT Esteemje      Area Head    ⏱ 1h                                      │ │
│  └──────────────────────────────────────────────────────────────────────────┘ │
│                                                                                │
│  ┌──────────────────────────────────────────────────────────────────────────┐ │
│  │  PETA SEBARAN PELANGGAN & PROSPEK                      [ Buka Peta Penuh ]│ │
│  │  ●●  ●    ●●●        ● Direktori  ● Survei  ● A1  ● NOL (Interactive Map)│ │
│  └──────────────────────────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────────────────────────┘
```

### Why "Perlu Tindakan Anda" is first

Sales Area holds no approval step, so they have no inbox — but records **do** come
back to them via `Revisi` and `Tolak`. Without this panel those records are
invisible until someone chases by phone, which is precisely the failure the client
described.

**Show the reviewer's comment inline.** It is mandatory, so it always exists,
and reading it is the whole point of the notification.

### Widgets

| Widget | Content | Links to |
|---|---|---|
| **Top KPI Stat Cards (`BbStatTile`)** | Total Pipeline, Perlu Tindakan Anda, Dalam Persetujuan, Terbit NOL | Direct metrics |
| **Perlu Tindakan Anda** | Records returned by `Revisi`/`Tolak`, newest first, with comment | Record hub |
| **Pipeline Penjualan Saya** | Count per stage, scoped to Area | Directory filtered by stage |
| **Dalam Proses Persetujuan** | Submitted records, current holder, ageing | Record hub |
| **Peta Sebaran Pelanggan & Prospek** | Live interactive Leaflet preview map (~220 px tall) with stage-colored circle markers | Full map (`/map`) |

---

## Approver — Area Head, Reviewer, Division Head (`ApproverDashboard.razor`)

```
┌────────────────────────────────────────────────────────────────────────────────┐
│  Beranda Area Head · Area Surabaya                                             │
│  Lingkup: Scope · Daftar tugas verifikasi & persetujuan berkas                 │
│                                           [ Antrean Tugas Saya ] [ Direktori ] │
├────────────────────────────────────────────────────────────────────────────────┤
│  ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐ ┌────────────┐  │
│  │ Menunggu Persetuj│ │ Total Record     │ │ Terbit NOL Bulan │ │ Disetujui  │  │
│  │     3            │ │    184           │ │     2            │ │    18      │  │
│  └──────────────────┘ └──────────────────┘ └──────────────────┘ └────────────┘  │
│                                                                                │
│  MENUNGGU PERSETUJUAN ANDA (3)                             [ Lihat Semua ]     │
│  ┌────────────────────────┬────────────┬──────────────┬───────┬──────────┐   │
│  │ Nama Perusahaan        │ Tahap      │ Diajukan oleh│Durasi │          │   │
│  ├────────────────────────┼────────────┼──────────────┼───────┼──────────┤   │
│  │ PT Indah Kejora        │ Permoh. NOL│ Budi S.      │🔴 11d │[ Tinjau ]│   │
│  │ PT Big Note            │ Permoh. NOL│ Budi S.      │🟡  4d │[ Tinjau ]│   │
│  │ PT Kota Baru           │ Permoh. NOL│ Rina A.      │🟢  1d │[ Tinjau ]│   │
│  └────────────────────────┴────────────┴──────────────┴───────┴──────────┘   │
│                                                                                │
│  ┌──────────────────────────────────┬───────────────────────────────────────┐ │
│  │  RINGKASAN                       │  KINERJA PERSETUJUAN SAYA             │ │
│  │  Total record aktif      184     │  Rata-rata waktu tinjau   1,8 hari    │ │
│  │  Menunggu persetujuan      3     │  Disetujui bulan ini        18        │ │
│  │  Terbit NOL bulan ini      2     │  Revisi / Tolak bulan ini    4        │ │
│  └──────────────────────────────────┴───────────────────────────────────────┘ │
│                                                                                │
│  AKTIVITAS TERBARU                                                             │
│  ● 09:14  Sari W. Setuju PT Esteemje → Admin Regional                          │
│  ● 08:02  Budi S. Submit PT Kota Baru                                          │
└────────────────────────────────────────────────────────────────────────────────┘
```

---

## Regional Admin (`RegionalAdminDashboard.razor`)

- **Hero Quick Action Bar**: `[ Kelola Tugas Tertahan ]`, `[ Laporan Ageing ]`, `[ Direktori Region ]`.
- **Top Row KPI Stat Cards**: `Total Pipeline Region`, `Tugas Tertahan` (rose tinted), `Menunggu Tindakan Saya` (amber tinted), `Total Berkas Berjalan`.
- **Attention Grid & Corong Region**: Stuck tasks queue, Stage 7 evaluation queue, and regional funnel summary.

---

## System Admin (`SystemAdminDashboard.razor`)

- **Hero Quick Action Bar**: `[ Langkah Tertahan ]`, `[ Audit Break-Glass ]`, `[ Organisasi & Akses ]`.
- **Top Row KPI Stat Cards**: `Pengguna Aktif`, `Sales Area Terdaftar`, `Template Dokumen`, `Kesehatan Master Data`.
- **Master Data Health Checklist & Quick Access Grid**: Fast action cards for Industry Types, Fuel Types, Meter Sizes, and Reference Documents.
