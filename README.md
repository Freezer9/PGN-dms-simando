# DMS - Simando

**Sistem Manajemen Proses Berlangganan Gas** — the web system that digitises
PGN's gas-subscription sales pipeline: from an industrial prospect in a
directory, through survey and pricing, to the issuance of a **NOL (No
Objection Letter)**.

## Stack

.NET 10 · ASP.NET Core · Blazor Web App (`InteractiveServer`) · EF Core 10 +
Npgsql · PostgreSQL 18 + PostGIS · S3-compatible object storage · Hangfire ·
Serilog. Self-hosted.

See [docs/build/architecture.md](docs/build/architecture.md) for the
reasoning behind each choice.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Docker (with Compose)
- A JS package manager for the Tailwind build: [bun](https://bun.sh) (primary,
  `bun.lock` committed), pnpm, or npm — any one works

## Quickstart

```bash
# 1. Start dependencies (Postgres+PostGIS, S3-compatible storage, docx→pdf converter)
docker compose -f docker-compose.dev.yml up -d

# 2. Configure the app for local dev
cp src/Simando.Web/appsettings.Local.example.json src/Simando.Web/appsettings.Local.json

# 3. Apply EF Core database migrations
dotnet ef database update --project src/Simando.Infrastructure --startup-project src/Simando.Web

# 4. Seed master data (geography, units of measure, industry types, etc.)
dotnet run --project src/Simando.Web -- seed-master-data

# 5. Seed initial accounts (System Admin & Demo Users)
SeedAdmin__Password="<a strong password>" dotnet run --project src/Simando.Web -- seed-admin
SeedDemo__Password="<a shared password>" dotnet run --project src/Simando.Web -- seed-demo-users

# 6. Run (builds JS deps + Tailwind CSS automatically on restore/build)
dotnet run --project src/Simando.Web
```

### Hot reload

```bash
scripts/dev.sh          # Windows: scripts/dev.ps1
```

Runs `dotnet watch` (C#/Razor hot reload, auto browser refresh) and the
Tailwind watcher together in one terminal; `Ctrl+C` stops both.

### Full containerised stack

```bash
docker compose up --build
```

Runs the app itself in a container alongside its dependencies. Use this to
mirror deployment; use the quickstart above for day-to-day development.

## Seeding accounts

There's no self-service sign-up — the app has no one to sign in as until an
account is seeded.

### First System Admin

```bash
SeedAdmin__Password="<a strong password>" dotnet run --project src/Simando.Web -- seed-admin
```

Creates the one seed System Admin (`must_change_password` is set, so it's
forced through `/change-password` on first sign-in). Safe to rerun — it
no-ops once an active System Admin exists. See
[docs/design/roles-permissions.md §2.6](docs/design/roles-permissions.md#bootstrapping-the-first-admin).

### Demo / testing accounts, one per role

```bash
SeedDemo__Password="<a shared password>" dotnet run --project src/Simando.Web -- seed-demo-users
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
dotnet run --project src/Simando.Web -- seed-master-data
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
  Simando.Web/              Blazor Web App, components, endpoints, auth
tests/
  Simando.Domain.Tests/
  Simando.Application.Tests/
  Simando.Integration.Tests/
  Simando.Web.Tests/
  Simando.E2E.Tests/
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
