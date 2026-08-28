# AGENTS.md — DMS Simando

Guidance for coding agents working in this repository. Read this before touching
code; it tells you where the real spec lives and which rules are load-bearing
rather than stylistic.

## What this is

**DMS - Simando** digitises PGN's gas-subscription sales pipeline — from an
industrial prospect in a directory, through survey and pricing, to the issuance
of a **NOL (No Objection Letter)**. Client is an Indonesian state-owned
enterprise (SOE); the system backs signed commercial documents, so correctness
and auditability outrank throughput.

**Repo status: pre-implementation.** `src/` currently holds only the default
Blazor Web App template plus empty `Domain`/`Application`/`Infrastructure`
projects — there is no domain code yet. `docs/` is complete and is the spec of
record: architecture, data model, workflow and roles are already settled.
Writing code here means **implementing the docs**, not designing from
scratch. When something is ambiguous, the answer is very likely already in
`docs/` — check there before inventing an approach.

## Start here

| Task                        | Read first                                                                                                                                                    |
| --------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Any new session             | [docs/README.md](docs/README.md) — the full map of `docs/`                                                                                                    |
| Backend / entities          | [docs/design/data-model.md](docs/design/data-model.md), [docs/build/architecture.md](docs/build/architecture.md)                                              |
| Workflow / approvals        | [docs/design/approval-workflow.md](docs/design/approval-workflow.md)                                                                                          |
| Frontend / screens          | [docs/design/frontend/13-page-role-matrix.md](docs/design/frontend/13-page-role-matrix.md) → [docs/design/frontend/README.md](docs/design/frontend/README.md) |
| Before writing tests        | [docs/build/testing.md](docs/build/testing.md)                                                                                                                |
| Storage / attachments       | [docs/build/storage.md](docs/build/storage.md)                                                                                                                |
| API & frontend conventions  | [docs/build/web-conventions.md](docs/build/web-conventions.md)                                                                                                |
| "Is this in scope for v1?"  | [docs/future/README.md](docs/future/README.md) — six features raised but not confirmed                                                                        |

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
- **Auth & RBAC:** ASP.NET Core Identity (SameSite=Lax Cookie Auth, Scoped `ICurrentUser` driving EF Core RLS query filters)
- **Background Jobs & Logging:** Hangfire · FluentValidation · Serilog
- **Frontend Runtime & Bundler:** **Bun** (strictly prefer `bun` / `bunx --bun` over `npm`/`npx`) + Vite 8
- **Frontend UI Framework:** **React 19** + TypeScript
- **Frontend Routing:** `@tanstack/react-router` (type-safe file-based routes & search params)
- **API Client & Caching:** `openapi-typescript` + `openapi-fetch` + `openapi-react-query` + `@tanstack/react-query` v5
- **Data Tables & Forms:** `@tanstack/react-table` v8 + `@tanstack/react-form` + `zod`
- **UI Components & Styling:** `shadcn/ui` (Tailwind CSS v4 + Radix UI) + `lucide-react`
- **Geospatial Mapping:** `mapcn` (`bunx --bun shadcn@latest add @mapcn/map`, powered by MapLibre GL)
- **Testing:** xUnit + Testcontainers (Backend) · Vitest + Playwright (Frontend & E2E)

## Solution layout

```
src/
  Simando.Domain/           entities, enums, value objects, domain rules
                             — NO EF Core dependency, deliberately (see below)
  Simando.Application/      use cases, DTOs, validators, service interfaces
  Simando.Infrastructure/   EF Core, storage (S3/RustFS), OpenXML, Identity
  Simando.Api/              ASP.NET Core 10 Web API, REST controllers, OpenAPI 3.1, auth
frontend/
  src/
    api/                    schema.d.ts (generated via codegen), client.ts ($api openapi-react-query)
    components/             shadcn/ui components, mapcn map, layout shell, shared widgets
    features/               feature modules (companies, survey, nol, tasks, admin, reports)
    routes/                 TanStack Router file-based routes
    hooks/                  custom react hooks
    lib/                    utilities, zod validation schemas
tests/
  Simando.Domain.Tests/         pure, fast, no I/O
  Simando.Application.Tests/
  Simando.Integration.Tests/    Testcontainers: real PostGIS + S3 store + WebApplicationFactory
  frontend.tests/               Vitest (hooks, schemas, tables) + Playwright (E2E smoke test)
```

