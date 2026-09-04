# AGENTS.md - DMS Simando

Guidance for coding agents working in this repository. Read this before touching
code; it tells you where the real spec lives and which rules are load-bearing
rather than stylistic.

## What this is

**SIMANDO (Sistem Manajemen Dokumen)** digitises PGN's gas-subscription sales
pipeline: from an industrial prospect in a directory, through survey and pricing,
to the issuance of a **NOL (No Objection Letter)** or **RL (Surat Kesiapan Pasokan Gas - Tidak Layak)**.
Client is an Indonesian state-owned enterprise (SOE); the system backs signed
commercial documents, so correctness and auditability outrank throughput.

**Repo status: implemented and operational.** The core architecture, domain
entities, workflow engine, background jobs, document generation, and React 19 SPA
are implemented across all 10 planned epics. All backend test suites (Domain,
Application, Integration with Testcontainers, E2E) and frontend test suites
(Vitest, Playwright) are passing. `docs/` remains the canonical architectural
spec of record. When modifying or extending functionality, consult `docs/` first
to ensure alignment with domain and regulatory requirements.

## Start here

| Task                        | Read first                                                                                                                                                    |
| --------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Any new session             | [docs/README.md](docs/README.md) - the full map of `docs/`                                                                                                    |
| Backend / entities          | [docs/design/data-model.md](docs/design/data-model.md), [docs/build/architecture.md](docs/build/architecture.md)                                              |
| Workflow / approvals        | [docs/design/approval-workflow.md](docs/design/approval-workflow.md)                                                                                          |
| Frontend / screens          | [docs/design/frontend/13-page-role-matrix.md](docs/design/frontend/13-page-role-matrix.md) -> [docs/design/frontend/README.md](docs/design/frontend/README.md) |
| Before writing tests        | [docs/build/testing.md](docs/build/testing.md)                                                                                                                |
| Storage / attachments       | [docs/build/storage.md](docs/build/storage.md)                                                                                                                |
| API & frontend conventions  | [docs/build/web-conventions.md](docs/build/web-conventions.md)                                                                                                |
| "Is this in scope for v1?"  | [docs/future/README.md](docs/future/README.md) - six features raised but not confirmed                                                                        |

**Canonical-source rule** (inherited from `docs/`): each topic below is owned by
one document. Elsewhere it's summarised and linked, never redefined. If you
change one of these, grep the repo for references before assuming you're done.

| Topic                                               | Canonical doc                      |
| --------------------------------------------------- | ---------------------------------- |
| Approval chain, transitions, notification behaviour | `docs/design/approval-workflow.md` |
| Entities and fields                                 | `docs/design/data-model.md`        |
| Scope, capability, turn (RBAC)                      | `docs/design/roles-permissions.md` |
| Master/config data inventory                        | `docs/domain/master-data.md`       |
| Features not yet planned                            | `docs/future/README.md`            |
| Storage abstraction                                 | `docs/build/storage.md`            |
| API endpoints and frontend conventions              | `docs/build/web-conventions.md`    |
| Coding conventions, licensing, secrets              | `docs/build/conventions.md`        |

## Stack

- **Backend Runtime & Framework:** .NET 10 (LTS) · ASP.NET Core 10 Web API
- **API Spec & Docs:** OpenAPI 3.1 (`Microsoft.AspNetCore.OpenApi`) + Scalar UI (`Scalar.AspNetCore` at `/scalar/v1`)
- **Database & ORM:** EF Core 10 + Npgsql · **PostgreSQL 18 + PostGIS** (NetTopologySuite geography Point)
- **Object Mapping & Projections:** **Mapster v10** (`Mapster` + `Mapster.DependencyInjection`, assembly-scanned `IRegister`, `ProjectToType<TDto>()`)
- **Auth & RBAC:** ASP.NET Core Identity (SameSite=Lax Cookie Auth, Scoped `ICurrentUser` driving EF Core RLS query filters)
- **Background Jobs & Logging:** Hangfire · FluentValidation · Serilog
- **Frontend Runtime & Bundler:** **Bun** (strictly prefer `bun` / `bunx --bun` over `npm`/`npx`) + Vite 8
- **Frontend UI Framework:** **React 19** + TypeScript
- **Frontend Routing:** `@tanstack/react-router` (type-safe file-based routes & search params)
- **API Client & Caching:** `openapi-typescript` + `openapi-fetch` + `openapi-react-query` + `@tanstack/react-query` v5
- **Data Tables & Forms:** `@tanstack/react-table` v8 + `@tanstack/react-form` + `zod`
- **UI Components & Styling:** `shadcn/ui` (Tailwind CSS v4 + Radix UI) + `lucide-react`
- **Geospatial Mapping:** `mapcn` (powered by MapLibre GL)
- **Testing:** xUnit + Testcontainers (Backend: Domain, Application, Integration, E2E) · Vitest + Playwright (Frontend)

