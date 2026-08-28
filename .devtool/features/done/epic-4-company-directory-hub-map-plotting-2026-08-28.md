---
id: "epic-4-company-directory-hub-map-plotting-2026-08-28"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-28T00:00:00.000Z"
modified: "2026-08-28T09:55:00.000Z"
completedAt: "2026-08-28T09:55:00.000Z"
labels: ["backend", "frontend", "directory", "mapcn", "tanstack-table"]
order: "a3"
---

# Epic 4: Company Directory, Record Hub & Geospatial Plotting

Deliver the company directory with TanStack Table, new company prospect creation with cascading geography dropdowns, the 9-tabbed Company Record Hub, and MapLibre geospatial plotting with mapcn.

## User Stories & Scope

- [x] **Story 4.1:** Build Backend Directory & Company APIs (`GET /api/companies`, `POST /api/companies`, `GET/PUT/DELETE /api/companies/{id}`, `GET/PUT /api/companies/{id}/contacts`).
- [x] **Story 4.2:** Build Frontend Company Directory (`/directory`) with **TanStack Table v8** and shadcn/ui: multi-column sorting, stage filter pills, search bar, and server-side pagination.
- [x] **Story 4.3:** Build Frontend Create Prospect Form (`/directory/new`) with **TanStack Form** and province/regency/district/village cascading dropdowns.
- [x] **Story 4.4:** Build Frontend Company Record Hub (`/directory/$companyId`): 9-tabbed interface (Overview, Contacts, Plotting, Survey KK0, A1, Permohonan NOL, Evaluasi NOL, Penerbitan NOL, Documents & Timeline).
- [x] **Story 4.5:** Build Plotting & Map API (`GET/PUT /api/companies/{id}/plotting`, `GET /api/companies/map-pins`).
- [x] **Story 4.6:** Build Frontend Interactive Geospatial Map (`/map` and Plotting tab) with **mapcn**: PostGIS coordinate pin drop, radius/bounding box query, pipeline proximity indicator, and company preview drawer.

## Acceptance Criteria

1. Directory table lists companies respecting user's Area/Region row-level security with fast pagination, sorting, and stage filters.
2. Creating a new prospect auto-allocates atomic sequence number and renders formatted company nomor (`{seq}-{prov}-{kab}`).
3. Record Hub tabs display stage-specific read/edit views and status badges.
4. Geospatial map renders PostGIS pins with interactive popup cards and allows sales area users to drop/adjust company coordinates.
