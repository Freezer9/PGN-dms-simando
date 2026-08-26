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

Central package management only. Add/bump packages in
`Directory.Packages.props` (`PackageVersion`); `.csproj` files carry
`PackageReference` with no version attribute.

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
| **Grid / Collection Rows** | **`Item`** or **`Row`** | `PendingApprovalItem`, `StuckTaskItem`, `CompanyListItem`, `SurveyProductivityRow` | Line items inside collection properties (e.g., `BbDataTable TData="PendingApprovalItem"`). Signals that the type is a single line item, not an aggregate response container. |
| **Read Projections & Views** | **`Detail`** or **`Summary`** | `CompanyDetail`, `SurveyDetail`, `PlottingDetail`, `ContactSummary` | Aggregated read models or compact projections for record views and cards. |
| **Command / Query Inputs** | **`Request`** or **`Filter`** | `CreateCompanyRequest`, `CompanyListFilter` | Inbound payload parameters sent from forms and UI interactions. |
| **Operation Outcomes** | **`Result`** | `StageEditResult`, `SubmitResult`, `WorkflowActResult` | Result objects carrying status, validation error messages, or outcome payloads. |

**Rule for Blazor Pages:** When binding table components (`BbDataTable TData="..."`), use the collection line item type (e.g., `PendingApprovalItem` or `SurveyProductivityRow`) rather than assuming a `*Dto` suffix.

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
belongs in `docs/`, with the code carrying a one-line pointer to it (see
[docs/build/web-conventions.md](web-conventions.md) and its use in
`AccountController.cs` for the pattern).

## BlazorBlueprint components, OKLCH theming, MCP usage

- Always use official `BlazorBlueprint` components (`BbCard`,
  `BbCardHeader`, `BbCardTitle`, `BbCardDescription`, `BbCardContent`,
  `BbCardFooter`, `BbEmpty`, `BbAlert`, `BbSidebarHeaderContent`,
  `BbSidebarHeaderInfo`, `BbSidebarMenuChevron`, etc.) rather than raw
  unstyled HTML (`<h3>`, raw `<div>` flex wrappers, or raw `<LucideIcon>`
  chevrons) when building UI screens.
- **Consult the MCP tool server first:** before building or refactoring UI
  components, query the registered `blazorblueprint` MCP tool server
  (`get_component`, `get_setup`, `search_components`, `validate_icon`) to
  verify required sub-components, parameter names, valid Lucide icon names,
  slot structures, and icon-mode behaviors.
- **OKLCH theme system:** theme color tokens belong in
  `wwwroot/styles/theme.css` using pure `oklch(...)` color space for light
  (`:root`) and dark (`.dark`) modes (no `data-base-color` HTML attributes
  required), preserving PGN Corporate Blue
  (`--primary: oklch(0.352 0.165 259.7)`).
- **Tailwind CSS build artifact:** `wwwroot/app.css` is automatically generated from Tailwind on build by `scripts/js.sh` and is ignored by git. Never hand-edit `wwwroot/app.css`.
