# Frontend — Pages & Layout

Screen-by-screen specification for **DMS - Simando**. Rough layouts, not visual
design: structure, content, controls, and which roles see what.

Read [roles-permissions.md](../roles-permissions.md) first — every
page here assumes that model.

## Documents

| # | Document | Covers |
|---|---|---|
| 01 | [Shell & navigation](01-shell-and-navigation.md) | App frame, sidebar per role, header, route table |
| 02 | [Dashboard](02-dashboard.md) | Role-aware landing pages |
| 03 | [Directory, Plotting & Map](03-directory-plotting-map.md) | Stages 1–2 lists, map, company form |
| 04 | [Record hub](04-record-hub.md) | **The central page** — one record, all stages, timeline |
| 05 | [Prospect & Survey (KK0)](05-prospect-and-survey.md) | Stages 3–4 forms, equipment table |
| 06 | [A1 & NOL request](06-a1-and-nol-request.md) | Stages 5–6, pricing, document generation, signing |
| 07 | [Evaluation & issuance](07-evaluation-and-issuance.md) | Stages 7–8, Resume Evaluasi, NOL/RL |
| 08 | [Tasks & approvals](08-tasks-and-approvals.md) | Inbox, approve/revise/reject, reassignment |
| 09 | [Reports](09-reports.md) | Funnel, ageing, exports |
| 10 | [Admin](10-admin.md) | Master data, users, documents |
| 11 | [Component library](11-components.md) | Shared building blocks |
| 12 | [Patterns & states](12-patterns-and-states.md) | Loading, empty, error, validation, concurrency, responsive |
| 13 | [**Page × role matrix**](13-page-role-matrix.md) | **Start here for orientation** — who reaches what, and which pages differ by role |
| 14 | [Role navigation guide](14-role-navigation-guide.md) | Role-first companion to 13 — sidebar menu, pages, and what each role does on them |

## Conventions

**Language.** The UI is **Indonesian**. Wireframes use the Indonesian labels the
product will ship with; prose around them is English. Field names come from the
source forms verbatim — do not "improve" them, they are legally prescribed
([01-overview](../../domain/01-overview.md#key-design-implications)).

**Density.** This is an internal line-of-business tool used all day by people who
know the domain. Favour compact tables and dense forms over generous whitespace.
The KK0 form has ~60 fields; a wizard that shows six at a time would be slower,
not friendlier.

**Desktop-first.** All roles work at desks. The one exception is pin-drop and
survey capture, which may happen on a tablet in the field — see
[12-patterns-and-states](12-patterns-and-states.md#responsive).

**Blazor render modes.** Default `InteractiveServer`
([architecture](../../build/architecture.md#why-blazor-server-rather-than-webassembly)).
Static SSR is fine for read-only report pages; note per-page where it differs.

**Wireframe notation.**

```
[ Button ]      primary action        [Tab]        selected tab
( Button )      secondary action      ▾            dropdown
[ ✓ ] / ( ○ )   checkbox / radio      ⏱            ageing indicator
▓▓░░            progress              ⚠️ 🔴 🟡 🟢   status severity
{{ }}           computed / read-only  …            truncated content
```

## Page inventory

Routes are **English**; the UI is Indonesian. See
[01-shell-and-navigation](01-shell-and-navigation.md#route-design) for why, and
for the full route table with render modes.

| Route | Page | Primary roles |
|---|---|---|
| `/` | Dashboard | all |
| `/directory` | Directory list + map | SA, RA |
| `/directory/new` | Create company | SA, RA |
| `/plotting` | Plotting list + map | SA, RA |
| `/map` | Full-screen map | all |
| `/companies/{id}` | **Record hub** | all |
| `/companies/{id}/plotting` | Plotting tab | SA, RA |
| `/companies/{id}/prospect` | Contacts | SA, RA |
| `/companies/{id}/survey` | KK0 survey | SA, RA |
| `/companies/{id}/a1` | A1 registration | SA, RA |
| `/companies/{id}/nol-request` | NOL request (stage 6) | SA, RA |
| `/companies/{id}/evaluation` | Evaluation + Resume (stage 7) | RA |
| `/companies/{id}/nol-issuance` | NOL/RL issuance (stage 8) | DH |
| `/companies/{id}/documents` | Attachments | all |
| `/tasks` | My tasks (inbox) | AH, RA, RV, DH |
| `/tasks/blocked` | Stuck steps | RA |
| `/reports` | Reports hub | all |
| `/reports/{report}` | Individual report | all |
| `/master/{entity}` | Administration, see [roles-permissions](13-page-role-matrix.md#group-e--administration) | SYS, RA |

**Keep these stable.** They appear in the email deep links that drive the approval
chain, and those emails live in inboxes indefinitely.
