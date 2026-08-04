# SIMANDO

**Sistem Manajemen Dokumen Proses Berlangganan Gas**

A workflow management system for gas subscription processes, built with **Blazor Web App (.NET 10, InteractiveServer)** and **BlazorBlueprint** (shadcn/ui for Blazor) on top of **Tailwind CSS v4**.

## Running

```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet run --project Api        # http://localhost:5010
dotnet run --project Frontend   # http://localhost:5009
```

Open `http://localhost:5009`. The Api applies EF Core migrations and seeds demo data on startup.

### Demo Accounts

All seeded on first run with password **`Passw0rd!`**:

| Email | Role | Description |
|---|---|---|
| `sales@pgn.id` | SalesArea | Input, create, and upload subscription data |
| `areahead@pgn.id` | AreaHead | View subscriptions in their area |
| `admin@pgn.id` | AdminRegional | Manage region, assign reviewers, fill evaluation data |
| `reviewer@pgn.id` | Reviewer | Review and approve/reject/revise subscriptions |
| `division@pgn.id` | DivisionHead | Final approval authority |

### Reseeding Database

Delete the DB and restart:

```bash
rm Api/Data/app.db Api/Data/app.db-wal Api/Data/app.db-shm 2>/dev/null
dotnet run --project Api
```

## Pages

| Route | Description |
|---|---|
| `/` | Dashboard — stat tiles, recent subscriptions, activity timeline |
| `/subscriptions` | List all subscriptions with status/area filters |
| `/subscriptions/{id}` | Detail view — workflow steps, document upload, review, sign-off |
| `/review` | Review queue — subscriptions pending reviewer action |
| `/evaluation` | Resume evaluasi — analysis and evaluation summary |
| `/admin/users` | User management (AdminRegional only) |

Global: sidebar (`Ctrl/Cmd+B`), command palette (`Ctrl+K`), dark mode toggle.

## Workflow

```
Directory → Plotting → Prospect → Survey → A1 → Permohonan NOL → Disetujui
                                                          ↓
                                                       Ditolak (back to Admin Regional)
```

- **Sales Area** creates subscriptions and uploads documents at each stage
- **A1** stage includes digital sign-off (checkbox)
- **Permohonan NOL** triggers review flow through assigned reviewers (1→2→3)
- Each reviewer: **Setuju** (next) / **Tolak** (back to admin) / **Revisi** (back 1 step)
- **Division Head** gives final approval after all reviewers approve

## Architecture

Three projects:

```
Shared/                     DTOs and enums shared by Api and Frontend
  Models.cs                 SubscriptionDto, ReviewStepDto, UserInfo, requests/responses, enums

Api/                        ASP.NET Core Web API — all business logic and data access
  Data/
    ApplicationDbContext.cs EF Core context (SQLite), ApplicationUser, IdentitySeeder
    SimandoRoles.cs
    Migrations/
  Controllers/               AuthController, SubscriptionsController, EvaluationController, UsersController
  Services/                  SubscriptionService, WorkflowService (status/review/sign-off logic)

Frontend/                   Blazor Server UI — pure HTTP client of the Api
  Services/                  Typed HttpClient wrappers (ISubscriptionService, IWorkflowService,
                              IEvaluationService, IActivityService, IUserService, IAuthService),
                              BearerTokenHandler forwards the JWT from the auth cookie's claims
  Components/
    Account/                 Login.razor calls Api's /api/auth/login, signs in a local cookie
    Layout/                  AppShell, AppSidebar, CommandPalette, UserMenu
    Subscriptions/            StatusBadge, WorkflowSteps, DocumentUpload, ReviewCard
    Pages/                    Home, Review, Evaluation, Subscriptions/*, Admin/Users
  wwwroot/
    app.css                   Component-specific CSS
    css/app.generated.css     Tailwind v4 compiled utilities
```

Auth model: the Frontend keeps a cookie-authenticated Blazor circuit, but the cookie's claims (identity + role + the API's JWT) come from calling the Api's login endpoint rather than a local Identity store. Every Frontend→Api call rides on that forwarded bearer token, so authorization is enforced by the Api regardless of what the Frontend UI shows or hides.

## Tech Stack

- .NET 10 / Blazor Web App (InteractiveServer)
- BlazorBlueprint 3.14.1 (shadcn/ui components)
- Tailwind CSS v4.3
- ASP.NET Core Identity + SQLite
- Entity Framework Core 10

## Deployment (Docker)

Production domain: **simando.legain.id**. This stack does *not* run its own reverse proxy — `api` and `frontend` join a pre-existing external Docker network (`caddy`) that an already-running, separately-managed Caddy instance is also attached to. Neither container publishes a host port; they're reachable only by container name (`api:8080`, `frontend:8080`) to whatever else is on that `caddy` network.

```bash
# one-time, only if the network doesn't already exist:
docker network create caddy

cp .env.example .env
# edit .env: set JWT_KEY to a real secret, e.g. `openssl rand -base64 48`

docker compose up -d --build
```

- **`api`** — SQLite database (`Data/app.db`) and uploaded documents (`wwwroot/uploads`) live in named Docker volumes (`api_data`, `api_uploads`) so they survive `docker compose down`/rebuilds.
- **`frontend`** — talks to `api` directly over the internal `caddy` network (`http://api:8080`), not through the external reverse proxy.

Add this site block to the **external** Caddy's own Caddyfile (not part of this repo) so it can route to these containers by name:

```
simando.legain.id {
    handle /api/* {
        reverse_proxy api:8080
    }
    handle {
        reverse_proxy frontend:8080
    }
}
```

Rebuild after a code change: `docker compose up -d --build`. Tear down (keeps volumes): `docker compose down`. Wipe the database too: `docker compose down -v`.
