# Domain — Master Data & Configuration

> **Canonical.** This document owns the configuration inventory and the seeding
> order. Admin screens are in [design/frontend/10-admin.md](../design/frontend/10-admin.md).

Everything the system needs configured before it can run, and everything an
administrator must be able to change afterwards without a deployment.

---

## 1. How to classify

Five kinds of configurable thing, deliberately distinguished — putting an item in
the wrong bucket is how you end up shipping a release to add a fuel type, or
offering an admin screen that silently does nothing.

| Kind                  | Meaning                                                                 | Lives in                                  |
| --------------------- | ----------------------------------------------------------------------- | ------------------------------------------ |
| **Fixed enum**        | The domain admits exactly these values, and a new one changes behaviour | Code (C# enum + DB check constraint)      |
| **Master data**       | A list that grows or is corrected over time without changing logic      | Database, admin CRUD screen               |
| **Business constant** | A single effective-dated value used in a calculation                    | `system_constant`, admin screen           |
| **Deployment config** | A value the _server_ runs with, agreed with infrastructure              | `appsettings.json` + env vars             |
| **Operational data**  | Entered periodically as part of running the business                    | Owned by Regional Admin, not System Admin |

### The runtime / startup rule

> **If the application is not the only thing that enforces the value, it is not
> master data.** It is deployment configuration, and an admin screen for it is a
> lie.

Upload size limits, session timeouts and password policy all look like settings an
administrator should be able to change. They are not, because the application does
not own them alone — the reverse proxy caps the request body, and ASP.NET Core
Identity binds its options in the DI container at startup. A database row can only
change the application's _third_ opinion on the matter, and a screen that appears
to raise a limit while the proxy still rejects the upload is worse than no screen.
See [§11](#11-business-constants-and-deployment-configuration).

### The `(sebutkan)` rule

> **If the source form offers "Lainnya", "(sebutkan)" or "jika tidak ada bisa
> Input baru", the field must accept free text — never a closed enum.**

The form itself is admitting the list is incomplete, so whatever captures the
value must have an escape hatch. That much is unconditional. What's
**not** automatic is the follow-on question — does the *base list itself*
need to be admin-editable master data, or can it be a fixed set declared in
code alongside the free-text field?

Four items in the sources carry the escape hatch: `Jenis Peralatan Gas`,
`Rencana Pemanfaatan Gas`, `Kebutuhan Energi`, `Bahan Bakar Eksisting`
(`fuel_type`). `Kode Harga` says it outright too, though that one was never a
fixed-option list to begin with.

- **`Jenis Peralatan Gas` and `Rencana Pemanfaatan Gas`** are code-declared
  constants, not master data — a 9-option and a 5-option checklist with no
  evidence PGN wants to add to them without a deployment.
- **`fuel_type`** stays master data — it's a union of two official lists
  that already disagreed once, a different risk than a static checklist.
- **`Kebutuhan Energi`** (`Listrik`/`Steam`/`Panas`/`Dingin`/`Lainnya`) was
  never modelled as its own master-data table either; it's a fixed checkbox
  set with a free-text `Lainnya` field, same shape as the two above.

### Effective dating

Master data that feeds a signed document must not shift under it — a NOL
issued in 2026 has to reprint with 2026's numbers forever. There is no
`price_matrix`, `exchange_rate` or `system_constant` table in v1 — gas
pricing, gas-demand conversion and the evaluation's supply figure are all
plain typed values on the record, already frozen the moment they're saved.
What's left that still needs `effective_from`/`effective_to` is versioned
documents (`reference_document` and the six document templates). Editing
creates a new row; it never overwrites.

---

## 2. Complete inventory

| #                        | Item                                                   | Kind        | Route                           | Owner   |
| ------------------------ | ------------------------------------------------------ | ----------- | -------------------------------- | ------- |
| **Organisation & scope** |
| 1                        | Region (SOR) & Area                                    | Master      | `/master/organisation`           | SYS     |
| 2                        | Users & role assignments                               | Master      | `/master/users`                  | SYS, RA |
| **Geography**            |
| 3                        | Provinsi / Kota-Kabupaten / Kecamatan / Kelurahan-Desa | Master      | *(seeded, no admin route)*       | —       |
| 4                        | Negara (countries)                                     | Master      | `/master/countries`              | SYS     |
| **Classification**       |
| 5                        | Jenis Industri (+ optional KBLI)                       | Master      | `/master/industry-types`         | SYS     |
| **Commercial**           |
| 6                        | Segmen (Sub-Produk)                                    | Master      | `/master/segments`               | SYS     |
| **Energy & conversion**  |
| 7                        | Jenis Bahan Bakar                                      | Master      | `/master/fuel-types`             | SYS     |
| 8                        | **Satuan (units of measure)**                          | Master      | `/master/units`                  | SYS     |
| **Technical**            |
| 9                        | **G-Size / meter sizes**                               | Master      | `/master/meter-sizes`            | SYS     |
| 10                       | **Spesifikasi MRS**                                    | Master      | `/master/mrs-specs`              | SYS     |
| **Documents**            |
| 11                       | Dokumen Acuan Kerja                                    | Master      | `/master/reference-documents`    | SYS     |
| **Workflow**             |
| 12                       | **Kategori alasan revisi/tolak**                       | Master      | `/master/reason-categories`      | SYS     |
| **System — deployment**  |
| 13                       | **Batasan unggahan berkas**                            | Config      | `appsettings.json` + env        | Ops     |
| 14                       | **Kebijakan sandi, lockout & sesi**                    | Config      | `appsettings.json` + env        | Ops     |
| 15                       | **Koneksi & rahasia** (DB, penyimpanan berkas, SMTP)   | Config      | disuntik saat deploy            | Ops     |

Items 13–15 have no admin screen by design — see
[§11](#11-business-constants-and-deployment-configuration).

Several things that would look like natural entries on this list **do not
exist as master data at all**, and it's worth being explicit about why so
nobody rebuilds them by reflex:

- **Exchange rate** — nothing in the system converts IDR to USD; IRR, NPV and
  Payback are typed directly by Regional Admin.
- **Jenis Peralatan Gas** and **Rencana Pemanfaatan Gas** — fixed checklists
  declared in code, not admin-editable lists.
- **Format penomoran dokumen** — the number formats are code constants; only
  the counter is a database row (see [§9](#format-penomoran-dokumen--not-master-data)).
- **Template notifikasi** — message wording is a code constant.
- **Template dokumen** — the six Lampiran `.docx` files ship with the
  application as developer-managed files, not admin uploads.
- **Workflow template** — Regional Admin picks 2–3 reviewers per case; there
  is no pre-configured roster to store.
- **`system_constant`, `allocation`, `gas_balance`** — PGN tracks
  realisasi/alokasi and gas balance themselves; this platform doesn't rebuild
  that tracking, so there's no conversion constant to configure and no
  monthly/quarterly data to seed. See
  [design/reporting](../design/reporting.md#allocation--gas-balance--not-rebuilt-in-system).

---

## 3. Organisation & scope

### Region (SOR) and Area

Not reference data. These define the **scope boundaries every permission check and
approval chain resolves against** ([roles-permissions](../design/roles-permissions.md#4-scope-resolution)),
so they must be seeded before any user can be assigned a role.

```
region                        area
  id                            id
  code       'SOR-II'           region_id     fk
  name       'Region II'        code          'SBY'
  active                        name          'Area Surabaya'
                                active
```

- Regions are numbered I–IV on the official Nota Dinas.
- Deleting an Area that holds records is blocked; deactivate instead.
- Moving an Area between Regions changes the visibility of every record in it —
  require confirmation and log it.

---

## 4. Administrative geography

Four-level Indonesian hierarchy, cascading dropdowns everywhere:

```
Provinsi → Kota/Kabupaten → Kecamatan → Kelurahan/Desa
```

Source: `Entry Apps!F11:F14`, notes `(Drop down : Prov)`, `(Drop down Kota/Kab)`,
`(Drop down Kec)`, `(Drop down Desa/Kel)`.

### Terminology — "regency" means _Kabupaten_

**Level 2 has two kinds of unit, and they are siblings, not parent and child:**

| Indonesian    | English     | Character                                  |
| ------------- | ----------- | ------------------------------------------ |
| **Kabupaten** | **Regency** | The rural/wider unit, headed by a _Bupati_ |
| **Kota**      | **City**    | The urban unit, headed by a _Walikota_     |

Both sit directly under a Provinsi and both divide into Kecamatan — which is why
every form says **`Kota/Kabupaten`**. Wherever these docs say "regency", read it
as **Kota/Kabupaten, the level-2 unit, city or regency alike**.

| Level | Indonesian       | English                 | Count (approx.) |
| ----- | ---------------- | ----------------------- | ---------------- |
| 1     | Provinsi         | Province                | 38              |
| 2     | Kota / Kabupaten | City / Regency          | ~514            |
| 3     | Kecamatan        | District                | ~7,300          |
| 4     | Kelurahan / Desa | Urban village / Village | ~84,000         |

Level 4 splits the same way — **Kelurahan** urban, **Desa** rural.

### BPS / Kemendagri codes

```
35            Jawa Timur                    ← Provinsi
35.78         Kota Surabaya                 ← Kota (level 2)
35.15         Kabupaten Sidoarjo            ← Kabupaten (level 2, same level)
35.78.13      Kecamatan Genteng
35.78.13.xxxx Kelurahan Gentengkali
```

Two things to note for the `Nomor` format
([03](03-directory-plotting.md#the-nomor-format)):

1. **The level-2 code is only unique within its province.** `78` alone is
   ambiguous. `Nomor` carries both codes, and since the sequence is global the
   codes are descriptive rather than part of the uniqueness key — but any _lookup_
   keyed on the level-2 code alone would still be wrong.
2. **Both segments are 2 digits**, matching the worksheet's sample shape
   `0000001-15-20`.

### Modelling

```
regency
  id
  province_id   fk
  bps_code      char(2)      -- unique within province, not globally
  type          enum(kota, kabupaten)
  name          varchar      -- "Surabaya", without the prefix
  deleted_at    timestamptz, null
```

Storing `type` separately from `name` lets the UI render _"Kota Surabaya"_ vs
_"Kabupaten Sidoarjo"_ and lets reports group by kind. Same at level 4.

Seed from the official Kemendagri list (Permendagri 72/2019 and successors) rather
than typing it. **Every filter carries a `Semua` option** — confirmed by four cell
comments on `Directory Industry` and four on `Data Plotting`.

Sample values from the worksheet: `Jawa Timur / Surabaya / Genteng / Gentengkali`
and, on the filter mocks, `Jawa Timur / Kendal / Dawu / Jombok`. Note the mock has
`Kendal` under `Jawa Timur`; Kendal is actually in Jawa Tengah. Dummy data, not a
rule.

**Administrative units change.** Provinces split, regencies get created. Since
`Nomor` embeds the codes and is printed on signed documents, a reorganisation must
never rewrite existing numbers — retire superseded rows (soft-deleted via
`deleted_at`, never hard-deleted), and leave issued `nomor` strings untouched.

**No admin UI for this table.** Reorganisations are rare enough that seeding
once (and retiring rows directly against the database on the rare occasion
one happens) is simpler than building and maintaining a cascading
Provinsi→Regency→District→Village editor — especially at Village's ~84,000-row
scale, which would need its own pagination design distinct from the
small-lookup admin screens elsewhere in this document.

### Negara

Needed by `Bahan Baku` (Negara Asal) and `Orientasi Pasar` (Negara Tujuan) — cell
comments on `Entry Apps!J48:J51` and `J54:J57`. Seed from ISO 3166-1 with
Indonesian names.

---

## 5. Jenis Industri

`Jenis Industri!B3:C24` — 20 types with example products, plus `Semua` at `B4`.

| #   | Jenis Industri           | Contoh Produk                             |
| --- | ------------------------ | ------------------------------------------ |
| 1   | Makanan Minuman          | Wafer, Biskuit                            |
| 2   | Logam Dasar Non Baja     | Profil Aluminium                          |
| 3   | Logam Dasar Baja         | Besi Beton                                |
| 4   | Fabrikasi Logam Non Baja | Furniture                                 |
| 5   | Fabrikasi Logam Baja     | Otomotif, Heat Exchanger                  |
| 6   | Bahan Tekstil            | Kain Sarung, Kain Pakaian                 |
| 7   | Kertas                   | Karton, Packaging                         |
| 8   | Kaca                     | Kaca Otomotif, Kaca Lembaran, Gelas       |
| 9   | Ceramic                  | Keramik Lantai, Keramik Sanitary, Genteng |
| 10  | CNG/LNG                  | CNG, LNG                                  |
| 11  | Kimia                    | Stereofoam, Sabun                         |
| 12  | Smelter                  | Smelter Emas, Smelter Aluminium           |
| 13  | Rubber                   | Ban, Balon                                |
| 14  | Plastic                  | Alat Rumah Tangga, Packaging              |
| 15  | Laundry                  | Jasa Laundry                              |
| 16  | Tobacco                  | Rokok                                     |
| 17  | Wood                     | Furniture, Floring                        |
| 18  | Farmasi                  | Obat, Infus                               |
| 19  | Gas Industri             | H2, O2, CO2                               |
| 20  | Horeka                   | Hotel, Resto, Kafe                        |

Use BPS/KBLI classification if PGN can supply it; otherwise seed the 20
above. Either way this is an **extensible master table**, never an enum.

`Entry Apps!N17` says _"(Drop down : data BPS)"_ — the KBLI classification, which
has thousands of five-digit codes — while `Jenis Industri` is a curated list of 20
sales categories, and the sample value `Jasa Laundry` matches row 15's _example
product_ rather than the category name. **Recommended shape:** `industry_type`
holding the 20 for filtering and reporting, plus an optional `kbli_code` per
company. That satisfies "BPS if possible" without a 1,700-row dropdown.

---

## 6. Commercial

### Segmen (Sub-Produk)

`Tabel!A3:A8` — the sheet exists solely to hold this list:

`Bronze 1` · `Bronze 2` · `Bronze 3` · `Silver` · `Gold` · `Platinum`

Abbreviated `B1`–`B3` in the worksheet's price example; called **`Sub-Produk`**
on Lampiran 17, in the same order. Confirmed as a dropdown by cell comments on
`Entry Apps!L93` and `L108`.

No `price_matrix` table, no `OUP`/`UMP`, no automatic price resolver — gas
pricing is manual entry everywhere. `harga`, `kode_harga`, `currency` and
`unit` are plain columns on `a1_registration` and `nol_request` directly,
typed per record, not resolved from a shared table. `Kode Harga` was already
_"Drop down List, jika tidak ada bisa Input baru"_ (`Entry Apps!L94`) — free
entry, not a hard FK — so nothing changes there; it's the price lookup
behind it that doesn't exist.

`Entry Apps!O92:T98`'s 6×3 price grid was the one worked example in any
source, kept here for context only — not seeded, not queried:

| Segment  | Bulanan (USD) | Tahunan (USD) | Harian (USD) |
| -------- | ------------- | -------------- | ------------- |
| B1       | **10**        | —             | —            |
| B2       | 9.66          | —             | —            |
| B3       | 9.50          | —             | —            |
| Silver   | 9.26          | —             | —            |
| Gold     | 9.18          | —             | —            |
| Platinum | 8.78          | —             | —            |

The worksheet's `10,000` for B1 was a thousands-separator misread — confirmed
`10`, which fits the pattern (prices _decrease_ as segment improves, so B1,
the entry tier, is the most expensive). Lampiran 17 gives the price unit
precisely — `Harga Gas : <<kode harga>>  USD……/MMBtu  atau  Rp………/m³` — which
is why `harga` carries a currency and unit alongside the number.

### No `exchange_rate` table

Capex is IDR, gas price is USD/MMBtu. Converting between them for IRR/NPV/
Payback doesn't need a stored, effective-dated rate: those three figures are
manual entry, computed by Regional Admin outside the system and typed
directly into `nol_evaluation_scenario` as `irr_pct`/`npv`/`payback_years`.
Nothing in the system does the IDR↔USD conversion, so nothing needs to know
which rate was used.

`[ASSUMPTION — no longer load-bearing]` Which rate PGN uses for their own
IRR workings — BI middle rate, a budgeted RKAP rate, a contractual rate — is
still genuinely unknown, and still worth asking if it's useful to PGN. It
just doesn't block anything here anymore.

### Segment assignment

Nothing in any source says **how** a customer is assigned Bronze 1 vs Platinum.
Given prices fall as the tier improves, it is presumably volume- or value-based.
Currently a manual dropdown choice. If PGN has thresholds, they belong here as a
rule table — worth asking.

---

## 7. Energy & conversion

### Jenis Bahan Bakar — union of both sources

| Fuel              | Client `KK0` §12 | Official Lampiran 10 §11 |
| ----------------- | ---------------- | -------------------------- |
| LPG               | ✓                | ✓                          |
| CNG               | ✓                |                            |
| HSD (Solar)       | ✓                | ✓                          |
| MFO               | ✓                |                            |
| Minyak Berat (FO) |                  | ✓                          |
| IDO               |                  | ✓                          |
| MDF               |                  | ✓                          |
| Minyak Tanah      |                  | ✓                          |
| Batubara          | ✓                | ✓                          |
| Cangkang          | ✓                |                            |
| Kayu              | ✓                |                            |
| Listrik           |                  | ✓                          |
| Lainnya           | ✓                | ✓                          |

Seed the **union** — `Cangkang` (palm kernel shell) and `Kayu` are real Indonesian
industrial fuels the 2023 official list omits. The presence of `Lainnya` makes this
master data by the `(sebutkan)` rule.

No `conversion_factor` table and no `gas_calorific_value` constant exist in
v1 — `Konversi ke Gas` on the survey equipment row is typed by the user
directly, not computed from calorific values. The formula it's based on, and
PGN's official calorific references, are documented for context in
[04-prospect-survey.md](04-prospect-survey.md#the-conversion-engine) rather
than modelled as master data here.

### Satuan (units of measure)

The sources define at least eight different unit sets, currently scattered as
ad-hoc enums:

| Set                  | Values                   | Source                |
| --------------------- | ------------------------- | ------------------------ |
| Kapasitas energi     | MW · Ton/Jam · Kkal · TR | `F66:K66`             |
| Pendinginan          | TR · PK · Kw             | comment on `K66`      |
| Pemakaian energi     | Kwh · Ton · Kkal · TR    | `F68:K68`             |
| Konsumsi bahan bakar | Ton · Liter · Kwh        | comment on `L72`      |
| Bahan baku / pasar   | % · Ton · KL · m²        | comments on `M48:M51` |
| Diameter             | Inch · mm                | `I120`                 |
| Volume gas           | m³ · MMBtu               | Lampiran 11, 17        |
| Tekanan              | barg                     | Lampiran 11, 17        |

Consolidate into **one table with dimension and set membership**, rather than eight
enums:

```
unit_of_measure                unit_set_member
  id                             set_code    'capacity' | 'consumption' | …
  code        'Ton/Jam'          unit_id     fk
  name                           sort_order
  dimension   enum(mass, volume, energy, power, length, pressure, flow, ratio)
  deleted_at, null
```

One `<MeasureInput Set="consumption">` component then drives every unit dropdown
in the system ([frontend/11-components.md](../design/frontend/11-components.md#measureinput)),
and adding a unit becomes a data change.

### Jenis Peralatan Gas — not master data

Lampiran 11: `Oven` · `Boiler` · `Air Conditioner` · `Gas Engine` · `Gas Turbine` ·
`Chiller` · `Furnace` · `Dryer` · `Kiln` · **3× `(sebutkan)`**.

Nine options plus three free-text slots — the free text is a form field,
declared alongside the nine constants in the Blazor checkbox component, not
an `equipment_type` table. `a1_registration.jenis_peralatan_gas` stores the
selection as a plain `set` — only the admin-managed source of the option
list is absent.

### Rencana Pemanfaatan Gas — not master data

KK0 §14 / Lampiran 10 §13: `Bahan Baku` · `Bahan Bakar` · `Pembangkit Listrik` ·
`CNG` · `Transportasi Gas` · **`......(Sebutkan)`**. Same call as above.

Two smaller lists follow it, and neither was ever modelled as its own
master-data table: `Kebutuhan Energi` (`Listrik` · `Steam` · `Panas` ·
`Dingin` · **`Lainnya`**) and `Kebutuhan Jasa/Produk` (`Energi` · `Engineering` ·
`O&M`) — both fixed checkbox sets, `Kebutuhan Energi` with its own free-text
`Lainnya` slot.

---

## 8. Technical

### G-Size / meter sizes

`Entry Apps!D124` is `G.Size / Tekanan / Maks Flowrate` with the example
`65 / 1 / 200`, and the cell comment marks it a dropdown. G-Size is a standardised
gas-meter designation, so it belongs in a lookup carrying its ratings:

```
meter_size
  g_size          varchar   -- 'G65'
  nominal_flow    decimal   -- m³/jam
  max_flow        decimal   -- m³/jam
  pressure_rating decimal   -- barg
  deleted_at, null
```

Seed from **PGN's meter catalogue** — the worksheet gives a single data point
(`G65`, 1 barg, 200 m³/jam), which is not enough to infer the series. Selecting a
G-Size should populate max flowrate rather than leaving it free-typed, since the
pair has to be consistent.

### Spesifikasi MRS

`Entry Apps!D122`, and `G-Size MR/S` on Resume Evaluasi §4. Currently modelled as
free text. PGN builds MRS to standard specifications, so a lookup stops the same
station being written five ways across five records — which would make any
reporting on it useless. Confirm whether PGN has a standard catalogue.

### Gas Balance — not master data

Resume Evaluasi §5:

> _"Pasokan masih mencukupi dengan Gas Balance untuk Area …… pada Triwulan ……
> Tahun …… masih terdapat ketersedian Pasokan sebesar …… BBTUD"_

**No `gas_balance` table.** PGN tracks quarterly gas balance per Area
themselves; rebuilding that as stored, seedable master data is scope this
platform doesn't take on. `Ketersediaan Pasokan` is a plain manual field on
the evaluation's supply-analysis panel instead
([frontend/07](../design/frontend/07-evaluation-and-issuance.md#supply-analysis)) —
Regional Admin types in whatever figure their own process produces for this
case. No Area/Triwulan/Tahun lookup, nothing to seed.

---

## 9. Documents

### Dokumen Acuan Kerja

**Not the same thing as a document template, despite both being ".docx-ish
files with versions" — worth being explicit about the difference.** These
are PGN's own *policy* documents — pricing rules, connection-fee rules —
that a NOL request **cites** as the basis for a decision, the way a legal
memo cites a regulation. Nothing merges data into them; the system just
records which version was in force when a given NOL was decided. Compare
[Template dokumen](#template-dokumen--not-master-data) below, which *is*
about generation.

From the docx "LAIN-LAIN" annotations — selectable on the NOL form via _"Drop Down
Daftar Dokumen dan atau Entry manual"_:

1. Ketentuan Produk-Sub Produk, Segmentasi
2. Ketentuan Harga Gas
3. Ketentuan Biaya Penyambungan
4. Ketentuan Jaminan Pembayaran
5. Ketentuan Biaya Sewa Lahan

Versioned, with the file attached. Citing the applicable version is what makes a
decision auditable years later.

`Ketentuan Biaya Sewa Lahan` (land rent) implies a cost element appearing in no
other source. There is no land-rent cost field anywhere in the system — it
exists here as a reference document only.

### Template dokumen — not master data

**Not admin-uploadable.** The six Lampiran `.docx` templates ship as files in
the application, deployed the same way as any other code
([architecture §Document Generator](../build/architecture.md#document-generator)).

The alternative — letting an admin upload a new template revision without a
deployment — sounds appealing because PGN revises procedure `O-001/06.02` on
their own cycle, but nothing would validate that an uploaded `.docx` still
has the merge fields the generator expects. Open XML template-merge fails at
generation time, quietly or loudly depending on what changed, not at upload
time. A developer-managed file goes through the same review a code change
gets, which is the only thing that actually guarantees the next KK0
generates correctly. Getting a new template from PGN to production is still
same-day once the file is in hand — it's a build and deploy, not a
renegotiation.

### Format penomoran dokumen — not master data

Several documents carry generated numbers. The format is a code constant per
document type, not an admin-editable row:

| Document                                 | Format shipped in v1                          | Counter scope                                            |
| ----------------------------------------- | ------------------------------------------------ | ------------------------------------------------------------ |
| Formulir KK0                             | `No. ……KK0/AREA ……/20……` (Lampiran 10 header) | per Area per year                                        |
| Nota Dinas — Permohonan / Penerbitan NOL | `<<Nomor>>` in template                       | per Region per year                                      |
| Company `Nomor`                          | `{seq}-{prov}-{kab}`                          | global ([03](03-directory-plotting.md#the-nomor-format)) |

The "format shipped in v1" column is a best guess built from the one
observed example of each — **PGN hasn't confirmed the real convention**
([stakeholder-brief.md §2](../stakeholder-brief.md), item 7). That no longer
blocks generation the way an empty master-data table would; it just means
the numbers v1 issues may need correcting once PGN answers, which is a code
change and a deploy, not a live data fix.

The counter itself still needs a database row — a format string is
configuration, but `next_seq` is mutable state:

```
document_number_counter
  document_type  enum(kk0, nota_dinas_permohonan, nota_dinas_penerbitan)
  scope_key      varchar   -- area_id or region_id
  period_key     varchar   -- '2026', reset yearly
  next_seq       int
```

Unlike the company `Nomor`, these are **official correspondence numbers** where
gaps invite questions — so they use a locked counter row rather than a
non-transactional sequence. Contention is negligible; these are issued a handful of
times a day. No admin screen — the counter isn't something anyone edits by hand.

---

## 10. Workflow & notification

### Workflow template — not master data

**No `workflow_template` table.** The notulen only asks for Regional Admin to
pick 2–3 reviewers **per case** (*"bisa dipilih reviewernya siapa saja bisa
2-3 reviewer"*) — there's no mention of setting up a default roster in
advance. Area Head and Division Head aren't a workflow-config concern at all;
they're resolved from whoever holds that role for the record's Area/Region
via `/master/users`, the same as any other role-scoped lookup.

What's built instead: the `Tetapkan Reviewer` action on the record itself,
at the point Regional Admin's step is reached
([frontend/07](../design/frontend/07-evaluation-and-issuance.md#assigning-reviewers)).
`workflow_instance`/`workflow_step` still exist as case data — every case
needs a record of who was assigned to which step — they're just populated
directly by that action, not snapshotted from a template that doesn't exist.

No SLA per step either — ageing is plain elapsed time, not a configurable
threshold.

### Notification wording — not master data

**No `notification_template` table.** Every transition still notifies —
submitted, approved, revised, rejected, reviewers assigned, NOL/RL issued —
the bell panel and `Tugas Saya` badge still carry the load that in-app-only
notification (email is deferred) puts on them. What's not built is a
database-backed, admin-editable wording layer: with email disabled and six
in-app events, the message text is a constant in the notification service,
not a `subject`/`body` row with `{{placeholders}}` an admin edits from a
screen.

If PGN asks to reword an in-app message, that's a one-line code change and a
deploy — an acceptable cost at six events. Revisit this if that cadence
ever becomes a problem, or when email is switched on and legal/branding
requirements make templated wording worth the admin-screen cost.

### Kategori alasan revisi/tolak

Comments are mandatory on `Revisi` and `Tolak`, but free text cannot be
grouped — which is exactly what the NOL-outcomes report needs
([reporting](../design/reporting.md)). A short optional category beside the
comment makes that report possible.

**Do not invent the categories.** Ask PGN; shipped with an empty list it degrades
gracefully to free text only.

---

## 11. Business constants and deployment configuration

Two different kinds of value, kept apart. The test is **what does the value have
to agree with?**

|                 | Business constant                              | Deployment config                     |
| --------------- | ------------------------------------------------ | ---------------------------------------- |
| Lives in        | `system_constant` table                        | `appsettings.json`, overridden by env |
| Changed by      | System Admin, in the UI                        | Ops, in the deployment                |
| Takes effect    | Immediately                                    | Next start                            |
| Effective-dated | **Yes** — last year's documents must reproduce | No — one current value                |
| Must agree with | Nothing outside the app                        | Kestrel, the proxy, the DI container  |

The reproducibility row is the sharper test. A NOL signed in 2026 must reprint with
2026 numbers forever, so every value feeding a document carries `effective_from` and
every computed figure stores the id of the row it used. Nothing on the right-hand
side has that property: nobody needs to know what the upload limit was in March.

### Business constants — none in v1

`system_constant` has no rows and `/master/system-constants` doesn't exist as
a screen. The classification and the table above stay documented — a real
business constant may turn up later — there just isn't one in this system
yet.

### Deployment config — `appsettings.json` + environment

Not in the database, and **deliberately not admin-editable**.

```jsonc
// appsettings.json — defaults, committed, safe to read in a pull request
{
  "Upload": {
    "MaxSizeMb": 25,
    "AllowedTypes": ["pdf", "docx", "xlsx", "jpg", "png"],
  },
  "Auth": {
    "SessionTimeoutMinutes": 60,
    "Password": {
      "MinLength": 12,
      "RequireMixed": true,
      "HistoryCount": 3,
      "ExpiryDays": 0,
    },
    "Lockout": { "MaxAttempts": 10, "Minutes": 15 },
  },
}
```

Every key is overridable by an environment variable using ASP.NET Core's `__`
separator, so no environment needs the file edited:

```bash
Upload__MaxSizeMb=100
Auth__Password__MinLength=14
Auth__Lockout__Minutes=30
```

Bound to typed options at startup and injected as `IOptions<UploadOptions>`. The
uploader still tells the user _"maks. 25 MB"_ — it just reads that from
configuration instead of a table.

#### Why `upload_max_size_mb` in particular cannot be a row

The limit is enforced in three places, and the application owns only the last:

| Layer              | Setting                                    | Owned by |
| ------------------ | ---------------------------------------------- | -------- |
| Reverse proxy      | nginx `client_max_body_size`               | Ops      |
| Kestrel            | `MaxRequestBodySize` — bound at host build | Startup  |
| Blazor `InputFile` | `maxAllowedSize` on `OpenReadStream`       | The app  |

An admin screen could change the third. Raise the value there and nowhere else and
you get a UI promising 100 MB above an upload that dies at the proxy with a **413**
— a setting that appears to work and does not. Silent disagreement between layers
is the failure mode, and the only fix is to change them together, which means
changing them where they live.

The auth values fail the same way. Identity binds `PasswordOptions` and
`LockoutOptions` in the DI container at `AddIdentity()`, and cookie `ExpireTimeSpan`
at `AddAuthentication()` — startup, once. Sourcing them from the database means
either restarting anyway or re-implementing validation outside Identity's pipeline,
which is how work factors quietly get lowered.

> **The cost is honest and worth stating:** PGN cannot self-serve a password-policy
> change. That is a deployment task. Given the alternative is a screen whose
> settings may or may not be the ones actually in force, it is the right trade.

#### Secrets — out of source control, out of the image

The connection string, storage credentials and (when email is enabled) the SMTP
password are configuration like everything else here. The rule is not _"secrets
may not live in `appsettings.json`"_ — it is:

> **A secret must not be committed to the repository, and must not be baked into
> the container image.**

`appsettings.json` fails both, which is why it holds only defaults that are safe
to read in a pull request. Two consequences make this worth being strict about:

- **Committing is not reversible.** Rotating the credential does not remove it
  from git history, from every clone, fork and CI cache that already pulled it.
- **Baking is invisible.** A secret in the image is readable by anyone who can
  pull the image, and rotating it means rebuilding rather than restarting.

Anything that satisfies the rule is acceptable, and ASP.NET Core's configuration
system layers them transparently — the code reads `IOptions<T>` either way:

| Where                                                                   | Use                                                                                                                   |
| ------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------- |
| **Docker/Compose secrets** → `/run/secrets/*`, read via `AddKeyPerFile` | **Preferred for this deployment**                                                                                     |
| **Environment variables** (`Storage__OneDrive__ClientSecret=…`)         | Fine, and simplest                                                                                                    |
| **A non-committed `appsettings.Production.json`**, mounted at runtime   | Fine — the file is a deployment artefact, not a repo artefact                                                         |
| **`appsettings.Local.json`**, gitignored _and_ dockerignored            | Developer machines ([layering](../build/architecture.md#the-layering-and-where-a-developer-puts-their-own-values)) |
| `dotnet user-secrets`                                                   | Developer machines; stored outside the repo tree, so it cannot be committed by accident                               |

Prefer mounted secret **files** over environment variables where the platform
offers them. Environment variables are readable from `/proc/<pid>/environ`, are
inherited by every child process, and show up in `docker inspect` and in crash
dumps; a file can be mounted read-only and permissioned.

```bash
# environment-variable form
ConnectionStrings__Default=Host=…;Username=…;Password=…
Storage__S3__SecretKey=…                 # when Storage__Type=S3
Storage__OneDrive__ClientSecret=…        # when Storage__Type=OneDrive
```

**Attachment storage is itself type-switched** — `Storage:Type` selects `S3` or
`OneDrive` and only the matching credential block is required or validated
([storage](../build/storage.md)). It is deployment config for the same
reason as the rest of this section: the credentials belong to whoever runs the
server, and the choice is made once per environment, not per user.

> ⚠️ **The OneDrive client secret is the highest-value credential in this system.**
> `Files.ReadWrite.All` is an **application** permission and is **tenant-wide** —
> it does not stop at the Simando document library. That one string grants
> read/write over every file in PGN's OneDrive and SharePoint. Treat it
> accordingly: mounted file, restricted permissions, rotation schedule agreed with
> PGN's IT, and ideally a **certificate** rather than a shared secret
> ([storage §8](../build/storage.md#8-what-we-need-from-pgn)).

---

## 12. Fixed enums — _not_ master data

These change behaviour, so they live in code with a database check constraint.
Listed explicitly so nobody builds an admin screen for them:

| Enum                      | Values                                                                                                                                            |
| -------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| `posisi_pelanggan`        | Pengembangan · Jalur Existing _(also surfaced as `Jalur Pipa` on the Plotting screen — one field, two labels)_ |
| `kawasan`                 | Kawasan Industri · Non Kawasan Industri                                                                                                           |
| `basis_kontrak`           | Harian · Bulanan · Tahunan                                                                                                                        |
| `skema_harga`             | Reguler · SiGas · Bersyarat                                                                                                                       |
| `asal`                    | Impor · Lokal                                                                                                                                     |
| `status_bangunan`         | Dalam rencana · Dalam pembangunan · Eksisting · Proses ekspansi                                                                                   |
| `sektor`                  | Komersial · Industri · Transportasi                                                                                                               |
| `skema_pembayaran`        | Jaminan Pembayaran · Pembayaran Dimuka                                                                                                            |
| `status_capel`            | Calon Pelanggan · Eks Pelanggan                                                                                                                   |
| `status_rkap`             | RKAP · Non RKAP                                                                                                                                   |
| `feed_status`             | Belum · Dalam proses · Selesai                                                                                                                    |
| `registration_type`       | Registrasi baru · Amendemen · Perpanjangan                                                                                                        |
| `signature_method`        | Wet · Digital — declared metadata on upload, never verified                                                                                       |
| `outcome`                 | NOL · RL                                                                                                                                           |
| `shift`                   | 1 · 2 · 3                                                                                                                                          |
| `nama_hari`               | Senin … Minggu                                                                                                                                    |
| `attachment.kind`         | 15 values — see [data-model](../design/data-model.md#attachment)                                                                                       |
| Workflow status & actions | See [approval-workflow](../design/approval-workflow.md)                                                                                                       |
| Roles                     | 6 — see [roles-permissions](../design/roles-permissions.md#2-role-catalogue)                                                                                  |

`shift` is borderline: `Entry Apps!J60` says _"Drop down (1, 2, 3)"_, a closed set
in practice. Keep it an enum until someone reports a four-shift plant.

---

## 13. Seeding checklist

Nothing works until these exist. Ordered by dependency — this is the go-live
prerequisite list.

| #   | Item                                           | Blocks                 | Source                                                                                                                                                                   |
| --- | ------------------------------------------------ | ------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1   | Regions (SOR I–IV) & Areas                     | **User assignment**    | PGN org chart                                                                                                                                                            |
| 2   | Administrative geography + BPS codes           | Company creation       | Kemendagri                                                                                                                                                               |
| 3   | **User accounts, roles & temporary passwords** | **All access**         | 🚧 PGN — we cannot read their directory, so PGN must supply the list of people, their Areas and their roles |
| 4   | Units of measure                               | Survey, A1             | This document                                                                                                                                                            |
| 5   | Fuel types                                     | Survey                 | Union list above                                                                                                                                                         |
| 6   | Industry types                                 | Company creation       | 20-row list, or KBLI                                                                                                                                                     |
| 7   | Countries                                      | Survey                 | ISO 3166-1                                                                                                                                                               |
| 8   | Segments                                       | A1                     | 6-row list                                                                                                                                                               |
| 9   | Document templates (6), as a build artefact   | Document generation    | 🚧 Lampiran 16 not yet supplied — a developer packages the six `.docx` files with the release, but still needs the file from PGN first |
| 10  | Meter sizes (G-Size)                           | Evaluation             | 🚧 PGN meter catalogue                                                                                                                                                   |
| 11  | Reference documents (5)                        | NOL request            | PGN, versioned                                                                                                                                                           |

No conversion-factor, price-matrix, exchange-rate, equipment-type,
gas-usage-type, notification-template, document-numbering, workflow-template,
allocation or gas-balance rows: gas-demand conversion, gas pricing, the
equipment and gas-usage checklists, notification wording, document numbering,
reviewer assignment, and supply availability are all either manual entry,
per-case action, or code constants, so none of them need seed data to ship.
Document numbering ships with a best-guess format instead of nothing — see
[§9](#format-penomoran-dokumen--not-master-data).

🚧 = blocked on PGN input. **Three of eleven.** None of them block a core
calculation anymore — the two that used to (conversion, pricing) are
resolved by going manual. Worth putting the rest to the client as one list
rather than discovering them one at a time during build.

**Inventory items 14–16 are not on this list** — deployment config is not seeded, it
is set when the environment is built. It belongs in the deployment runbook instead,
and two of its values need agreeing with PGN before go-live rather than after:
**password policy** (theirs may differ from our defaults) and the **upload size
limit** (which must be set at the reverse proxy in the same change).
