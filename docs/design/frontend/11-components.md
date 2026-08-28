# Frontend 11 — Component Library

Shared React components and primitives built on `shadcn/ui` (Radix UI + Tailwind CSS v4), `lucide-react`, `mapcn`, and `@tanstack/react-table`. Building these shared components ensures visual consistency, high accessibility, and clean code reuse across all feature views.

---

## 1. Status & Workflow Components

### `<StatusBadge>` / Status Chip
Renders record and workflow status with standardized colors, icons, and themes:
- `DRAFT` / `BELUM_SURVEY`: Slate / Gray muted badge (`variant="secondary"`).
- `IN_PROGRESS` / `DALAM_EVALUASI` / `MENUNGGU_PERSETUJUAN`: Amber / Yellow badge (`bg-amber-100 text-amber-800 dark:bg-amber-950 dark:text-amber-300`).
- `APPROVED` / `TERBIT_NOL` / `SELESAI`: Emerald / Green badge (`bg-emerald-100 text-emerald-800 dark:bg-emerald-950 dark:text-emerald-300`).
- `REVISI` / `TOLAK` / `TERTAHAN`: Rose / Red badge (`bg-rose-100 text-rose-800 dark:bg-rose-950 dark:text-rose-300`).

### `<StatTile>` (KPI Card)
Unified metric card component used on all role dashboards (`SalesAreaDashboard`, `RegionalAdminDashboard`, `ApproverDashboard`, `SystemAdminDashboard`).
- **Props**: `label`, `value`, `subtext`, `icon` (`LucideIcon`), `trend` (optional percentage/direction), and `className`.
- Built with `shadcn/ui` `Card`, `CardHeader`, `CardTitle`, and `CardContent`.

### `<StageStepper>`
Visual progress indicator for the eight pipeline stages:
- **Completed**: Green checkmark circle with connecting solid line.
- **Active / Current**: Brand blue ring with glowing pulse indicator.
- **Pending**: Muted slate circle with dashed connecting line.
- Interactive click handlers to switch stage tabs if the user has read/edit permissions for that stage.

---

## 2. Form Control Guidelines (TanStack Form + shadcn/ui)

To ensure predictable UX across forms and master data screens:

- **`<Select>` (Native / Radix Dropdown)**:
  - **When to use**: Short, static option cardinality (< 10 items) where search filtering is unnecessary.
  - **Examples**: `Tahap`, `Posisi Pelanggan`, `Kawasan`, `SortMode`, `UnitDimension`.
- **`<Combobox>` (Searchable Command / Popover)**:
  - **When to use**: High-cardinality datasets, dynamic lookups, or lists > 10 items requiring text filtering.
  - **Examples**: `Provinsi`, `Kota/Kabupaten`, `Kecamatan`, `Kelurahan/Desa`, `Jenis Industri`, `User Reassignment`.
  - **Behavior**: Trigger buttons truncate overflow text (`truncate max-w-[200px]`) and support keyboard arrow navigation.
- **Form Asterisk Rule**: All mandatory fields include an asterisk in their label (`<Label>Nama Perusahaan <span className="text-destructive">*</span></Label>`).
- **Form Error Feedback**: Rendered via `<field.state.meta.errors>` using Zod error messages directly below input controls.

---

## 3. Geospatial Mapping (`mapcn`)

Built using `@mapcn/map` (installed via `bunx --bun shadcn@latest add @mapcn/map`, wrapping MapLibre GL):

- **`<MapPinDrop>`**: Interactive coordinate selector for Plotting and Survey.
  - Allows clicking or dragging a marker to update latitude and longitude coordinates.
  - Emits `(lng, lat)` to the parent TanStack Form field handler.
  - Supports search geocoding and zoom controls.
- **`<MapDirectoryView>`**: Read-only cluster and marker view for Directory and Peta pages.
  - Color-coded pins by stage (Prospek, Survey, A1, NOL, Selesai).
  - Popup tooltip showing company name, segment, sector, and deep-link button to `/companies/{id}`.
