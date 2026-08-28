# Build — Web API & Frontend Conventions

> **Canonical.** This document owns the REST API conventions, OpenAPI contract generation, and React frontend architectural patterns. [architecture.md](architecture.md) references it rather than restating it.

---

## 1. Backend REST API Conventions

### Route Structure & Resource Naming
All Web API endpoints are organized under the `/api/` route prefix and grouped logically by domain resources using ASP.NET Core API Controllers:

| Resource Path | Controller | Primary Responsibilities |
|---|---|---|
| `/api/auth/*` | `AuthController` | Sign-in, sign-out, session verification, change-password |
| `/api/companies/*` | `CompaniesController` | Company directory, plotting, stages 1–3 data |
| `/api/survey/*` | `SurveyController` | Stage 4 KK0 survey data, equipment, gas conversion |
| `/api/registration/*` | `RegistrationController` | Stage 5 A1 customer registration |
| `/api/nol/*` | `NolController` | Stages 6–8 NOL request, evaluation, issuance |
| `/api/tasks/*` | `TasksController` | User tasks inbox, stuck-steps monitor, action history |
| `/api/workflow/*` | `WorkflowController` | Approval step execution (Setuju, Revisi, Tolak) |
| `/api/reports/*` | `ReportsController` | Funnel, gas demand, ageing, ClosedXML Excel exports |
| `/api/attachments/*` | `AttachmentsController` | Multipart file upload, secure authenticated stream downloads |
| `/api/documents/*` | `DocumentsController` | OpenXML Lampiran docx generation and stream downloads |
| `/api/admin/*` | `AdminController` | Users, roles, organisation hierarchy, master lookup CRUDs |

### Response Envelopes & Error Handling (RFC 7807 ProblemDetails)
- **Success Responses:** Return direct JSON DTOs or collections with standard HTTP status codes (`200 OK`, `201 Created`, `204 No Content`).
- **Error Responses:** All validation errors, business rule violations, domain exceptions, and authorization failures return RFC 7807 standard `ProblemDetails` or `ValidationProblemDetails` (`400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `409 Conflict`, `422 Unprocessable Entity`).

```csharp
[HttpPost]
[ProducesResponseType(typeof(CompanyDto), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
public async Task<IActionResult> Create([FromBody] CreateCompanyRequest request, CancellationToken ct)
{
    // ...
}
```

### OpenAPI 3.1 & Metadata Specifications
- Built-in OpenAPI is configured via `builder.Services.AddOpenApi()` in ASP.NET Core 10.
- Interactive API documentation is available locally via Scalar at `/scalar/v1`, exposing the raw specification at `/openapi/v1.json`.
- Every controller action must include `[ProducesResponseType]` annotations for all expected HTTP status codes to produce complete, accurate TypeScript types during codegen.

---

## 2. Authentication, RBAC & Row-Level Security

- **Cookie Authentication:** ASP.NET Core Identity configures `SameSite=Lax` HTTP-only cookies with secure attributes.
- **Per-Request `ICurrentUser`:** An `ApiCurrentUser` scoped service resolves the authenticated principal from `HttpContext.User`, loading their active role, assigned `RegionId`, `AreaId`, and computed `AccessScope`.
- **EF Core RLS:** Global query filters in `SimandoDbContext` automatically apply Area/Region visibility constraints across all queries executed in that request context.
- **Capability Authorization:** Endpoints enforce RBAC capabilities via `[Authorize(Policy = "...")]` or custom `[RequireCapability(Capability.ManageMasterData)]` attributes.

---

## 3. Attachment & Document Streaming

Every file download (attachments, OpenXML `.docx` lampiran merges, ClosedXML `.xlsx` exports) is served through an authenticated API endpoint:
- **No Pre-Signed Storage URLs:** Storage keys are never exposed directly to clients. Downloads stream securely through the backend after verifying user permissions and row-level access scope.
- **Content-Disposition Headers:** Handlers set `Content-Disposition: attachment; filename="..."` with verified MIME types (`application/vnd.openxmlformats-officedocument.wordprocessingml.document`, `application/pdf`, `image/jpeg`).

---

## 4. Frontend Architecture & Tooling

### Package Manager & Scripts
Always use **Bun** (or `pnpm` if bun is not suitable), never `npm`/`npx`.

```bash
bun install                                       # install npm dependencies
bun run codegen                                   # generate src/api/schema.d.ts from /openapi/v1.json
bun run dev                                       # start Vite dev server with proxy
bun test                                          # run Vitest unit tests
bun run build                                     # produce optimized production build
```

### Zero-Runtime Type-Safe Client (`openapi-typescript` + `openapi-react-query`)
1. **Schema Generation:** `openapi-typescript` generates static TypeScript types in `src/api/schema.d.ts` from `/openapi/v1.json`.
2. **Client Configuration:**
   ```typescript
   // src/api/client.ts
   import createFetchClient from 'openapi-fetch';
   import createClient from 'openapi-react-query';
   import type { paths } from './schema';

   export const fetchClient = createFetchClient<paths>({
     baseUrl: '/api',
     credentials: 'include',
   });

   export const $api = createClient(fetchClient);
   ```
3. **Usage in Components:**
   ```typescript
   // Type-safe query
   const { data, isLoading } = $api.useQuery('get', '/api/companies', {
     params: { query: { stage: 'Prospek', page: 1, pageSize: 20 } },
   });

   // Type-safe mutation
   const mutation = $api.useMutation('post', '/api/companies');
   ```

### Routing: TanStack Router
- **File-Based Routing:** Route trees live under `src/routes/`.
- **Search Parameter Validation:** Every table filter, pagination param, and tab state is validated using `zodSearchValidator`.
- **Prefetching:** Route loaders prefetch critical data using `$api.queryOptions(...)` before rendering pages.

### Forms: TanStack Form + Zod
- Use `@tanstack/react-form` for all multi-step and dynamic forms (Survey KK0, A1 Registration, Plotting, Approval Action dialogs).
- Bind form field validation schemas directly with `zod`.
- Dynamic arrays (e.g. Survey equipment and material rows) use TanStack Form's fine-grained field array helpers.

### Tables: TanStack Table v8
- Headless table configuration for Directory, Tasks Inbox, Stuck-Steps, and Master Data grids.
- Paired with `shadcn/ui` table primitives (`Table`, `TableHeader`, `TableRow`, `TableCell`).

### UI & Maps
- **UI Components:** `shadcn/ui` (built on Tailwind CSS v4 and Radix UI primitives) with `lucide-react` icons.
- **Geospatial Maps:** `mapcn` (`@mapcn/map`), installed via `bunx --bun shadcn@latest add @mapcn/map`, providing MapLibre GL map components for interactive coordinate plotting and pin-drops.
