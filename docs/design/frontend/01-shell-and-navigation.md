# Frontend 01 — Shell & Navigation

## App frame

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│ ▣ Simando        [🔍 Cari perusahaan / nomor…            ]   🔔 3   Budi S. ▾  [ID ▾] │
│                                                              Sales Area · Area Sby    │
├───────────────────┬──────────────────────────────────────────────────────────────────┤
│                   │  Beranda › Direktori › PT Indonesia 1945                          │
│  ▪ Beranda        │ ┌──────────────────────────────────────────────────────────────┐ │
│  ▪ Tugas Saya  ③  │ │                                                              │ │
│  ▪ Direktori      │ │                    page content                              │ │
│  ▪ Plotting       │ │                                                              │ │
│  ▪ Peta           │ │                                                              │ │
│  ▪ Laporan        │ │                                                              │ │
│                   │ │                                                              │ │
│  ─────────────    │ │                                                              │ │
│  ▪ Admin       ▸  │ │                                                              │ │
│                   │ └──────────────────────────────────────────────────────────────┘ │
│                   │                                                                  │
└───────────────────┴──────────────────────────────────────────────────────────────────┘
```

### Header

| Element | Behaviour |
|---|---|
| **Global search** | Searches `nama_perusahaan` and `nomor`. Scoped — a Sales Area user searching cannot surface another Area's company. Typeahead, max 8 results, grouped by stage |
| **Notification bell** | Count of unread workflow notifications. Opens a panel listing pending actions and recent transitions; each row links to the record hub. **The only notification channel while email is deferred** |
| **User menu** | Name, **role and scope** (`Sales Area · Area Surabaya`), profile, sign out |
| **Language** | `ID` default. Only include if PGN wants EN — otherwise drop the control entirely |

> **Always show the user's role and scope in the header.** In a system where five
> roles see overlapping subsets of the same records, "why can't I see this
> company?" is the most common support question. Answering it in the chrome costs
> nothing.

### Breadcrumbs

Always present on detail pages. The record hub's crumb shows the company name, not
the id — users refer to cases by company.

---

## Sidebar per role

Navigation is **generated from capabilities**, never hand-coded per role. A user
holding two roles ([roles-permissions §4](../roles-permissions.md#multi-role-users)) sees
the union.

### Sales Area

```
▪ Beranda
▪ Direktori          ← create + edit companies
▪ Plotting
▪ Peta
▪ Laporan
```

No *Tugas Saya* — Sales Area never holds an approval step. Records returned by
`Revisi` appear on their dashboard instead.

### Area Head

```
▪ Beranda
▪ Tugas Saya      ③  ← badge = steps awaiting them
▪ Direktori          ← read-only
▪ Plotting           ← read-only
▪ Peta
▪ Laporan
```

*Plotting* is read-only, same reasoning as *Direktori*: an Area Head can see
every record in their Area at every stage, including the `Posisi Pelanggan`
and assigned-rep columns that only the Plotting list shows
([design/roles-permissions §2.2](../roles-permissions.md#22-area-head)).

### Regional Admin

```
▪ Beranda
▪ Tugas Saya      ⑦
▪ Tugas Tertahan  ②  ← rejected + orphaned steps needing reassignment
▪ Direktori
▪ Plotting
▪ Peta
▪ Evaluasi        ④  ← records at stage 7 awaiting evaluation
▪ Laporan
─────────────
▪ Pengguna           ← area-level role assignment, own region
▪ Akses Darurat      ← break-glass requests on own region's records, read-only
```

The busiest sidebar, matching the busiest role. *Akses Darurat* is a
read-only peek, not master-data editing — Regional Admin is one
of the two roles notified when a break-glass request touches their region
([design/roles-permissions §2.6](../roles-permissions.md#break-glass)), so both need a
standing place to check, not just a notification that scrolls away.

**Not a separate nav item:** the read-only grants on `Organisasi`, `Segmen`,
`G-Size` and `Spesifikasi MRS` ([roles-permissions](13-page-role-matrix.md#group-e--administration))
are reached in context, not by browsing a master-data list — `Organisasi`
via the Area picker on *Pengguna*, `Segmen`/`G-Size`/`Spesifikasi MRS` via
the dropdowns on the *Evaluasi* tab. Regional Admin "owns the feasibility
analysis, so they need to see the values feeding their numbers"
([roles-permissions §2](13-page-role-matrix.md#2-the-matrix)) — that's satisfied by the
values appearing where they're used, without a standalone browsing screen.

### Reviewer

```
▪ Beranda
▪ Tugas Saya      ②
▪ Peta
▪ Laporan
```

Deliberately minimal. A reviewer's job is to act on what is in front of them.

### Division Head

```
▪ Beranda
▪ Tugas Saya      ①
▪ Peta
▪ Laporan
─────────────
▪ Akses Darurat      ← break-glass requests on own region's records, read-only
```

Same reasoning as Regional Admin's item above — Division Head is the
other role notified on break-glass. The read-only grant on `Segmen` doesn't
get a nav item: the segment name already appears in context on the record
hub's Ringkasan and Penerbitan tabs, which is all a final approver needs it
for.

### System Admin

```
▪ Organisasi          ← SOR & Area         /master/organisation
▪ Pengguna                                 /master/users

