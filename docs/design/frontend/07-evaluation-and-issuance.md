# Frontend 07 — Evaluation & Issuance

Stages 7–8, the Region's half of the process. Field definitions in
[06-nol.md](../../domain/06-nol.md).

Two roles only: **Regional Admin** owns evaluation, **Division Head** owns
issuance.

---

## Evaluasi tab — Regional Admin

Visible only to Regional Admin and System-scoped readers; hidden for Sales Area
and Area Head, whose involvement ended at Lampiran 17.

### FEED checkpoint — first, because it blocks everything

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│  FEED — Front End Engineering Design                                                 │
│  Disusun oleh Fungsi Infrastruktur (di luar sistem ini)                              │
│                                                                                      │
│  Status *   ( ) Belum   (●) Dalam proses   ( ) Selesai                               │
│  Tanggal selesai  [            ]        ( ⬆ Unggah dokumen FEED )                    │
│                                                                                      │
│  ⚠️ Analisis kelayakan tidak dapat diselesaikan sebelum FEED selesai.                 │
└──────────────────────────────────────────────────────────────────────────────────────┘
```

FEED produces the pipe routing, MRS specification and capex estimate that populate
everything below, so it leads the tab and gates completion.

### Gate Review data

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│  DATA GATE REVIEW                          Tanggal GR [ 02/09/2026 ]  ( ⬆ Dokumen )  │
│  🚧 Diisi manual — integrasi Gate Review belum tersedia.                              │
│                                                                                      │
│  Total Nilai Capex          Rp [ 3.100.000.000 ]                                     │
│  Pipa Induk / Cabang        Panjang [ 1.250 ] m   Diameter [  8 ] inch ▾             │
│  Pipa Servis                Panjang [   350 ] m   Diameter [  4 ] inch ▾             │
│  Spesifikasi MRS            [ MRS-B 65 / 1 barg                                  ]   │
│  G-Size / Tekanan / Flowrate  [ 65 ]  [ 1 ] barg  [ 200 ] m³/jam                     │
│  Maksimum Kapasitas Meter   [ 200 ] m³/jam                                           │
│  Durasi Pelaksanaan         [  8 ] bulan sejak PJBG diterima PMO                     │
└──────────────────────────────────────────────────────────────────────────────────────┘
```

