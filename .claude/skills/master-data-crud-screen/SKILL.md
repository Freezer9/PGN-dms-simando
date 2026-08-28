---
name: master-data-crud-screen
description: >
  Playbook for building an admin CRUD screen for a master-data/lookup
  entity in Simando DMS. Use when asked to add a new /admin/master/*
  screen, wire up CRUD for an AuditableEntity, or build a master data
  management page using React 19, TanStack Table, TanStack Form, and shadcn/ui.
---

# Master-data CRUD Screen Playbook

Built once (Segments, FuelTypes), reused for every other master-data admin screen. Read this before starting a new one — most of the plumbing already exists; the work is almost always just the checklist below, not new architecture.

---

## 1. Architecture Overview — Reuse, Don't Rebuild

- **Backend Entity & Endpoints**:
  - `AuditableEntity` (`src/Simando.Domain/Common/AuditableEntity.cs`) — `Id`, `CreatedAt`, `DeletedAt` with soft-delete query filters.
  - RESTful Endpoints (`/api/admin/master/{entity}`) — GET list, POST create, PUT update, DELETE soft-delete.
  - OpenAPI Spec auto-generates TypeScript types into `frontend/src/api/schema.d.ts`.
- **Frontend Data Layer**:
  - `$api.useQuery('get', '/api/admin/master/{entity}')` for fetching data with automatic caching and background revalidation.
  - `$api.useMutation('post', ...)` and `$api.useMutation('put', ...)` for create/update.
  - `$api.useMutation('delete', ...)` for soft-delete.
- **UI & Interaction Components**:
  - `@tanstack/react-table` + `shadcn/ui` `Table` for sorting, search filtering, and pagination.
  - `@tanstack/react-form` + `zod` inside `Dialog` / `Sheet` for create and edit forms.
  - `AlertDialog` for delete confirmation.
  - `sonner` (`toast.success`, `toast.error`) for feedback.

---

## 2. Checklist for a New Master-Data Screen

1. **Verify Backend Endpoint**: Confirm `/api/admin/master/<entity>` is registered in OpenAPI (`bun run codegen`).
2. **Create Route File** (`frontend/src/routes/admin/master/<entity>.tsx`):
   ```tsx
   import { createFileRoute } from '@tanstack/react-router';
   import { useState } from 'react';
   import { $api } from '@/api/client';
   import { DataTable } from '@/components/ui/data-table';
   import { Button } from '@/components/ui/button';
   import { Plus } from 'lucide-react';
   import { MasterDataFormDialog } from '@/features/admin/components/MasterDataFormDialog';
   import { DeleteConfirmDialog } from '@/features/admin/components/DeleteConfirmDialog';

   export const Route = createFileRoute('/admin/master/<entity>')({
     component: MasterDataScreen,
   });

   function MasterDataScreen() {
     const [isCreateOpen, setIsCreateOpen] = useState(false);
     const [editingItem, setEditingItem] = useState<ItemType | null>(null);
     const [deletingItem, setDeletingItem] = useState<ItemType | null>(null);

     const { data: items, isLoading, refetch } = $api.useQuery('get', '/api/admin/master/<entity>');

     // ... table columns & handlers
   }
   ```
3. **Table Columns Setup**:
   - Small fixed columns (Urutan, Code) go **first** (`w-20`).
   - The primary identifying column (Nama) takes flexible width (`w-full` / `flex-1`).
   - Actions column goes **last** with fixed width (`w-24`) containing Edit (`Pencil`) and Delete (`Trash2`) buttons.
4. **No "Show Deleted" UI**: Deleting an item soft-deletes it on the backend; the row disappears from the table immediately upon mutation success. Do not add a deleted toggle/badge unless explicitly requested.
5. **Card Subtitle**: Write one clear sentence explaining the entity's **purpose** (where it is used in the pipeline) — do not copy static seed values into subtitle text.
6. **Form Validation & Conflict Handling**:
   - Use Zod schema matching the backend contract.
   - On `409 Conflict` (duplicate name/code), display the inline error: `"Nama [Entity] sudah digunakan."`
   - On success: `toast.success(isEdit ? "[Entity] berhasil diperbarui." : "[Entity] berhasil ditambahkan.")`.
7. **Delete Confirmation**:
   - Open `AlertDialog` asking: `"Apakah Anda yakin ingin menghapus [Entity] \"{item.name}\"?"`
   - On success: `toast.success("[Entity] berhasil dihapus.")` and invalidate queries.

---

## 3. Large Tables vs. Enum-like Lookups

- **Small Lookup Tables** (Segment, FuelType, Country, IndustryType, MeterSize, MrsSpec, ReasonCategory, UnitOfMeasure, Province):
  - Fetched once via `/api/admin/master/{entity}`.
  - Client-side sorting and filtering in TanStack Table.
- **Large Tables** (Village ~83k, District ~7k, Regency ~500):
  - **Must use server-side pagination and scoping**: `/api/admin/master/villages?districtId=...&page=1&pageSize=50`.
  - Pass `pageIndex`, `pageSize`, and filters directly to `$api.useQuery`.

---

## 4. Working Norms

- Use **Bun** (`bun run codegen`, `bun run dev`, `bun test`). Never use `npm`.
- Verify TypeScript types match after codegen before completing tasks.
- Commit only when explicitly requested, using Conventional Commits single-line subject messages.
