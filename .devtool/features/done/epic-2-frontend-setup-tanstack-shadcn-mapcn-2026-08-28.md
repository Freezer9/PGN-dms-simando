---
id: "epic-2-frontend-setup-tanstack-shadcn-mapcn-2026-08-28"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-28T00:00:00.000Z"
modified: "2026-08-28T16:05:00.000Z"
completedAt: "2026-08-28T16:05:00.000Z"
labels: ["frontend", "tanstack", "shadcn", "mapcn", "bun"]
order: "a1"
---

# Epic 2: Frontend Setup, TanStack Suite, shadcn/ui & mapcn

Bootstrap the React 19 Single-Page Application using Bun, Vite 8, the complete TanStack suite, Biome toolchain, shadcn/ui, mapcn, and automated OpenAPI client codegen.

## User Stories & Scope

- [x] **Story 2.1:** Initialize React 19 + Vite 8 + TypeScript application in `frontend/` using `bunx @tanstack/cli create --router-only --toolchain biome`.
- [x] **Story 2.2:** Initialize **shadcn/ui** with Tailwind CSS v4, Lucide icons, `cn` utility, and core UI primitives: Button, Dialog, Card, Input, Select, DropdownMenu, Table, Badge, Tabs, Sonner Toast.
- [x] **Story 2.3:** Add **mapcn** ([mapcn.dev](https://www.mapcn.dev/)) map components via `@mapcn/map` (MapLibre GL) for theme-aware mapping, markers, popups, and zoom controls.
- [x] **Story 2.4:** Configure `@tanstack/react-router` with root layout, router context, and route tree generation.
- [x] **Story 2.5:** Configure `@tanstack/react-query` v5 QueryClient with default caching policies, error handling, and toast notifications.
- [x] **Story 2.6:** Configure `@tanstack/react-form` form helpers & field wrappers integrated with shadcn/ui input components and Zod validator.
- [x] **Story 2.7:** Setup `openapi-typescript`, `openapi-fetch`, and `openapi-react-query` with custom client (`src/api/client.ts`) and `bun run codegen` script.

## Acceptance Criteria

1. Running `bun run dev` boots the frontend with instant HMR.
2. Running `bun run codegen` fetches `/openapi/v1.json` and outputs zero-runtime type definitions into `src/api/schema.d.ts`.
3. `$api.useQuery` and `$api.useMutation` provide full TypeScript autocompletion on endpoints, parameters, request bodies, and responses.
4. shadcn/ui primitives and mapcn map components render and match the design system.
5. Strict package manager rule enforced: all package installations and scripts use `bun` / `bunx --bun`.
