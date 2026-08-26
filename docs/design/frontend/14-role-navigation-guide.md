# Frontend 14 — Role Navigation Guide

A **role-first** companion to [roles-permissions — Page × role matrix](13-page-role-matrix.md)
(page-first) and [design/roles-permissions — Roles, Permissions & User Management](../roles-permissions.md)
(capability-first). This document answers one question per role: _if I sign in
as this role, what's in my sidebar, and what do I actually do on each page?_

Six roles, one section each. Sidebar wireframes are reproduced from
[01 — Shell & navigation](01-shell-and-navigation.md#sidebar-per-role); access
badges are reused from [roles-permissions](13-page-role-matrix.md):

✅ full access · 👁 read-only · ⏱ only when the record is at their step ·
🔀 same page, different content · 🔓 only under break-glass · ❌ not in navigation

Two things every role shares, stated once here rather than six times:

- **Navigation is generated from capabilities, never hand-coded per role**
  ([01](01-shell-and-navigation.md#sidebar-per-role)). A user holding two roles
  sees the union of both sidebars ([design/roles-permissions §4](../roles-permissions.md#multi-role-users)).
- **A cell in a page table is necessary but not sufficient.** Scope, capability,
  turn and — inside the record hub — stage, all gate what's actually editable.
  See [roles-permissions §4](13-page-role-matrix.md#4-the-three-gates-in-the-ui) for the
  full resolution order.

---

## Sales Area

**Scope:** own Area · **Chain position:** creator, stages 1–6 · **Lands on:**
Dashboard → _Perlu Tindakan Anda_

The workhorse role: creates, surveys, prices and submits. Holds no approval
step, so there is no inbox — records that come back via `Revisi`/`Tolak`
surface on the dashboard instead.

```
▪ Beranda
▪ Direktori          ← create + edit companies
▪ Plotting
▪ Peta
▪ Laporan
```

| Sidebar item  | Route(s)                       | What Sales Area does here                                                                                                                                                                                                                                              |
| ------------- | ------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Beranda**   | `/`                            | _Perlu Tindakan Anda_ (records returned by Revisi/Tolak, comment shown inline) · _Pipeline Saya_ (count per stage, click to filter Directory) · _Dalam Proses Persetujuan_ (submitted records + current holder + ageing) · mini map ([02](02-dashboard.md#sales-area)) |
| **Direktori** | `/directory`, `/directory/new` | Create a company (stage 1): name, location, production type, mandatory map pin. Edit any of their Area's companies. Soft-delete, `DRAFT` only ([03](03-directory-plotting-map.md#directory-list))                                                                      |
| **Plotting**  | `/plotting`                    | Set `Plotting By`, `Posisi Pelanggan`, `Kawasan` (stage 2) — inline or via the record hub. `[ Prospek ]` unlocks once all three are filled ([03](03-directory-plotting-map.md#plotting-list))                                                                          |
| **Peta**      | `/map`                         | Full-screen map, own Area only. Can drop and move pins                                                                                                                                                                                                                 |
| **Laporan**   | `/reports/*`                   | Full ✅ on Corong Penjualan, Penuaan, Potensi Kebutuhan. Read-only (👁) on Produktivitas Survei and Hasil NOL/RL — those two are for approvers judging performance, not the rep being judged                                                                           |

**Record hub** (`/companies/{id}`, reached by clicking a Direktori/Plotting row
— not a sidebar item of its own):

| Tab                  | What Sales Area does                                                                                                                                                                                                                                                                 |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Ringkasan            | Read-only summary of everything below, plus document checklist                                                                                                                                                                                                                       |
| Plotting, Prospek    | Same as the list-level actions above, done in context                                                                                                                                                                                                                                |
| Survei               | Fills the ~60-field KK0 (see [05](05-prospect-and-survey.md)) — in practice, transcribes it at a desk from a paper form filled on-site, since there's no system access in the field ([12](12-patterns-and-states.md#responsive)). Generates the KK0 `.docx`, uploads the signed scan |
| A1                   | Registration + pricing, all typed by hand. Generates A1, uploads the signed file                                                                                                                                                                                                     |
| NOL                  | Permohonan NOL: contract volumes/pricing, Lampiran 17 narrative, attaches references                                                                                                                                                                                                 |
| Evaluasi, Penerbitan | **Hidden** — never reaches these stages                                                                                                                                                                                                                                              |
| Dokumen              | Upload and download attachments                                                                                                                                                                                                                                                      |

Action bar while `DRAFT`: `Simpan Draf` / `Ajukan untuk Persetujuan` (disabled
with the blocking reason named, if a gate is unmet). **Once submitted, every
tab above goes read-only** until the record comes back via `Revisi`
([04](04-record-hub.md#action-bar)).

---

## Area Head

**Scope:** own Area · **Chain position:** step 1, terminates at Lampiran 17 ·
**Lands on:** Dashboard → pending-approval table

Approves the Area's work and hands off to the Region. Never edits a field, at
any stage — reads and decides only.

```
▪ Beranda
▪ Tugas Saya      ③  ← badge = steps awaiting them
▪ Direktori          ← read-only
▪ Plotting           ← read-only
▪ Peta
▪ Laporan
```

| Sidebar item   | Route(s)                   | What Area Head does here                                                                                                                                                                                 |
| -------------- | -------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Beranda**    | `/`                        | Pending-approval table (the page itself) · area summary · _Kinerja Persetujuan Saya_ (own turnaround, private) · recent Area activity ([02](02-dashboard.md#approver--area-head-reviewer-division-head)) |
| **Tugas Saya** | `/tasks`, `/tasks/history` | Inbox of records at the Area Head step, own Area, sorted by wait time. `[ Tinjau ]` opens the record hub. `Riwayat Tindakan` tab is their own action history                                             |
| **Direktori**  | `/directory`               | Read-only list of their Area's companies. No `+ Tambah`, no edit                                                                                                                                         |
| **Plotting**   | `/plotting`                | Read-only list — the `Posisi Pelanggan` and assigned-rep columns Direktori's list doesn't show                                                                                                           |
| **Peta**       | `/map`                     | Read-only, own Area                                                                                                                                                                                      |
| **Laporan**    | `/reports/*`               | Full ✅ on all five reports, including Produktivitas Survei and Hasil NOL/RL                                                                                                                             |

**Record hub**, opened via `Tinjau` or a Direktori row:

| Tab                  | What Area Head does                                                                           |
| -------------------- | --------------------------------------------------------------------------------------------- |
| Ringkasan            | The tab that matters — the dozen figures a reviewer actually checks, without opening six tabs |
| Plotting … NOL       | Read-only (👁)                                                                                |
| Evaluasi, Penerbitan | **Hidden** — their chain involvement ends before stage 7                                      |
| Dokumen              | Read-only; can download every attachment                                                      |

Action bar, only when the record is at their step: `Tolak` / `Minta Revisi` /
`Setuju`. **Read access continues after handover** — an Area Head can still
open a case that's moved on to Regional Admin or later, just never act on it
again ([design/roles-permissions §2.2](../roles-permissions.md#22-area-head)).

---

## Regional Admin

**Scope:** own Region — every Area within it · **Chain position:** step 2,
owns stage 7 · **Lands on:** Dashboard → stuck tasks

The busiest role and the only one in the chain that both **edits** and
**approves**. Completes what the Area left incomplete, runs the feasibility
analysis, and owns the region's rejected/stuck queue.

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

| Sidebar item       | Route(s)                              | What Regional Admin does here                                                                                                                                                                                                                                                                                                                                                                      |
| ------------------ | ------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Beranda**        | `/`                                   | _Tugas Tertahan_ leads (every `Tolak` + orphaned step in the Region) · _Menunggu Saya_ count · Corong Region (funnel) · Penuaan summary ([02](02-dashboard.md#regional-admin))                                                                                                                                                                                                                     |
| **Tugas Saya**     | `/tasks`, `/tasks/history`            | Inbox at the Regional Admin step, whole Region. `Tinjau` → record hub with **both** edit rights and the action bar — the only role with both                                                                                                                                                                                                                                                       |
| **Tugas Tertahan** | `/tasks/blocked`                      | Every `Tolak` in the Region (`[ Kelola ]`: return to creator / fix & resubmit / reassign to another sales rep / _Hentikan_) plus _Langkah Terhambat_ — steps whose assignee is no longer active, reassigned with a reason                                                                                                                                                                          |
| **Direktori**      | `/directory`, `/directory/new`        | Same as Sales Area, region-wide: create, edit, soft-delete                                                                                                                                                                                                                                                                                                                                         |
| **Plotting**       | `/plotting`                           | Same as Sales Area, region-wide                                                                                                                                                                                                                                                                                                                                                                    |
| **Peta**           | `/map`                                | Full edit (drop/move pins), whole Region                                                                                                                                                                                                                                                                                                                                                           |
| **Evaluasi**       | `…/evaluation` (stage 7, **RA only**) | FEED checkpoint · Gate Review data (capex, pipe sizing, MRS spec, G-Size/Tekanan/Flowrate) · Supply analysis (`Ketersediaan Pasokan`, manual) · Feasibility (IRR/NPV/Payback, competitor analysis) · composes and generates the **Resume Evaluasi** `.docx` · `Tetapkan Reviewer` (picks 2–3 reviewers per case) ([approval-workflow](07-evaluation-and-issuance.md#evaluasi-tab--regional-admin)) |
| **Laporan**        | `/reports/*`                          | Full ✅ on all five, plus **PII contact export** (one of only two roles that can)                                                                                                                                                                                                                                                                                                                  |
| **Pengguna**       | `/master/users`                       | Create accounts, assign **Sales Area / Area Head / Reviewer**, reset passwords, deactivate — **own Region only**. Cannot appoint Regional Admin or Division Head, cannot touch another region                                                                                                                                                                                                      |
| **Akses Darurat**  | `/admin/break-glass`                  | Read-only — sees break-glass requests touching their region's records; notified in-app when one is granted                                                                                                                                                                                                                                                                                         |

**Reached in context, not as a sidebar item:** the 👁 grants on `Organisasi`,
`Segmen`, `G-Size` and `Spesifikasi MRS`
([roles-permissions](13-page-role-matrix.md#group-e--administration)) show up where they're
used — `Organisasi` in the Area picker on _Pengguna_, the other three as the
dropdowns on the _Evaluasi_ tab — rather than as standalone browsing screens.

**Record hub**, reached via Tugas Saya, Direktori/Plotting rows, or a company
search:

| Tab            | What Regional Admin does                                                        |
| -------------- | ------------------------------------------------------------------------------- |
| Plotting … NOL | **Editable** — completes data the Area left incomplete, audited                 |
| Evaluasi       | Same content as the standalone Evaluasi queue item above, in one case's context |
| Penerbitan     | Read-only (👁) — Division Head's tab                                            |
| Dokumen        | Upload and download                                                             |

Action bar at their step: `Tolak` / `Minta Revisi` / `Tetapkan Reviewer` /
`Setuju`.

---

## Reviewer

**Scope:** own Region · **Chain position:** steps 3–5, 2–3 per case, assigned
by Regional Admin · **Lands on:** Dashboard → pending-approval table

A per-case assignment, not a standing job. Deliberately the thinnest sidebar —
a reviewer's job is to act on what's in front of them, nothing else.

```
▪ Beranda
▪ Tugas Saya      ②
▪ Peta
▪ Laporan
```

| Sidebar item   | Route(s)                   | What Reviewer does here                                                                                                                  |
| -------------- | -------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- |
| **Beranda**    | `/`                        | Same composition as Area Head/Division Head: pending-approval table, region summary, own turnaround, recent activity                     |
| **Tugas Saya** | `/tasks`, `/tasks/history` | Inbox of records at their own reviewer step (Reviewer 1/2/3), whole Region. `Tinjau` → record hub, read-only + action bar. Adds comments |
| **Peta**       | `/map`                     | Read-only, whole Region                                                                                                                  |
| **Laporan**    | `/reports/*`               | ✅ on Corong Penjualan, Penuaan, Potensi Kebutuhan. **❌ — not even read-only** on Produktivitas Survei. 👁 on Hasil NOL/RL              |

**Record hub**, opened via `Tinjau`:

| Tab             | What Reviewer does        |
| --------------- | ------------------------- |
| Ringkasan … NOL | Read-only (👁)            |
| Evaluasi        | Read-only (👁)            |
| Penerbitan      | **Hidden**                |
| Dokumen         | Read-only; downloads only |

Action bar at their step: `Tolak` / `Minta Revisi` / `Setuju`. Cannot edit any
field — the record is read-only for **everyone** while under review, including
Regional Admin.

---

## Division Head

**Scope:** own Region · **Chain position:** final step · **Lands on:**
Dashboard → pending-approval table

The terminal decision. On the official Nota Dinas this is _Direktur Komersial
/ General Manager, Sales and Operation Region_.

```
▪ Beranda
▪ Tugas Saya      ①
▪ Peta
▪ Laporan
─────────────
▪ Akses Darurat      ← break-glass requests on own region's records, read-only
```

| Sidebar item      | Route(s)                   | What Division Head does here                                                                                                           |
| ----------------- | -------------------------- | -------------------------------------------------------------------------------------------------------------------------------------- |
| **Beranda**       | `/`                        | Same composition as Area Head/Reviewer: pending-approval table (their issuance queue), region summary, own turnaround, recent activity |
| **Tugas Saya**    | `/tasks`, `/tasks/history` | Inbox at the final step. `Tinjau` → record hub → **Penerbitan** tab                                                                    |
| **Peta**          | `/map`                     | Read-only, whole Region                                                                                                                |
| **Laporan**       | `/reports/*`               | Full ✅ on all five, plus PII contact export (the other of the two roles that can)                                                     |
| **Akses Darurat** | `/admin/break-glass`       | Read-only — break-glass requests touching their region's records; notified in-app when one is granted, same as Regional Admin          |

**Record hub**, opened via `Tinjau`:

| Tab                  | What Division Head does                                                                                                                                                                                                                                                                                                                                        |
| -------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Ringkasan … Evaluasi | Read-only (👁) — Resume Evaluasi is one click away                                                                                                                                                                                                                                                                                                             |
| **Penerbitan**       | **The only tab they edit.** Decides `Menyetujui` (issue NOL) or `Tidak menyetujui` (issue RL); sets `Isi Persetujuan` — same as requested or modified terms, stored separately from the request; attaches numbered `Kontrak Bersyarat` conditions; sets the validity period ([approval-workflow](07-evaluation-and-issuance.md#penerbitan-tab--division-head)) |
| Dokumen              | Read-only; downloads only                                                                                                                                                                                                                                                                                                                                      |

Action bar at their step: `Tolak` / `Minta Revisi` / `[ Terbitkan NOL ]` (or
`Terbitkan RL`, in red, once _Tidak menyetujui_ is selected). Issuance is
**irreversible** and confirmed explicitly — it generates the signed Lampiran
16 and locks the record for everyone but downloads.

**Reached in context, not as a sidebar item:** the 👁 grant on `Segmen`
([roles-permissions](13-page-role-matrix.md#group-e--administration)) doesn't get a nav
entry — the segment name already appears on the record hub's Ringkasan and
Penerbitan tabs, which is all a final approver needs it for.

---

## System Admin

**Scope:** all, for platform data · none, for case data · **Chain position:**
none — never appears in an approval chain · **Lands on:** master-data health
panel

A _platform_ administrator, not a super user. Owns configuration and accounts;
deliberately excluded from every commercial record except through an audited,
time-boxed break-glass request.

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

| Sidebar item          | Route(s)                                                   | What System Admin does here                                                                                                                                                                                                                 |
| --------------------- | ---------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **(landing page)**    | `/`                                                        | No case dashboard — master-data health instead: _Perlu Perhatian_ alerts (e.g. G-Size catalogue empty), counts (wilayah, pengguna aktif, template dokumen) ([02](02-dashboard.md#system-admin))                                             |
| **Organisasi**        | `/master/organisation`                                     | Create/manage SOR regions and Areas. **Must be seeded before any user can be assigned** — everything scopes off it                                                                                                                          |
| **Pengguna**          | `/master/users`                                            | Create any user, assign **any** role including Regional Admin/Division Head, in **any** region, reset passwords, deactivate                                                                                                                 |
| **Referensi**         | `/master/countries`, `/master/industry-types`              | Countries; industry types                                                                                                                                                                                                                   |
| **Komersial**         | `/master/segments`                                         | Six-tier segment list — name only, no pricing (pricing is typed per-record on A1/NOL)                                                                                                                                                       |
| **Energi & Konversi** | `/master/fuel-types`, `/master/units`                      | Fuel type union list; the one unit-of-measure table covering all eight unit sets                                                                                                                                                            |
| **Teknis**            | `/master/meter-sizes`, `/master/mrs-specs`                 | G-Size/meter catalogue (feeds the Evaluasi meter picker) · MRS spec catalogue                                                                                                                                                               |
| **Dokumen**           | `/master/reference-documents`, `/master/reason-categories` | Versioned _Ketentuan_ reference documents · optional reason-category grouping for Revisi/Tolak                                                                                                                                              |
| **Pemulihan**         | `/admin/stuck-steps`, `/admin/break-glass`                 | _Langkah Tertahan_: reassign a stuck step **without opening the record** — company name, step and assignee only, all regions. _Akses Darurat_: review/grant time-boxed (60 min), reason-required, heavily audited read access to one record |

Never edits a case field, never approves/rejects/revises anything — no
exception, no override, ever. Only path to record content is break-glass, and
even then it's read-only for 60 minutes.

There is deliberately **no bootstrap/seed-account screen.** The first System
Admin is seeded via a database migration or a one-off CLI command, outside
the app entirely — nobody can be authenticated to view such a page before
that account exists
([design/roles-permissions §2.6](../roles-permissions.md#bootstrapping-the-first-admin)).
Deactivating the seed account afterward is an ordinary `Pengguna` action, not
a special screen.

---

## Entry points, at a glance

Condensed from [roles-permissions §5](13-page-role-matrix.md#5-entry-points-per-role):

| Role           | Lands on                        | Shortest path to their job                                                  |
| -------------- | ------------------------------- | --------------------------------------------------------------------------- |
| Sales Area     | Dashboard → returned-work panel | Direktori → create → record hub → survey → A1 → NOL → submit                |
| Area Head      | Dashboard → pending table       | Tugas Saya → Tinjau → Setuju                                                |
| Regional Admin | Dashboard → stuck tasks         | Tugas Saya → Evaluasi tab → assign reviewers → Setuju; watch Tugas Tertahan |
| Reviewer       | Dashboard → pending table       | Tugas Saya → Tinjau → Setuju                                                |
| Division Head  | Dashboard → pending table       | Tugas Saya → Penerbitan tab → Terbitkan NOL                                 |
| System Admin   | Master-data health              | Seeding and maintenance only                                                |

## Discrepancies resolved while compiling this guide

The first draft of this document flagged four places where the sidebar
wireframes in [01](01-shell-and-navigation.md) didn't show a page the
capability grants in [roles-permissions](13-page-role-matrix.md) actually allow. Each has
since been checked against why the grant exists and resolved one of two ways:
add the missing nav item, or confirm the access is reached in context and
document that instead. Nothing was removed from either 01 or 13 as a result —
every grant checked out as intentional.

| Role           | Grant                                          | Resolution                                                                                                                                              |
| -------------- | ---------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Area Head      | 👁 `/plotting`                                 | **Added** to the sidebar — Direktori's list doesn't carry the `Posisi Pelanggan`/assigned-rep columns Plotting's does, so it wasn't redundant           |
| Regional Admin | 👁 Organisasi, Segmen, G-Size, Spesifikasi MRS | **Documented as contextual** — reached via the Area picker on Pengguna and the dropdowns on Evaluasi, not a standing browsing screen                    |
| Regional Admin | 👁 Akses Darurat                               | **Added** to the sidebar — a standalone page with no dropdown/contextual substitute, and Regional Admin is one of the two roles notified on break-glass |
| Division Head  | 👁 Segmen                                      | **Documented as contextual** — already visible on the record hub's Ringkasan/Penerbitan tabs                                                            |
| Division Head  | 👁 Akses Darurat                               | **Added** to the sidebar, same reasoning as Regional Admin — Division Head is the other role notified on break-glass                                    |