The `🚧` banner is honest signposting — these fields are meant to arrive by
integration and currently do not; see
[docs/future](../../future/README.md#gate-review-integration).

### Supply analysis

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│  ANALISIS PASOKAN                                                                    │
│                                                                                      │
│  ┌────────────────────────────────────────────────────────────────────────────────┐ │
│  │  Ketersediaan pasokan     [    39,35 ] BBTUD                                   │ │
│  │  Kebutuhan calon pelanggan 3,89 BBTUD  ← dari Survei (116.667 m³/bln)          │ │
│  │  Sisa setelah pelanggan   {{ 35,46 }} BBTUD    ✅ mencukupi                     │ │
│  └────────────────────────────────────────────────────────────────────────────────┘ │
│  Kesimpulan  [ Pasokan masih mencukupi untuk Area Surabaya…                     ]    │
└──────────────────────────────────────────────────────────────────────────────────────┘
```

`Ketersediaan pasokan` is a **plain manual field** — Regional Admin types in
whatever figure their own supply-tracking process produces for this case, the
same as IRR/NPV/Payback and gas pricing. No Area/Triwulan/Tahun lookup, no
stored allocation or gas-balance history —
this platform doesn't track PGN's supply capacity, only this case's evaluation.
`Kebutuhan calon pelanggan` still pulls from the survey automatically (it's
already on the record), and `Sisa setelah pelanggan` is the one derived
figure — plain subtraction between a typed number and a known one, not a
lookup — so the Resume Evaluasi §5 question still gets a checked answer
rather than admin arithmetic.

### Feasibility

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│  ANALISIS KELAYAKAN                                          [ + Tambah skenario ]   │
│  ┌───────────────────────────┬──────────────────────┬──────────────────────┐        │
│  │ Indikator                 │ Harga USD 9,18       │ (skenario 2)         │        │
│  ├───────────────────────────┼──────────────────────┼──────────────────────┤        │
│  │ Internal Rate of Return   │ [ 14,2 ] %           │ [        ] %         │        │
│  │ Pay Back Period           │ [  6,1 ] tahun       │ [        ] tahun     │        │
│  │ Net Present Value ⁽ⁱ⁾     │ Rp [ 1.240.000.000 ] │ Rp [               ] │        │
│  │ Hasil Analisis            │ [ Layak          ▾ ] │ [               ▾ ]  │        │
│  └───────────────────────────┴──────────────────────┴──────────────────────┘        │
│  ⁽ⁱ⁾ NPV disimpan untuk keperluan internal, tidak dicetak pada Resume Evaluasi.      │
│                                                                                      │
│  ANALISIS KOMPETITOR                                                                 │
│  Radius kompetitor [ 12 ] km    ( ) Berada di zona  (●) Tidak berada di zona          │
│  Nama kompetitor / bahan bakar substitusi [ Batubara — CV Sinar Energi           ]   │
│  Harga bahan bakar kompetitor  Rp [ 1.500 ] / kg    ( ⬆ Unggah data kompetitor )     │
└──────────────────────────────────────────────────────────────────────────────────────┘
```

The second scenario column exists because the official Resume Evaluasi compares
two prices side by side. v1 renders one and allows adding a second — see
[docs/future](../../future/README.md#two-scenario-feasibility-comparison) —
the schema already supports N.

**NPV is captured but not printed**, and the footnote says so, because the
official form carries only IRR and Payback.

### Resume Evaluasi composer

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│  RESUME EVALUASI / ANALISIS                            ( 👁 Pratinjau )  ( ⬇ .docx )  │
├──────────────────────────────────────────────────────────────────────────────────────┤
│  I. DATA PENDUKUNG                                                                   │
│  1. Status Capel di RKAP   (●) RKAP  ( ) Non RKAP   di Area [ Surabaya ▾ ]           │
│  2. Status JP              [ Jaminan pembayaran akan disediakan apabila…         ]   │
│  3. Target tandatangan PJBG[ Calon pelanggan berkomitmen untuk segera…           ]   │
│  4. Gate Review            {{ terisi otomatis dari bagian di atas }}                 │
│  5. Analisis pasokan       {{ terisi otomatis }}                                     │
│  6. Analisis kelayakan     {{ terisi otomatis }}                                     │
│  7. Analisis Kompetitor    {{ terisi otomatis }}                                     │
│  8. Lain-lain              [                                                     ]   │
│                                                                                      │
│  II. HASIL ANALISIS                                                                  │
│  ( ) Dapat disetujui     ( ) Ditolak                                                 │
│  [                                                                               ]   │
│                                                                                      │
│  Disiapkan oleh: Sari W. — Fungsi Sales & Customer Management Regional               │
└──────────────────────────────────────────────────────────────────────────────────────┘
```

Sections 4–7 are **derived from fields already entered above**, not retyped. The
composer is an assembly and review screen; only the narrative sections take free
text.

### Assigning reviewers

Regional Admin's distinctive action — they choose the chain before approving into
it ([approval-workflow](../approval-workflow.md#reviewer-configuration)).
Chosen fresh for **this case**, every time — there's no pre-set default to pull
from.

```
┌────────────────────────────────────────────────────────────────────┐
│  Tetapkan Reviewer                                                 │
│                                                                    │
│  Jumlah reviewer   (●) 2      ( ) 3                                │
│                                                                    │
│  Reviewer 1 *  [ Andi P. — PIC Area Support           ▾ ]          │
│  Reviewer 2 *  [ Dewi K. — PIC Leader Area Support    ▾ ]          │
│                                                                    │
│  ⚠️ Budi S. tidak dapat dipilih — beliau membuat record ini.        │
│                                                                    │
│                                    ( Batal )   [ Simpan ]          │
└────────────────────────────────────────────────────────────────────┘
```

The exclusion notice is the **segregation-of-duties** control made visible
([roles-permissions §4](../roles-permissions.md#segregation-of-duties)) — the creator
cannot be a reviewer of their own case. Show why the name is missing rather than
silently omitting it.

---

## Penerbitan tab — Division Head

The terminal decision. Two outcomes, and the screen makes both explicit rather
than hiding refusal behind a generic reject.

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│  Penerbitan NOL / RL — PT Indonesia 1945                                             │
├──────────────────────────────────────────────────────────────────────────────────────┤
│  RINGKASAN KEPUTUSAN                                                                 │
│  Kebutuhan Gas   116.667 m³/bln    Segmen  Gold      Harga  USD 9,18 /MMBtu          │
│  Kontrak Min/Maks 80.000 / 110.000 Capex   Rp 3,10 M IRR/PBP 14,2 % / 6,1 th         │
│  Rekomendasi Admin Regional: ✅ Dapat disetujui           ( Lihat Resume Evaluasi )   │
├──────────────────────────────────────────────────────────────────────────────────────┤
│  KEPUTUSAN *                                                                         │
│    (●) Menyetujui — terbitkan NOL (No Objection Letter)                              │
│    ( ) Tidak menyetujui — terbitkan RL (Refusal Letter)                              │
│                                                                                      │
│  ── jika NOL ──────────────────────────────────────────────────────────────────────  │
│  ISI PERSETUJUAN                                                                     │
│  (●) Sama dengan permohonan      ( ) Ubah ketentuan                                  │
│  ┌───┬──────────────────────┬───────────┬─────────────────┬─────────────────┐       │
│  │ 1 │ 01/01/27 – 31/12/27  │  95.000   │      80.000     │     110.000     │       │
│  └───┴──────────────────────┴───────────┴─────────────────┴─────────────────┘       │
│                                                                                      │
│  Kontrak Bersyarat                                        [ + Tambah syarat ]        │
│  1. [ Jaminan pembayaran diserahkan selambatnya 30 hari sejak NOL terbit.      ] (✕)│
│  2. [ Pemasangan pipa instalasi menjadi tanggung jawab calon pelanggan.        ] (✕)│
│                                                                                      │
│  Persetujuan berlaku sejak Tanggal Dimulai s.d  [ 31/12/2028 ]                       │
│                                                                                      │
│  Nomor Nota Dinas  {{ dibuat otomatis saat terbit }}                                 │
├──────────────────────────────────────────────────────────────────────────────────────┤
│  ( Tolak )   ( Minta Revisi )                        [ Terbitkan NOL ]               │
└──────────────────────────────────────────────────────────────────────────────────────┘
```

### Design notes

- **`Isi Persetujuan` is a separate block from the request.** Lampiran 16 lets
  approval modify the terms, so approved terms are stored as their own records —
  never by overwriting the request
  ([data-model](../data-model.md#nol_issuance--stage-8)).
- **`Kontrak Bersyarat` conditions are a numbered list**, matching *"Kontrak
  Bersyarat: 1. … 2. …"*.
- Selecting **RL** collapses the approval block and reveals a mandatory reason
  field; the primary button becomes `[ Terbitkan RL ]` in red.
- The decision summary at the top exists so the Division Head can decide without
  reading eight tabs, with the Resume Evaluasi one click away.

### Confirmation

Issuance is irreversible and generates a signed document, so it confirms
explicitly:

```
┌────────────────────────────────────────────────────────────────────┐
│  Terbitkan No Objection Letter?                                    │
│                                                                    │
│  PT Indonesia 1945 · 0000042-35-78                                 │
│  Berlaku sejak Tanggal Dimulai s.d 31/12/2028                      │
│  2 kontrak bersyarat dilampirkan                                   │
│                                                                    │
│  Dokumen NOL akan dibuat dan dikirim ke pembuat record,            │
│  Area Head, dan Admin Regional. Tindakan ini tidak dapat           │
│  dibatalkan.                                                       │
│                                                                    │
│                              ( Batal )   [ Ya, Terbitkan NOL ]     │
└────────────────────────────────────────────────────────────────────┘
```

On success the record hub's status card becomes:

```
┌───────────────────────────────┐
│  ✅ NOL TERBIT                │
│  0142/ND/SOR-II/2026          │
│  Berlaku s.d 31/12/2028       │
└───────────────────────────────┘
```

and the stepper shows all eight stages complete. The record becomes read-only for
everyone; only document download remains available.
