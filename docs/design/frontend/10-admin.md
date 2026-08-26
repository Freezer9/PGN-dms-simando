# Frontend 10 — Admin

Two distinct administrative surfaces, deliberately separated:

| Surface | Owner | Scope |
|---|---|---|
| **Master data & system** | System Admin | All regions |
| **Region administration** | Regional Admin | Own region only |

System Admin never sees case data; Regional Admin never edits corporate master
data ([roles-permissions §2](../roles-permissions.md#2-role-catalogue)).

---

## Master data — System Admin

Consistent CRUD frame across all entities. Most are a plain named list —
**Segmen** (`/master/segments`) is exactly this shape: `Bronze 1`, `Bronze 2`,
`Bronze 3`, `Silver`, `Gold`, `Platinum`, add/rename/reorder, nothing else. It
used to be a combined "Segmen & Harga" screen with a price matrix attached,
but gas pricing is manual entry now — `Harga`, `Kode Harga`, currency and
unit are typed per record on the A1/NOL
forms, not looked up from an admin-managed table — so there's no price data
left for this screen to hold, and no effective-dating concern either: a
segment *name* doesn't need version history the way a *price* did.

### Entities

Grouped in the sidebar into six sections. The complete inventory with
sources, ownership and seeding order is in
[master-data.md §2](../../domain/master-data.md#2-complete-inventory)
(15 items, most but not all a distinct screen); the highlights follow.

| Entity | Route | Notes |
|---|---|---|
| **Organisasi** | `/master/organisation` | **SOR regions and Areas.** Not reference data — these define the scope boundaries every permission and approval chain resolves against, so they must exist before any user can be assigned. Deleting an Area with records is blocked |
| **Jenis Industri** | `/master/industry-types` | Extensible list; optional KBLI code |
| **Segmen** | `/master/segments` | Six-tier list, name only — above |
| **Jenis Bahan Bakar** | `/master/fuel-types` | Union of both source lists, including Cangkang and Kayu |
| **Dokumen Acuan** | `/master/reference-documents` | The five *Ketentuan* documents, versioned with file attachments |
| **Satuan** | `/master/units` | One table for all eight unit sets, with `dimension` and set membership |
| **G-Size / Meter** | `/master/meter-sizes` | Nominal & max flow, pressure rating. Selecting a G-Size populates max flowrate |
| **Spesifikasi MRS** | `/master/mrs-specs` | Stops the same station being written five ways |
| **Kategori Alasan** | `/master/reason-categories` | Optional grouping for Revisi/Tolak; ships empty |
| **Langkah Tertahan** | `/admin/stuck-steps` | All regions. Company name, step and assignee only — **not** the record |
| **Akses Darurat** | `/admin/break-glass` | Request 60-min read on one record, reason required, loudly audited |

No `Kurs USD/IDR`, `Jenis Peralatan Gas`, `Rencana Pemanfaatan Gas` or
`Template Notifikasi` screens, no `Format Penomoran` screen, and no `Alur
Kerja` screen either — none of those six are master data.

### The two recovery screens

The last two rows are not master data and are easy to misread, so:

**`/admin/stuck-steps` — the cross-region backstop.** Regional Admin already has
[`/tasks/blocked`](08-tasks-and-approvals.md#tugas-tertahan--regional-admin), but
it is scoped to their own region. That fails when the Regional Admin *is* the
problem — left PGN, on leave, account deactivated — because then nobody in that
region can unstick anything and cases sit there silently.

This view is the fallback, and it also resolves what would otherwise be a
contradiction: System Admin can reassign a stuck step but cannot open records.
Company name, region, step, assignee and age are enough to reassign and not enough
to read the case.

It matters more since there's no SSO: leaver revocation is manual, so
orphaned steps are routine rather than rare.

**`/admin/break-glass` — the support escape hatch.** When a user reports "my record
won't load", System Admin cannot reproduce it without record access. The choice was
standing read access (they then browse commercial pricing at will), nothing (they
cannot support the system they run), or **time-boxed audited access to one
record** — which is what this is.

### What is deliberately not a screen here

**Upload limits, session timeout, password and lockout policy.** They read like
admin settings and they are not: the application is not the only layer enforcing
them, so a screen could change the app's opinion while nginx and the DI container
kept theirs. They live in `appsettings.json` with environment-variable overrides
([domain/master-data §11](../../domain/master-data.md#11-business-constants-and-deployment-configuration)).

**Allocation & Gas Balance tracking.** PGN already tracks realisasi/alokasi in
their own spreadsheet; this platform doesn't rebuild it. `Ketersediaan Pasokan`
on the Evaluation tab is a plain manual field instead.

> **Three of these block calculations the evaluation screens already assume are
> available:** user accounts, document templates and meter sizes. Seed them first.

> **`Organisasi` must be seeded before anything else.** Users are scoped to an Area
> and Areas belong to Regions; no user can be assigned a role until those org units
> exist.

### No Template Dokumen screen

There's no admin screen for the six Lampiran `.docx` templates, and no
`/admin/document-templates` route. They ship as files with the application,
the same way any other code asset does — a developer replaces the file,
tests that generation still works, and deploys. An admin-facing upload
screen was the original plan, but nothing validated that an uploaded
`.docx` still had the merge fields the generator expects; template-merge
fails at generation time, not upload time, so a bad upload was a landmine
for the next KK0 rather than a caught error. Version history for the
templates lives in source control instead of a `Versi`/`Diperbarui` admin
table.

---

## Pengguna — role assignment

Available to System Admin (all regions) and Regional Admin (own region, area-level
roles only).

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│  Pengguna — SOR II                        [ 🔍 Cari pengguna ]   [ + Tambah Pengguna ]│
├──────────────────────────────────────────────────────────────────────────────────────┤
│  ℹ️ Akun dikelola di dalam sistem ini. Tidak ada integrasi direktori PGN.             │
├──────────────────┬──────────────────────┬────────────────┬────────┬─────────┬───────┤
│ Nama             │ Peran                │ Lingkup        │ Status │ Login   │       │
│                  │                      │                │        │ terakhir│       │
├──────────────────┼──────────────────────┼────────────────┼────────┼─────────┼───────┤
│ Budi Santoso     │ Sales Area           │ Area Surabaya  │ Aktif  │ hari ini│ ( ⋮ ) │
│ Rina Ayu         │ Sales Area           │ Area Sidoarjo  │ Aktif  │ 2 hari  │ ( ⋮ ) │
│ Rudi Hartono     │ Area Head            │ Area Surabaya  │ Aktif  │ 1 hari  │ ( ⋮ ) │
│ Andi Pratama     │ Reviewer             │ SOR II         │ Aktif  │ 4 jam   │ ( ⋮ ) │
│ Dewi Kartika     │ Reviewer · Sales Area│ SOR II · Gresik│ Aktif  │ hari ini│ ( ⋮ ) │
│ Sinta Maharani   │ Sales Area           │ Area Gresik    │ ⚠️ Baru │ belum   │ ( ⋮ ) │
│ Hendra Lesmana   │ Reviewer             │ SOR II         │ 🔴 Nonaktif│ 94 hari│ ( ⋮ )│
└──────────────────┴──────────────────────┴────────────────┴────────┴─────────┴───────┘
   ⚠️ 1 akun belum pernah login · 🔴 1 akun tidak aktif lebih dari 90 hari
```

`( ⋮ )` → `Ubah peran` · `Atur ulang kata sandi` · `Nonaktifkan`.

**The `Login terakhir` column is a compensating control**, not a nicety. Without a
directory, nobody is automatically revoked when they leave PGN
([design/roles-permissions §5](../roles-permissions.md#movers-and-leavers)) — a dormant-account
warning is the cheapest way to make that visible. Recommend a quarterly review.

### Creating a user

```
┌────────────────────────────────────────────────────────────────────┐
│  Tambah Pengguna                                                   │
│                                                                    │
│  Nama Lengkap *    [ Sinta Maharani                            ]   │
│  Nama Pengguna *   [ sinta.maharani                            ]   │
│  Email             [ sinta.maharani@pgn.co.id                  ]   │
│                    ℹ️ Disimpan untuk keperluan nanti; email belum   │
│                       diaktifkan, tidak ada surat yang dikirim.    │
│                                                                    │
│  Peran *           [ Sales Area              ▾ ]                   │
│  Lingkup *         [ Area Gresik             ▾ ]                   │
│                                                                    │
│  Kata Sandi Sementara                                              │
│  ┌──────────────────────────────────────────────────────────────┐ │
│  │  Xk7#mQ2vT9pL                              ( 📋 Salin )       │ │
│  └──────────────────────────────────────────────────────────────┘ │
│  ⚠️ Sampaikan kata sandi ini langsung kepada pengguna. Sistem      │
│     tidak mengirimkannya. Pengguna wajib menggantinya saat login   │
│     pertama.                                                       │
│                                                                    │
│                                    ( Batal )   [ Simpan ]          │
└────────────────────────────────────────────────────────────────────┘
```

The generated password is shown **once**, on this screen only. It is stored hashed
and cannot be retrieved — an admin who loses it issues a new one.

`Atur ulang kata sandi` reuses this panel.

```
┌────────────────────────────────────────────────────────────────────┐
│  Ubah Peran — Dewi Kartika                                         │
│  dewi.kartika@pgn.co.id · dari direktori PGN                       │
│                                                                    │
│  Peran aktif                                                       │
│  ┌──────────────────────────────────────────────────────────────┐ │
│  │ Reviewer      · SOR II            ( Nonaktifkan )            │ │
│  │ Sales Area    · Area Gresik       ( Nonaktifkan )            │ │
│  └──────────────────────────────────────────────────────────────┘ │
│                                                                    │
│  Tambah peran   [ pilih peran ▾ ]  [ pilih lingkup ▾ ]  ( Tambah ) │
│                                                                    │
│  🔒 Admin Regional dan Division Head hanya dapat ditetapkan oleh    │
│     System Admin.                                                  │
│                                                                    │
│  ⚠️ Dewi memegang 5 langkah persetujuan aktif. Menonaktifkan peran  │
│     Reviewer akan menahan langkah tersebut hingga ditetapkan ulang. │
│                                                                    │
│                                    ( Batal )   [ Simpan ]          │
└────────────────────────────────────────────────────────────────────┘
```

Design points:

- **Multi-role is normal** ([roles-permissions §4](../roles-permissions.md#multi-role-users))
  — Dewi is a Reviewer at region level *and* Sales Area in Gresik. The UI shows
  roles as a list, not a single dropdown.
- **The in-flight warning is the important control.** Deactivating someone holding
  approval steps is the single most common way a workflow silently stalls; say so
  before it happens, and route the orphaned steps to *Tugas Tertahan*.
- **Escalation guard rails** are stated inline: Regional Admin cannot appoint
  Regional Admins or Division Heads, and nobody may edit their own assignments.
- Deactivating a role never rewrites history — past `status_event` rows keep the
  original attribution.

---

## No Alokasi & Gas Balance screen

PGN already tracks realisasi/alokasi BBTUD and quarterly gas-balance figures
in their own spreadsheet (`Data Plotting!C30:F33`, with real formulas). This
platform doesn't rebuild that tracking — it's document/workflow management,
not a capacity-planning tool. The one place the workflow needs a supply
figure — Resume Evaluasi §5 — is a
plain manual field on the Evaluation tab, not a lookup from stored monthly or
quarterly data. See
[frontend/07 § Supply analysis](07-evaluation-and-issuance.md#supply-analysis).
