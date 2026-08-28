# DMS - Simando

**Sistem Manajemen Proses Berlangganan Gas** — design documentation for the web
system that digitises PGN's gas-subscription sales pipeline: from an industrial
prospect in a directory, through survey and pricing, to the issuance of a
**NOL (No Objection Letter)**.

**Stack:** .NET 10 (LTS) · ASP.NET Core 10 Web API · OpenAPI 3.1 + Scalar · React 19 SPA (Bun + Vite + TanStack Suite + shadcn/ui + mapcn) · EF Core 10 · PostgreSQL 18 + PostGIS · S3-compatible storage · self-hosted.

---

## Start here

| If you are… | Read |
|---|---|
| **A PGN stakeholder** | [stakeholder-brief.md](stakeholder-brief.md) — 2 pages, Indonesian, what we need from you |
| **New to the project** | [domain/01-overview.md](domain/01-overview.md) → [domain/02-process-flow.md](domain/02-process-flow.md) |
| **Building the backend** | [design/data-model.md](design/data-model.md) → [build/architecture.md](build/architecture.md) |
| **Building the frontend** | [design/frontend/13-page-role-matrix.md](design/frontend/13-page-role-matrix.md) → [design/frontend/](design/frontend/README.md) |
| **What's not built yet** | [future/](future/README.md) — features the client raised but didn't confirm |
| **Verifying a claim** | [source/](source/README.md) — every citation resolves here |

---

## Layout

```
docs/
├── stakeholder-brief.md      for PGN — decisions needed, in Indonesian
├── notulen.txt               client meeting notes (raw)
│
├── source/                   ARCHIVED SOURCE MATERIAL
│   ├── worksheet.xlsx        the client's field spec
│   ├── form-output-data.docx annotated procedure screenshots
│   ├── procedure-pages/      17 pages of O-001/06.02, named by page
│   │   └── annotated/        the 8 pages with client notes, overlaid
│   └── extracts/             all cells, 54 comments, 9 formulas, 19 annotations
│
├── domain/                   WHAT THE BUSINESS DOES
│   ├── 01-overview.md         problem, actors, scope, glossary
│   ├── 02-process-flow.md     official flow vs. 8 stages, state machine, gates
│   ├── 03-directory-plotting.md
│   ├── 04-prospect-survey.md  KK0 + the equipment table
│   ├── 05-a1-registration.md
│   ├── 06-nol.md              request, evaluation, issuance
│   └── master-data.md         16-item configuration inventory + seeding order
│
├── design/                    HOW WE BUILD IT
│   ├── approval-workflow.md   ← canonical: the chain and its transitions
│   ├── data-model.md          ← canonical: entities and fields
│   ├── roles-permissions.md   ← canonical: scope, capability, turn
│   ├── reporting.md           map, dashboard, metrics
│   └── frontend/               14 docs — every screen, with layouts
│
├── build/                     HOW WE SHIP IT
│   ├── architecture.md        stack, modules, deployment, security
│   ├── web-conventions.md     ← canonical: REST API, OpenAPI, and frontend conventions
│   ├── testing.md             fixtures for the riskiest calculations
│   └── storage.md             S3 / OneDrive abstraction and migration
│
└── future/                   NOT PLANNED FOR v1
    └── README.md              six features raised but never confirmed, and
                                 the v1 seam each one leaves behind
```

---

## Source material

Three sources, all reconciled here and all archived in [`source/`](source/README.md).

| Source | What it is | What it gave us |
|---|---|---|
| `worksheet.xlsx` | Client's functional spec | Sheet `Entry Apps` defines every field tagged with its pipeline stage (1–8). Six further sheets define screens, master data, the survey form and segments. **54 cell comments** carry input-type hints; **9 formulas** carry the real calculations. |
| `form-output-data.docx` | Annotated screenshots | 17 pages of PGN's **official procedure** `O-001/06.02 Rev.01` (*Prosedur Operasi Berlangganan Gas*, eff. 1 April 2023, 211 pages), plus **19 floating text-box annotations drawn over them**. Position is half their meaning — see [`extracts/annotations.txt`](source/extracts/annotations.txt). |
| [`notulen.txt`](notulen.txt) | Client meeting notes | Problem statement, role hierarchy, configurable reviewer chain, visibility rules, and the three status transitions. **Most authoritative source on workflow**; overrides the docx annotations where they conflict. |

The official pages captured in the docx are the authoritative **form** definitions:

| Page | Content | |
|---|---|---|
| 66 | **Diagram Alir 6.1** — the official swimlane process flow | |
| 160 | **Lampiran 10** — Formulir KK0 (survey) | `FORM 1` |
| 161–162 | **Lampiran 11** — Formulir Registrasi Berlangganan Gas (the A1) | `FORM 2` |
| **165** | **Daftar Peralatan Gas** — official energy-conversion references | |
| 169–174 | **Lampiran 15/16** — Permohonan & Penerbitan NOL/RL | |
| 175–182 | **Lampiran 17** — Evaluasi Registrasi Berlangganan Gas | `FORM 3` |
| 185–186 | **Resume Evaluasi / Analisis** | |

The three `FORM n` labels are the client's own, drawn on the pages. They mark the
three **data-entry forms**; everything unlabelled is generated output
([why that matters](source/README.md#what-the-shape-annotations-settle)).

> ⚠️ **We have 17 screenshots of a 211-page procedure, and never the document
> itself.** Every Lampiran specification here derives from those images. The file
> is named `Final-PO Berlangganan Gas 2023_ o.pdf`; requesting it is item 5 on the
> [stakeholder brief](stakeholder-brief.md).

---

## Status

[future/README.md](future/README.md) lists six features the client has raised
but never confirmed as requirements — none block v1, each has a cheap seam
already in the schema.

Three of twelve seeding items are blocked on PGN input, and none of them
block a core calculation anymore — see
[domain/master-data.md §13](domain/master-data.md#13-seeding-checklist).

---

## Conventions

**Language.** Narrative in English; every domain term, field label and status name
kept **verbatim in Indonesian** so the docs map 1:1 onto the forms. The shipped UI
is Indonesian; **routes are English**
([why](design/frontend/01-shell-and-navigation.md#route-design)). The stakeholder
brief is entirely in Indonesian.

**Traceability.** Claims cite their origin — `Entry Apps!D93`, `Lampiran 17 §6` —
and resolve against [`source/extracts/`](source/README.md). If a statement has no
citation and no `[ASSUMPTION]` marker, treat that as a gap worth challenging.

**Canonical sources.** Several topics recur across many documents. To stop them
drifting, one document owns each; elsewhere they are summarised briefly and linked,
never redefined:

| Topic | Canonical |
|---|---|
| Approval chain, transitions | [design/approval-workflow.md](design/approval-workflow.md) |
| Entities and fields | [design/data-model.md](design/data-model.md) |
| Scope, capability, turn | [design/roles-permissions.md](design/roles-permissions.md) |
| Configuration inventory | [domain/master-data.md](domain/master-data.md) |
| REST API and frontend conventions | [build/web-conventions.md](build/web-conventions.md) |
| Features not yet planned | [future/README.md](future/README.md) |

If you change one of these, grep for references before assuming you are done.

**Markers.** `[ASSUMPTION]` = inferred, not sourced. `🚧` = blocked on PGN
input or deferred — see the nearby note, or [future/](future/README.md) for
deferred features. `⚠️` = a known problem in the source data.

**Diagrams.** Mermaid — renders natively in GitHub, GitLab, VS Code and Obsidian.