## Solution layout

```text
src/
  Simando.Domain/           entities, enums, value objects, pure workflow state machine
                             (NO EF Core dependency, deliberately)
  Simando.Application/      use cases, DTOs, validators, service interfaces, Mapster configs
  Simando.Infrastructure/   EF Core DbContext, migrations, RLS filters, storage (S3/RustFS),
                             OpenXML docx template generation, Identity, Hangfire
  Simando.Api/              ASP.NET Core 10 Web API, REST controllers, OpenAPI 3.1, auth, Scalar UI
frontend/
  src/
    api/                    schema.d.ts (generated via codegen), client.ts ($api openapi-react-query)
    components/             shadcn/ui components, mapcn map, layout shell, shared widgets,
                             stage-gate forms (KK0, A1, Permohonan, Evaluasi, Penerbitan)
    routes/                 TanStack Router file-based routes (_auth, directory, tasks, reports, master, login)
    hooks/                  custom react hooks (use-mobile, etc.)
    lib/                    utilities, zod validation schemas, auth context, role formatting, stage gates
tests/
  Simando.Domain.Tests/         pure, fast, no I/O (120 tests)
  Simando.Application.Tests/    service & validation tests (16 tests)
  Simando.Integration.Tests/    Testcontainers: real PostGIS + S3 store + WebApplicationFactory (160 tests)
  Simando.E2E.Tests/            end-to-end API scenario tests (2 tests)
  frontend/src/**/*.test.tsx    Vitest component and hook unit tests (103 tests across 19 files)
```

