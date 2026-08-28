---
id: "epic-1-backend-web-api-foundation-2026-08-28"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-28T00:00:00.000Z"
modified: "2026-08-28T12:00:00.000Z"
completedAt: "2026-08-28T12:00:00.000Z"
labels: ["backend", "api", "openapi"]
order: "a0"
---

# Epic 1: Backend Web API Foundation & OpenAPI Spec

Establish the headless ASP.NET Core 10 Web API foundation with OpenAPI 3.1 generation, scoped identity security, and RFC 7807 problem details error handling.

## User Stories & Scope

- [x] **Story 1.1:** Restructure `Simando.Web` into `Simando.Api` (remove Blazor components, assets, and Razor views).
- [x] **Story 1.2:** Configure ASP.NET Core 10 OpenAPI with `Microsoft.AspNetCore.OpenApi` + `Scalar.AspNetCore` interactive documentation UI at `/openapi/v1.json` & `/scalar/v1`.
- [x] **Story 1.3:** Implement `ApiCurrentUser` (scoped `ICurrentUser` resolved from `HttpContext.User`) to maintain EF Core RLS global query filters on `SimandoDbContext`.
- [x] **Story 1.4:** Setup global exception handling middleware with standard RFC 7807 `ProblemDetails` and structured validation response formatting.
- [x] **Story 1.5:** Configure CORS and Cookie Authentication settings for SPA consumption (supporting both Vite dev proxy and direct same-origin calls).

## Acceptance Criteria

1. API runs on .NET 10 without any Blazor/Razor dependencies.
2. Navigating to `/scalar/v1` displays interactive API docs.
3. `/openapi/v1.json` returns a valid OpenAPI 3.1 JSON specification.
4. EF Core queries automatically filter data by the authenticated user's `AreaId` / `RegionId` scope via `ApiCurrentUser`.
5. Invalid requests return standard `ProblemDetails` JSON with accurate validation errors.
