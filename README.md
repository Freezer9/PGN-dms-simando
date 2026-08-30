# DMS - Simando

**Sistem Manajemen Proses Berlangganan Gas** — the web system that digitises
PGN's gas-subscription sales pipeline: from an industrial prospect in a
directory, through survey and pricing, to the issuance of a **NOL (No
Objection Letter)**.

## Stack

- **Backend:** .NET 10 (LTS) · ASP.NET Core 10 Web API · OpenAPI 3.1 (`Microsoft.AspNetCore.OpenApi`) + Scalar UI · EF Core 10 + Npgsql · PostgreSQL 18 + PostGIS · S3-compatible object storage · Hangfire · Serilog.
- **Frontend:** React 19 + TypeScript · **Bun** & Vite 8 · TanStack Suite (`@tanstack/react-router`, `@tanstack/react-query` v5, `@tanstack/react-table` v8, `@tanstack/react-form` + `zod`) · `openapi-typescript` + `openapi-fetch` + `openapi-react-query` · `shadcn/ui` (Tailwind CSS v4 + Radix UI) · `mapcn` (MapLibre GL).

See [docs/build/architecture.md](docs/build/architecture.md) for the
reasoning behind each choice.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Docker (with Compose)
- [Bun](https://bun.sh) (primary package manager & runtime for frontend, `bun.lock` committed; pnpm as fallback)

## Quickstart

### 1. Backend (.NET 10 Web API)

```bash
# Start dependencies (Postgres+PostGIS, S3-compatible storage, docx→pdf converter)
docker compose -f docker-compose.dev.yml up -d

# Configure the Web API for local dev
cp src/Simando.Api/appsettings.Local.example.json src/Simando.Api/appsettings.Local.json

# Apply EF Core database migrations
dotnet ef database update --project src/Simando.Infrastructure --startup-project src/Simando.Api

# Seed master data (geography, units of measure, industry types, etc.)
dotnet run --project src/Simando.Api -- seed-master-data

# Seed initial accounts (System Admin & Demo Users)
SeedAdmin__Password="<a strong password>" dotnet run --project src/Simando.Api -- seed-admin
SeedDemo__Password="<a shared password>" dotnet run --project src/Simando.Api -- seed-demo-users

# Run the Web API backend (listens on https://localhost:5001, OpenAPI at /openapi/v1.json, Scalar UI at /scalar/v1)
dotnet run --project src/Simando.Api
```

### 2. Frontend (React 19 SPA)

```bash
cd frontend

# Install dependencies via Bun
bun install

# Generate TypeScript API definitions from the running Web API OpenAPI spec
bun run codegen

# Start Vite development server (proxies /api to https://localhost:5001)
bun run dev
```

### Full containerised backend stack

```bash
docker compose up --build -d
```

Runs the backend stack (PostgreSQL + PostGIS, S3 storage, .NET 10 Web API) in containers. The Web API is published on `127.0.0.1:5000`.

### Production reverse proxy with Caddy

For deployment with an external Caddy instance (or alongside existing projects), build the frontend and configure Caddy to reverse proxy the backend and serve the static SPA assets:

1. Build the production React bundle:
   ```bash
   cd frontend && bun install && bun run build
   ```
2. Configure your external Caddy using [Caddyfile.example](Caddyfile.example):
   - Serves `frontend/dist` with SPA routing (`try_files`).
   - Reverse proxies `/api/*`, `/scalar/*`, and `/openapi/*` to `127.0.0.1:5000`.
   - Handles TLS, gzip/zstd compression, and security headers.

## Seeding accounts

There's no self-service sign-up — the app has no one to sign in as until an
account is seeded.

### First System Admin

```bash
SeedAdmin__Password="<a strong password>" dotnet run --project src/Simando.Api -- seed-admin
```

Creates the one seed System Admin (`must_change_password` is set, so it's
forced through `/change-password` on first sign-in). Safe to rerun — it
no-ops once an active System Admin exists. See
[docs/design/roles-permissions.md §2.6](docs/design/roles-permissions.md#bootstrapping-the-first-admin).

### Demo / testing accounts, one per role

```bash
SeedDemo__Password="<a shared password>" dotnet run --project src/Simando.Api -- seed-demo-users
```

Creates a "Demo Region"/"Demo Area" plus one account per non-SysAdmin role,
all sharing the password you supply (no forced change on first sign-in —
deliberate, so the same shared password keeps working for whoever signs in
next):

Sign in with the email below, not the username — login is by email.

| Role | Email |
|---|---|
| Sales Area | `demo.salesarea@simando.local` |
| Area Head | `demo.areahead@simando.local` |
| Regional Admin | `demo.regionaladmin@simando.local` |
| Reviewer | `demo.reviewer@simando.local` |
| Division Head | `demo.divisionhead@simando.local` |

Idempotent — rerun anytime, including after a DB reset. Doesn't create a
System Admin; use `seed-admin` above for that role.

Both commands read their password from config (`SeedAdmin:Password` /
`SeedDemo:Password` — see `appsettings.Local.example.json`) or the matching
environment variable (`SeedAdmin__Password` / `SeedDemo__Password`), never
committed with a real value.

## Seeding master data

```bash
dotnet run --project src/Simando.Api -- seed-master-data
```

No credentials needed. Imports the go-live-prerequisite lookup tables that
ship with the repo rather than needing PGN input (`docs/domain/master-data.md
§13`):

- **Administrative geography** — Province/Regency/District/Village, ~91,600
  rows from [cahyadsn/wilayah](https://github.com/cahyadsn/wilayah) (the
  dataset wilayah.id's API is itself built from). There's deliberately no
  admin UI for this table — see the doc — so this import is the only way
  the rows get in.
- **Units of measure, fuel types, industry types, countries, segments** —
  small lists sourced from the doc itself (or ISO 3166-1 for countries).

Idempotent per table — safe to rerun, each list is skipped once it has any
rows. Doesn't touch Regions/Areas, user accounts, meter sizes, document
templates, or reference documents — those are blocked on PGN-supplied data
and have no source to seed from yet.

## Project structure

```
src/
  Simando.Domain/           entities, enums, value objects, domain rules
  Simando.Application/      use cases, DTOs, validators, service interfaces
  Simando.Infrastructure/   EF Core, storage, document generation, Identity
  Simando.Api/              ASP.NET Core 10 Web API, REST controllers, OpenAPI 3.1, auth
frontend/
  src/
    api/                    schema.d.ts (generated), client.ts ($api openapi-react-query)
    components/             shadcn/ui components, mapcn map, layout shell, shared widgets
    features/               feature modules (companies, survey, nol, tasks, admin, reports)
    routes/                 TanStack Router file-based routes
    hooks/                  custom react hooks
    lib/                    utilities, zod validation schemas
tests/
  Simando.Domain.Tests/
  Simando.Application.Tests/
  Simando.Integration.Tests/
  frontend.tests/
```

See [AGENTS.md](AGENTS.md) for the rules behind this layout (e.g. why
`Simando.Domain` has no EF Core dependency), and
[docs/build/architecture.md § Solution structure](docs/build/architecture.md#solution-structure)
for the full feature-folder breakdown within each project.

## Testing

```bash
dotnet test
```

Integration tests spin up real PostgreSQL/PostGIS and object storage via
Testcontainers. See [docs/build/testing.md](docs/build/testing.md) for
what's tested and to what depth — a few high-consequence areas (the
permission model, the workflow state machine) are held to 100% branch
coverage; most of the app is not.

## Documentation

| If you want... | Read |
|---|---|
| The full design-doc map (domain, data model, workflow, frontend) | [docs/README.md](docs/README.md) |
| Conventions and invariants for contributing code (human or AI) | [AGENTS.md](AGENTS.md) |
| Architecture and stack rationale | [docs/build/architecture.md](docs/build/architecture.md) |
| Features not yet planned for v1 | [docs/future/README.md](docs/future/README.md) |

## Configuration & secrets

`appsettings.json` holds committed defaults. Real credentials go in
`appsettings.Local.json` (gitignored and dockerignored — never commit it) or
`dotnet user-secrets`. Copy `appsettings.Local.example.json` as a starting
point; if you add a new local-only key, update that example file too.
