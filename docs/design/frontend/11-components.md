# Frontend 11 — Component Library

Shared Blazor components. Building these first is what keeps eight stage forms
consistent instead of eight variations on the same idea.

---

## Status & Workflow

### `<BbStatusBadge>` / Status Chip
Renders record status with standardized colors, icons, and themes.

### `<BbStatTile>`
Unified KPI stat card component used on all role dashboards (`SalesAreaDashboard`, `RegionalAdminDashboard`, `ApproverDashboard`, `SystemAdminDashboard`). Takes `Label`, `Value`, `Subtext`, `Icon`, and optional styling classes.

### `<StageStepper>`
Eight stages with completed (`bg-emerald-600` checkmark), current (pulsing ring), and pending states.

---

## Component Selection Guidelines (Form Controls)

To ensure predictable UX across forms and master data screens:

- **`<BbSelect>` (Native Select)**:
  - **When to use**: Short, static option cardinality (< 10 items) where search is unnecessary.
  - **Examples**: `Tahap`, `Posisi Pelanggan`, `Kawasan`, `SortMode`, `UnitDimension`.
- **`<BbCombobox>` (Searchable Select)**:
  - **When to use**: High-cardinality data sets, dynamic database lookups, or option lists > 10 items requiring text filtering.
  - **Examples**: `Provinsi`, `Kota/Kabupaten`, `Kecamatan`, `Kelurahan/Desa`, `Jenis Produksi`, `Reassign User`.
  - **Behavior**: Trigger buttons automatically truncate overflow text (`white-space: nowrap; overflow: hidden; text-overflow: ellipsis;`).
- **Form Asterisk Rule**: All mandatory fields include an asterisk (`*`) in their label (`Label="Nama *"`).