Each of `Domain`/`Application`/`Infrastructure` is organised into feature
folders that mirror the module list in
[docs/build/architecture.md § Module breakdown](docs/build/architecture.md#module-breakdown)
(`Companies/`, `Survey/`, `Nol/`, `Workflow/`, `Reports/`, `Admin/`).
`Simando.Domain` must stay free of EF Core (or any infrastructure) references.
The workflow transition rules are pure functions that run and are demonstrated
without a database.

## Build, run, test

```bash
# Backend (.NET 10 Web API)
dotnet build Simando.slnx
dotnet test                                       # all .NET test projects (including Testcontainers)
dotnet test tests/Simando.Domain.Tests            # fast loop while working in Domain
dotnet test tests/Simando.Application.Tests       # application service tests

# Local dev: run dependencies and Web API
docker compose -f docker-compose.dev.yml up -d     # Postgres+PostGIS, RustFS (S3-compat)
cp src/Simando.Api/appsettings.Local.example.json src/Simando.Api/appsettings.Local.json
dotnet ef database update --project src/Simando.Infrastructure --startup-project src/Simando.Api
dotnet run --project src/Simando.Api -- seed-master-data
SeedAdmin__Password="<password>" dotnet run --project src/Simando.Api -- seed-admin
SeedDemo__Password="<password>" dotnet run --project src/Simando.Api -- seed-demo-users
dotnet run --project src/Simando.Api              # starts Web API at http://localhost:5000 (Scalar at /scalar/v1)

# Frontend (React 19 SPA via Bun)
cd frontend
bun install
bun run codegen                                   # generates src/api/schema.d.ts from OpenAPI spec
bun run dev                                       # starts Vite dev server with proxy to API
bun test                                          # runs frontend unit tests with Vitest
bun run check                                     # runs Biome lint/format check and tsc typecheck
bun run test:e2e                                  # runs Playwright E2E smoke tests

# Full containerised stack (API + Frontend + DB + Storage)
docker compose up --build
```

## Backlog

Work is tracked as a kanban board in `.devtool/features/` (markdown +
YAML frontmatter, rendered by the kanban-markdown VS Code extension).
All planned implementation epics (Epics 1-10) are completed in `.devtool/features/done/`.
When picking up new work or tasks:

- Starting work: set `status: "in-progress"`, bump `modified`.
- Finishing work: set `status: "done"`, set `completedAt`, move the file
  into `.devtool/features/done/`.
- New scope discovered mid-task that is not in any existing file: add a new
  feature file rather than letting it go untracked.

## Conventions

Full detail in [docs/build/conventions.md](docs/build/conventions.md) -
canonical for coding conventions, licensing, and secrets handling. Digest:

- `decimal`, never `double`, for money/volumetric values.
- Package manager: always use `bun` (or `pnpm`), never `npm`/`npx`. Use `bunx <pkg>` instead of `npx <pkg>`.
- Central package management for .NET: versions only in `Directory.Packages.props`.
- ClosedXML for Excel export, never EPPlus (licensing).
- Secrets never committed: use `appsettings.Local.json` (gitignored + dockerignored) or `dotnet user-secrets`.
- Indonesian domain vocabulary stays verbatim; routes/URLs stay English.
- Role strings in UI and notifications must be formatted with natural spacing via `formatRole()` (e.g. "Sales Area", "Area Head", "Reviewer 1", "Regional Admin", "Division Head"), avoiding unspaced PascalCase tokens.
- Em-dashes (long dashes / U+2014) are strictly prohibited in UI text and documentation; use standard hyphens (`-`), colons, or parentheses instead.
- DTO & Model naming taxonomy: `Dto` for top-level service payloads, `Item`/`Row` for grid line items, `Detail`/`Summary` for read projections, `Request`/`Filter` for inputs, `Result` for outcomes.
- Object mapping & projections: use **Mapster** for mapping domain entities to DTOs and EF Core LINQ query projections (`ProjectToType<TDto>()`). Implement `IRegister` in `Simando.Application` for complex/custom mappings.
- Comments explain "why," never "what," and only when needed or the code itself is not obvious; keep it minimal / compact.
- Use `shadcn/ui` components and Tailwind CSS v4, not raw unstyled HTML elements.
- Use `mapcn` for map plotting and geospatial features.
- Form handling: use `@tanstack/react-form` combined with `zod` schemas for client validation. Long multi-section forms must include bottom save action bars so users do not have to scroll to persist data.
- API interactions: use `$api.useQuery` and `$api.useMutation` from `openapi-react-query` generated types; never handwrite untyped fetch calls.
- Web API Controllers return RFC 7807 `ProblemDetails` on errors and use standard HTTP status codes.
- Git commit messages: single-line subject only following Conventional Commits (`type(scope): description` or `type: description`), no commit bodies. Never commit or push without explicit user approval.

## Architecture invariants (don't casually refactor these away)

- **Row-level security:** EF Core global query filters. See
  [architecture.md#row-level-security](docs/build/architecture.md#row-level-security).
- **System Admin data isolation:** System Admin has zero visibility into commercial case data. It is a
  platform role (accounts, config) with a narrow, audited break-glass exception. Break-glass access grants
  temporary, read-only inspection without operational action capabilities.
- **Hybrid Discontinue Flow:** A case may be discontinued either from `Draft` (by creator / sales area with edit
  permissions when a lead is dropped or unqualified) or from `Rejected` (by Regional Admin terminating a rejected case).
  Active evaluation steps (`AreaHead`, `RegionalAdmin`, `Reviewer1-3`, `Approval`) cannot be discontinued directly;
  their sole negative decisions are `Tolak` (escalating sideways to Regional Admin in `/tasks/blocked`) or `Revisi`
  (returning one step back).
- **Reject routes sideways to Regional Admin, not back to the submitter:** The workflow's core escalation rule;
  see [W10/W11 in testing.md](docs/build/testing.md#4-workflow-state-machine).
- **No pre-signed storage URLs, no Graph `downloadUrl`:** Attachment downloads
  always stream through an authorising endpoint that re-checks scope in both storage providers.
- **No in-app document signing:** The flow is generate `.docx` -> download ->
  sign externally -> re-upload -> attach. Do not add a signing pad, PDF stamping,
  or certificate handling.
- **Audit/status log is append-only:** Enforced by a DB trigger rejecting
  `UPDATE`/`DELETE` on `status_events`. Application-level discipline alone is not acceptable here.
- **Workflow chains are snapshotted at submit time:** Not resolved lazily from
  the current template, otherwise reassigning a role corrupts history mid-flight.
- **Gas-demand conversion (`survey_equipment.konversi_ke_gas`) is a plain,
  manually-typed field, not computed:** There is no conversion-factor table
  and no conversion service; do not reintroduce one.

## Testing priorities

Permission model (scope × capability × turn) and the workflow state machine
get **100% branch coverage** (see [docs/build/testing.md](docs/build/testing.md) §4 W1-W16);
everything else (CRUD forms, master data, reports) has automated integration and unit test coverage.

## Open items

Six features are deliberately not planned for v1; see
[docs/future/README.md](docs/future/README.md): amendment workflow,
daily-basis contracts, scenario comparison, Gate Review integration, approver
delegation, and OneDrive attachment storage. Each has a specified v1 seam
(usually: add the column now, hide the UI, gate behind an interface). If
implementing one of these areas, build to the seam and reference
`docs/future/README.md` in a code comment; do not resolve the open question
independently.