Each of `Domain`/`Application`/`Infrastructure` is organised into feature
folders that mirror the module list in
[docs/build/architecture.md § Module breakdown](docs/build/architecture.md#module-breakdown)
(`Companies/`, `Survey/`, `Nol/`, `Workflow/`, …) — see
[docs/build/architecture.md § Solution structure](docs/build/architecture.md#solution-structure)
for the full folder-by-folder breakdown; don't create a module folder ahead of
the feature it belongs to. `Simando.Domain` must stay free of EF Core (or any
infrastructure) references. The workflow transition rules are the piece most
likely to be argued over with the client, so they must run and be
demonstrated without a database.

## Build, run, test

```bash
# Backend (.NET 10 Web API)
dotnet build Simando.slnx
dotnet test                                       # all .NET test projects
dotnet test tests/Simando.Domain.Tests            # fast loop while working in Domain

# Local dev: run dependencies and Web API
docker compose -f docker-compose.dev.yml up -d     # Postgres+PostGIS, RustFS (S3-compat)
cp src/Simando.Api/appsettings.Local.example.json src/Simando.Api/appsettings.Local.json
dotnet ef database update --project src/Simando.Infrastructure --startup-project src/Simando.Api
dotnet run --project src/Simando.Api -- seed-master-data
SeedAdmin__Password="<password>" dotnet run --project src/Simando.Api -- seed-admin
SeedDemo__Password="<password>" dotnet run --project src/Simando.Api -- seed-demo-users
dotnet run --project src/Simando.Api              # starts Web API at https://localhost:5001

# Frontend (React 19 SPA via Bun)
cd frontend
bun install
bun run codegen                                   # generates src/api/schema.d.ts from OpenAPI spec
bun run dev                                       # starts Vite dev server with proxy to API
bun test                                          # runs frontend unit tests with Vitest

# Full containerised stack (API + Frontend + DB + Storage)
docker compose up --build
```

## Backlog

Work is tracked as a kanban board in `.devtool/features/` (markdown +
YAML frontmatter, rendered by the kanban-markdown VS Code extension — see
the `kanban-markdown` skill for the file format). When you pick up or
finish work that has a feature file:

- Starting work: set `status: "in-progress"`, bump `modified`.
- Finishing work: set `status: "done"`, set `completedAt`, move the file
  into `.devtool/features/done/`.
- New scope discovered mid-task that isn't in any existing file: add a new
  feature file rather than letting it go untracked.

## Conventions

Full detail in [docs/build/conventions.md](docs/build/conventions.md) —
canonical for coding conventions, licensing, and secrets handling. Digest:

- `decimal`, never `double`, for money/volumetric values.
- Package manager: always use `bun` (or `pnpm`), never `npm`/`npx`. Use `bunx <pkg>` instead of `npx <pkg>`.
- Central package management for .NET — versions only in `Directory.Packages.props`.
- ClosedXML for Excel export, never EPPlus (licensing).
- Secrets never committed; use `appsettings.Local.json` (gitignored + dockerignored) or `dotnet user-secrets`.
- Indonesian domain vocabulary stays verbatim; routes/URLs stay English.
- DTO & Model naming taxonomy: `Dto` for top-level service payloads, `Item`/`Row` for grid line items, `Detail`/`Summary` for read projections, `Request`/`Filter` for inputs, `Result` for outcomes.
- Comments explain "why," never "what," and only when needed or the code itself is not obvious; keep it minimal / compact.
- Use `shadcn/ui` components and Tailwind CSS v4, not raw unstyled HTML elements.
- Use `mapcn` (`bunx --bun shadcn@latest add @mapcn/map`) for map plotting and geospatial features.
- Form handling: use `@tanstack/react-form` combined with `zod` schemas for client validation.
- API interactions: use `$api.useQuery` and `$api.useMutation` from `openapi-react-query` generated types; never handwrite untyped fetch calls.
- Web API Controllers return RFC 7807 `ProblemDetails` on errors and use standard HTTP status codes.
- Git commit messages: single-line subject only following Conventional Commits (`type(scope): description` or `type: description`), no commit bodies.

## Architecture invariants (don't casually refactor these away)

- **Row-level security:** EF Core global query filters. See
  [architecture.md#row-level-security](docs/build/architecture.md#row-level-security).
- **No pre-signed storage URLs, no Graph `downloadUrl`.** Attachment downloads
  always stream through an authorising endpoint that re-checks scope —
  in both storage providers.
- **No in-app document signing.** The flow is generate `.docx` → download →
  sign externally → re-upload → attach. Don't add a signing pad, PDF stamping,
  or certificate handling.
- **Audit/status log is append-only**, enforced by a DB trigger rejecting
  `UPDATE`/`DELETE` — application-level discipline alone is not acceptable here.
- **Workflow chains are snapshotted at submit time**, not resolved lazily from
  the current template — otherwise reassigning a role corrupts history mid-flight.
- **Gas-demand conversion (`survey_equipment.konversi_ke_gas`) is a plain,
  manually-typed field, not computed.** There is no conversion-factor table
  and no conversion service — don't reintroduce one.
- **System Admin has zero visibility into case data** — no super-admin. It's a
  platform role (accounts, config) with a narrow, audited break-glass
  exception. Don't reflexively add "admin bypasses this check."
- **Reject routes sideways to Regional Admin, not back to the submitter** — the
  workflow's one truly counter-intuitive rule; see
  [W10/W11 in testing.md](docs/build/testing.md#4-workflow-state-machine).

## Testing priorities

Not a blanket coverage target — see
[docs/build/testing.md](docs/build/testing.md) for the full fixture tables.
Permission model (scope × capability × turn) and the workflow state machine
get **100% branch coverage**; everything else (CRUD forms, master data)
gets a happy-path test.

## Open items

Six features are deliberately not planned for v1 — see
[docs/future/README.md](docs/future/README.md): amendment workflow,
daily-basis contracts, scenario comparison, Gate Review integration, approver
delegation, and OneDrive attachment storage. Each has a specified v1 seam
(usually: add the column now, hide the UI, gate behind an interface). If
you're implementing one of these areas, build to the seam and reference
`docs/future/README.md` in a code comment — don't resolve the open question
yourself.
