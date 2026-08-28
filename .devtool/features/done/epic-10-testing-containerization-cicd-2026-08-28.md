---
id: "epic-10-testing-containerization-cicd-2026-08-28"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-28T00:00:00.000Z"
modified: "2026-08-29T02:15:00.000Z"
completedAt: "2026-08-29T02:15:00.000Z"
labels: ["devops", "testing", "docker", "e2e"]
order: "a9"
---

# Epic 10: Testing, Containerization & CI/CD

Configure Docker compose multi-container orchestration with Caddy reverse proxy, backend integration tests with Testcontainers, frontend Vitest unit tests, and full sales pipeline E2E integration tests.

## User Stories & Scope

- [x] **Story 10.1:** Multi-stage Dockerfile for React SPA using Bun builder and Caddy runtime (`caddy:2-alpine`), production `Caddyfile` for SPA routing fallback and API reverse proxying (`/api/*`, `/scalar/*`, `/openapi/*`), and hardened multi-container orchestration in `docker-compose.yml`.
- [x] **Story 10.2:** Comprehensive Backend Integration Tests (`Simando.Integration.Tests`) testing all API controllers using `WebApplicationFactory<Program>` with real PostGIS & RustFS/MinIO Testcontainers.
- [x] **Story 10.3:** Frontend Vitest unit test suite covering auth hooks, capability evaluation, TanStack Table filters, and utility functions.
- [x] **Story 10.4:** End-to-End sales pipeline test suite (`Simando.E2E.Tests`) verifying the full 8-stage sales lifecycle from prospect creation through survey, A1 registration, multi-level review and approval, NOL evaluation, NOL issuance, and document download.

## Acceptance Criteria

1. `docker compose up --build` runs the entire stack (PostgreSQL + PostGIS 18, RustFS S3 storage, .NET 10 Web API, React SPA via Caddy reverse proxy).
2. `dotnet test` runs and passes all 288 unit, integration, and E2E tests against real Testcontainers across the entire solution.
3. Vitest unit tests pass 100% (69/69) across all frontend test suites.
4. E2E pipeline test passes the full end-to-end sales pipeline happy path from prospect creation through survey and approval to NOL issuance and document generation.
