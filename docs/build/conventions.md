# Build — Conventions

> **Canonical.** This document owns coding conventions, licensing constraints,
> and secrets handling for this repo. `AGENTS.md` carries only a one-line
> digest and a link here.

## Types and nullability

- **`decimal`, never `double`, for money or volumetric values.** These print
  on signed commercial documents; a narrowing conversion is a correctness
  bug, not a style nit.
- `Directory.Build.props` promotes `CS8602`/`CS8618` (nullability) and
  `CS4014` (unawaited task) to build errors for the same reason — don't
  suppress them, fix the code.

## Package management

- **Backend (.NET):** Central package management only. Add/bump packages in `Directory.Packages.props` (`PackageVersion`); `.csproj` files carry `PackageReference` with no version attribute.
- **Frontend (Node/Bun):** Prefer **Bun** (or `pnpm` if bun is not suitable) over `npm`/`npx`. Use `bun install`, `bun add <pkg>`, and `bunx <pkg>` instead of `npx <pkg>`. Commit `bun.lock`. Never use `npm`.

## Licensing

Licensing is load-bearing, not a preference:

- Excel export uses **ClosedXML** (MIT) — never EPPlus (non-commercial
  license since v5, and PGN is a commercial SOE).

## Secrets

Secrets never committed, never baked into an image. Real credentials go in
`appsettings.Local.json` (gitignored **and** dockerignored — check both when
adding a new local-only file) or `dotnet user-secrets`. Commit
`appsettings.Local.example.json` alongside any new key you add there.

## Domain vocabulary

Indonesian domain vocabulary stays verbatim in code, data, and UI text
(field names, status names, role names) — it maps 1:1 onto the paper forms
PGN audits against. Routes/URLs are English; UI language is Indonesian.

## DTO and Data Model Naming Taxonomy

To keep data transfer objects and read projections intent-revealing and DDD-aligned across `Simando.Application`, follow this role-based naming taxonomy instead of applying a blanket `Dto` suffix to all records:

| Role | Suffix / Pattern | Examples | Description |
| :--- | :--- | :--- | :--- |
| **Top-Level Payloads** | **`Dto`** | `ApproverDashboardDto`, `GasDemandReportDto`, `SurveyProductivityReportDto` | Container objects returned as top-level responses by Application services (`IDashboardService`, `IReportsService`). |
| **Grid / Collection Rows** | **`Item`** or **`Row`** | `PendingApprovalItem`, `StuckTaskItem`, `CompanyListItem`, `SurveyProductivityRow` | Line items inside collection properties (consumed by `@tanstack/react-table`). Signals that the type is a single line item, not an aggregate response container. |
| **Read Projections & Views** | **`Detail`** or **`Summary`** | `CompanyDetail`, `SurveyDetail`, `PlottingDetail`, `ContactSummary` | Aggregated read models or compact projections for record views and cards. |
| **Command / Query Inputs** | **`Request`** or **`Filter`** | `CreateCompanyRequest`, `CompanyListFilter` | Inbound payload parameters sent from forms and API requests. |
| **Operation Outcomes** | **`Result`** | `StageEditResult`, `SubmitResult`, `WorkflowActResult` | Result objects carrying status, validation error messages, or outcome payloads. |

**Rule for Frontend Tables & Forms:** When typing TanStack Table rows or TanStack Form schemas, use the generated OpenAPI types corresponding to these DTOs (`schema.d.ts`), maintaining consistent nomenclature across both tiers.

## Git commit messages

- Use Conventional Commits format (`type(scope): concise description` or `type: concise description`).
- **Single-line subject line only.** Do not write body text or multi-paragraph descriptions in commit messages. Keep commit messages brief, clear, and on a single line matching the repository's commit style.
- Commit only when explicitly requested.

## Comments

Comments explain non-obvious "why," never "what." Straightforward code gets
no comment — the code already says what it does. Write one only for a
special case a glance wouldn't reveal: a framework gotcha, a doc-sourced
business rule, a rejected alternative and why. Never narrate implementation
or debugging history ("this used to X, then broke when Y, so now Z") — that
belongs in a doc or task log, not living in the file. If the
rationale is more than a few lines or applies to more than one file, it
belongs in `docs/`, with the code carrying a one-line pointer to it.

## Frontend UI Components (shadcn/ui), Tailwind CSS v4, Lucide Icons, and mapcn

- **UI Components:** Use official `shadcn/ui` components (Card, Button, Dialog, DropdownMenu, Table, Input, Select, etc.) rather than raw unstyled HTML elements.
- **Tailwind CSS v4 & OKLCH Theming:** Global theme variables reside in CSS using OKLCH color space for light and dark modes, preserving PGN Corporate Blue (`--primary: oklch(0.352 0.165 259.7)`).
- **Icons:** Use `lucide-react` for all application iconography.
- **Geospatial Maps:** Use `mapcn` (`@mapcn/map`, powered by MapLibre GL) installed via `bunx --bun shadcn@latest add @mapcn/map` for map visualization, pin-drop coordinate selection, and boundary views.
- **Forms & Validation:** Use `@tanstack/react-form` combined with `zod` for type-safe validation schemas matching backend contracts.
- **API Fetching & Caching:** Use `$api.useQuery` and `$api.useMutation` from `openapi-react-query` generated from `/openapi/v1.json`.