▪ Referensi ▾
    Negara                                  /master/countries
    Jenis Industri                          /master/industry-types

▪ Komersial          ▾
    Segmen                                  /master/segments

▪ Energi & Konversi  ▾
    Jenis Bahan Bakar                       /master/fuel-types
    Satuan                                  /master/units

▪ Teknis             ▾
    G-Size / Meter                          /master/meter-sizes
    Spesifikasi MRS                         /master/mrs-specs

▪ Dokumen            ▾
    Dokumen Acuan Kerja                     /master/reference-documents
    Kategori Alasan                         /master/reason-categories

▪ Pemulihan          ▾
    Langkah Tertahan (semua region)         /admin/stuck-steps
    Akses Darurat (break-glass)             /admin/break-glass
```

Grouped into six sections because a flat list of 13 entries is unusable. The
grouping is visual only — the routes stay flat (see [route design](#route-design)).

**No case data in the System Admin sidebar.** They administer the system; they do
not browse commercial records. *Langkah Tertahan* shows only company name, step and
assignee — enough to reassign, not enough to read the case. Full record access
requires an audited, time-boxed **break-glass**
([design/roles-permissions §2.6](../roles-permissions.md#26-system-admin)).

Note that **Organisasi sits outside Master Data**. SOR regions and Areas are not
reference data — they define the scope boundaries that every permission and
approval chain resolves against, so they get their own top-level entry.

---

## Badges

The count next to *Tugas Saya* is the strongest single answer to the client's
problem statement. Rules:

- Counts **only steps where it is this user's turn** — not everything visible
- Refreshes on the Blazor circuit without a page reload
- Also rendered in the browser tab title, so it is visible when the tab is in the
  background

> **With email deferred, this badge is the entire notification system.** Nothing
> pushes; a user who does not open the app learns nothing. Treat the badge, the
> bell panel and `/tasks` as load-bearing rather than convenience features.

---

## Route design

**Routes are English; the UI is Indonesian.** These serve different audiences:

- **UI labels** are read by PGN staff all day, so they are Indonesian and taken
  verbatim from the source forms.
- **Routes** are read by developers — in code, logs, config, tests and bug reports
  — so they are English, like everything else in the codebase.

Mixing `@page "/perusahaan/{id}/evaluasi"` into an otherwise English C# codebase
buys nothing; nobody types these URLs by hand.

### Rules

| Rule | |
|---|---|
| **English, lowercase, kebab-case** | `/nol-request`, not `/nolRequest` or `/NOLRequest` |
| **Plural for collections** | `/companies`, `/tasks`, `/reports` |
| **Domain acronyms stay as-is** | `a1`, `nol`, `kk0` are PGN's own document names — and *NOL* is already English (*No Objection Letter*). Translating them would be wrong, not clearer |
| **Stage tabs are child routes** | `/companies/{id}/survey` — deep-linkable, back-button friendly |
| **Query strings in English too** | `?step=`, `?stage=`, `?province=`, `?period=` |
| **Never renamed** | They live in sent email indefinitely |

### One naming collision, resolved

**"Region" is ambiguous in this domain**, and the routes have to disambiguate it:

| Meaning | Route |
|---|---|
| **SOR I–IV** — the organisational unit that scopes visibility and approvals | `/master/organisation` |
| **Provinsi / Kota-Kabupaten / Kecamatan / Kelurahan-Desa** — administrative geography | *(seeded, no admin route — see [master-data.md §4](../../domain/master-data.md#4-administrative-geography))* |

`/master/regions` would have been genuinely unclear to anyone joining the project.
The same care applies to "Area", which is an org unit here and never a synonym for
a geographic district.

### `/master/*`, not `/admin/*`

Group E's route prefix is `/master/*`, not `/admin/*`. Several of these
screens — Segmen, Dokumen Acuan Kerja, System Constants, Users, Workflow,
Audit Log — grant read-only or partial access to non-System-Admin roles
(RA, DH), so a prefix implying System-Admin-exclusivity was misleading for
the whole group, even the screens that happen to be SYS-only today.

`/admin/*` is kept for Group C's two recovery tools, `/admin/stuck-steps`
and `/admin/break-glass` — grouped with workflow recovery rather than master
data. (`/admin/break-glass` also grants RA/DH a read-only peek, which is the
same shape of exception `/master/*` exists to avoid; it's kept `/admin/*`
here because it's a Group C tool, not Group E master data. Revisit if that
inconsistency starts to matter.)

---

## Route table

### Authentication

| Route | Page | Render mode | Notes |
|---|---|---|---|
| `/sign-in` | **Email + password form** | Interactive | Local accounts — no SSO |
| `/change-password` | Change password | Interactive | **Forced** when `must_change_password` |
| `/sign-out` | Sign out | Static | |
| `/access-denied` | Out-of-scope notice | Static | Explains scope rather than showing a blank page |

There is **no `/forgot-password`**. Self-service reset needs email, which is
deferred, so the sign-in page directs users to their administrator instead of
offering a link that cannot work.

```
┌────────────────────────────────────────────┐
│              ▣  Simando                    │
│                                            │
│  Nama Pengguna  [                      ]   │
│  Kata Sandi     [                      ]   │
│                                            │
│                     [    Masuk    ]        │
│                                            │
│  Lupa kata sandi? Hubungi administrator    │
│  Anda untuk pengaturan ulang.              │
└────────────────────────────────────────────┘
```

First sign-in — and every sign-in after an admin reset — lands on
`/change-password` before anything else is reachable.

### Records

| Route | Page | Render mode | Notes |
|---|---|---|---|
| `/` | Dashboard | Interactive | Role-aware ([02](02-dashboard.md)) |
| `/directory` | Directory list + map | Interactive | Stage 1 view |
| `/directory/new` | Create company | Interactive | |
| `/plotting` | Plotting list + map | Interactive | Stage 2 view |
| `/map` | Full-screen map | Interactive | JS interop |
| `/companies` | — | — | **Redirects to `/directory`** for anyone who guesses |
| `/companies/{id}` | Record hub — Summary | Interactive | ([04](04-record-hub.md)) |
| `/companies/{id}/plotting` | Plotting tab | Interactive | |
| `/companies/{id}/prospect` | Contacts | Interactive | |
| `/companies/{id}/survey` | KK0 survey | Interactive | |
| `/companies/{id}/a1` | A1 registration | Interactive | |
| `/companies/{id}/nol-request` | NOL request | Interactive | Stage 6 |
| `/companies/{id}/evaluation` | Evaluation + Resume | Interactive | Stage 7 · RA only |
| `/companies/{id}/nol-issuance` | NOL/RL issuance | Interactive | Stage 8 · DH only |
| `/companies/{id}/documents` | Attachments | Interactive | |

`/directory` and `/plotting` are **stage views over the same company records**,
while `/companies/{id}` is the record itself — which is why the hub is not nested
under `/directory`. It spans all eight stages and belongs to none of them.

### Workflow

| Route | Page | Render mode | Notes |
|---|---|---|---|
| `/tasks` | My tasks (inbox) | Interactive | Only steps where it is the user's turn |
| `/tasks/blocked` | Stuck steps | Interactive | RA only — rejected + orphaned |
| `/tasks/history` | My action history | Interactive | |
| `/admin/stuck-steps` | Stuck steps, all regions, minimal context | Interactive | SYS only. Grouped here, not under `/master/*` — see *"/master/\*, not /admin/\*"* above |
| `/admin/break-glass` | Break-glass requests & log | Interactive | SYS full access; **RA and DH read-only** — notified whenever a request touches their region |

### Reports

| Route | Page | Render mode |
|---|---|---|
| `/reports` | Hub | Static SSR |
| `/reports/funnel` | Corong Penjualan | Interactive |
| `/reports/ageing` | Penuaan | Interactive |
| `/reports/gas-demand` | Potensi Kebutuhan | Interactive |
| `/reports/survey-productivity` | Produktivitas Survei | Interactive |
| `/reports/nol-outcomes` | Hasil NOL / RL | Interactive |

No `/allocation` or `/reports/allocation` — PGN tracks realisasi/alokasi in
their own spreadsheet; this platform doesn't rebuild that tracking.

### Administration

Roles column shows who has **any** access; 👁 marks read-only where it isn't
the whole story — full detail in [roles-permissions](13-page-role-matrix.md#group-e--administration).

| Route | Page | Roles |
|---|---|---|
| `/master/organisation` | SOR regions & Areas | SYS, RA 👁 |
| `/master/users` | Role assignment | SYS, RA |
| `/master/countries` | Negara | SYS |
| `/master/industry-types` | Jenis Industri | SYS |
| `/master/segments` | Segments | SYS, RA 👁, DH 👁 |
| `/master/fuel-types` | Jenis Bahan Bakar | SYS |
| `/master/units` | Satuan (units of measure) | SYS |
| `/master/meter-sizes` | G-Size / meter catalogue | SYS, RA 👁 |
| `/master/mrs-specs` | Spesifikasi MRS | SYS, RA 👁 |
| `/master/reference-documents` | Dokumen Acuan Kerja | SYS, all case roles 👁 |
| `/master/reason-categories` | Kategori alasan revisi/tolak | SYS, RA 👁 |

Twelve screens. No `/admin/exchange-rates`, `/admin/equipment-types`,
`/admin/gas-usage-types` or `/admin/notification-templates` — none of those
four are master data. No `/admin/document-templates` or
`/admin/document-numbering` either — the six Lampiran `.docx` templates are
developer-managed files, and the numbering formats are code constants,
neither an admin screen. No `/admin/workflow` either — reviewers are chosen
per case on the record itself, not pre-configured. No
`/master/system-constants` either — its only rows were allocation constants,
and that computation isn't rebuilt here. The full inventory with sources and
ownership is in
[master-data.md §2](../../domain/master-data.md#2-complete-inventory).

There is deliberately **no route for upload limits, session timeout or password
policy** — those are deployment configuration, not screens.

Flat rather than `/admin/master-data/{entity}` — each segment is unambiguous on
its own, and the sidebar can still group them visually without the URL repeating
the grouping.

### Query parameters

| Parameter | Used on | Example |
|---|---|---|
| `step` | Record hub | `?step=8f2a…` — deep link target from approval email |
| `stage` | Directory, map | `?stage=4` |
| `province`, `regency`, `district`, `village` | Lists, map, reports | `?province=35&regency=78` |
| `industry`, `position`, `zone` | Lists, map | `?position=jalur-existing` |
| `area`, `period` | Reports | `?period=2026-08` |
| `q`, `page`, `sort` | Lists | `?q=indah&sort=-updated` |

Filter state living in the query string is what makes a filtered view shareable
and the back button behave.

---

## Deep links

*(Dormant while email is deferred — no messages are sent. Retained because routes
must stay stable so links sent later remain valid.)*

Approval notifications link to `/companies/{id}?step={stepId}`. The link lands on
the **login page and redirects after authentication** — never a bypass token.

If the step has already been actioned by the time the recipient clicks, the record
hub opens with a neutral banner:

```
ℹ️  Langkah ini sudah diselesaikan oleh Sari W. pada 21/08/2026 09:14.
```

Not an error. Someone acting on a stale email has done nothing wrong.

**Keep these routes stable.** They live in sent mail indefinitely.
