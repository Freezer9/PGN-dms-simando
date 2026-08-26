---
name: master-data-crud-screen
description: >
  Playbook for building an admin CRUD Blazor page for a master-data/lookup
  entity in this repo (Simando). Use when asked to add a new /master/*
  admin screen, wire up CRUD for an AuditableEntity, or "build the X admin
  page" for master/config data. Captures the reusable architecture
  (AuditableEntity, IEntityService<T>, AdminPageBase), UX conventions, and
  BlazorBlueprint component gotchas discovered while building the first two
  screens (Segments, FuelTypes) — so the remaining screens don't re-derive
  or re-hit the same issues.
---

# Master-data CRUD screen playbook

Built once (Segments, FuelTypes), reused for every other master-data admin
screen. Read this before starting a new one — most of the plumbing already
exists; the work is almost always just the checklist below, not new
architecture.

## Architecture already built — reuse, don't rebuild

- **`AuditableEntity`** (`src/Simando.Domain/Common/AuditableEntity.cs`) —
  `Id`/`CreatedAt`/`DeletedAt`. Entities extend it and only declare their
  own fields.
- **`AuditableEntityConfiguration<TEntity>`**
  (`src/Simando.Infrastructure/Persistence/Configurations/AuditableEntityConfiguration.cs`)
  — maps Id/CreatedAt/DeletedAt + the soft-delete query filter once.
  Concrete configs override `ConfigureEntity(builder)` for just `ToTable`
  and entity-specific columns/indexes.
- **`IEntityService<TEntity>`** (`src/Simando.Application/Common/`) — what
  Razor pages inject. `GetAllAsync`, `GetPagedAsync`, `AddAsync`,
  `UpdateAsync(id, mutate)`, `SoftDeleteAsync`, `RestoreAsync`. Backed by
  `Repository<TEntity>` (Infrastructure), generically DI-registered
  (`AddScoped(typeof(IEntityService<>), typeof(EntityService<>))` in
  `Infrastructure/DependencyInjection.cs`) — **a new entity type needs zero
  new DI registration.**
- **`AdminPageBase`** (`src/Simando.Web/Components/AdminPageBase.cs`) —
  handles `CurrentUser.LoadAsync` + capability gate (redirect to
  `/access-denied`) + breadcrumb, once. Pages `@inherits AdminPageBase` and
  override `RequiredCapability`, `Breadcrumbs`, `OnAuthorizedInitializedAsync`.

## Checklist for a new entity's screen

1. Confirm the entity already extends `AuditableEntity` and has an
   `AuditableEntityConfiguration<T>` subclass. All 12 master-data/geography
   entities from the phase-0 pass already do; verify before assuming for
   anything newer.
2. New file `src/Simando.Web/Components/Pages/Master/<Entity>s.razor`:
   ```razor
   @page "/master/<route>"
   @inherits AdminPageBase
   @inject IEntityService<TheEntity> Service
   @inject DialogService DialogService   @* only if delete needs confirmation *@
   @inject ToastService ToastService
   ```
   Override:
   - `RequiredCapability` — almost always `Capability.ManageMasterData`.
   - `Breadcrumbs` — middle crumb label matches the entity's `NavGroup` in
     `NavigationMenuBuilder.BuildAdminSection`
     (`src/Simando.Application/Navigation/NavigationMenuBuilder.cs`).
   - `OnAuthorizedInitializedAsync` → calls the page's own `LoadAsync()`.
3. `BbDataTable` columns: the identifying column (Nama, or equivalent) gets
   `Width="100%"`; small fixed-value columns (Urutan, a code, etc.) get a
   small fixed `Width` (e.g. `"80px"`) and go **first**; Aksi goes **last**
   with a small fixed width (e.g. `"90px"`).
4. **No "show deleted" UI.** Hapus soft-deletes; the row just disappears
   from the list. Nothing restores it from the UI — the admin checks the
   DB directly if a restore is ever needed. Don't add a status
   column/badge/toggle back in unless explicitly asked; this was tried and
   deliberately removed.
5. Card subtitle: one sentence on the entity's **purpose** (what it's for,
   where it's used) — never literal seed values or source-document trivia
   copied from `docs/domain/master-data.md`. Those go stale the moment a
   row is added via the very screen describing them, and just duplicate
   what the table already shows.
6. `SaveAsync`: catch `Simando.Application.Common.DuplicateNameException`
   (not `DbUpdateException` — that's now translated at the repository
   boundary, EF types don't reach the page) for the unique-name error
   message, worded per-entity ("Nama segmen sudah digunakan", etc.).
   Upon successful save/update, call `ToastService.Success(isEdit ? "<Entity> berhasil diperbarui." : "<Entity> berhasil ditambahkan.", "Berhasil");`.
7. `DeleteAsync`: Upon successful soft-delete confirmation, call `ToastService.Success($"<Entity> \"{entity.Name}\" berhasil dihapus.", "Berhasil");`.

## Data layer / DbContext rules

- Pages never inject `SimandoDbContext` directly, except
  `CurrentUser.LoadAsync(Db)` inside `AdminPageBase` itself — that's the
  one standing exception, documented in `AGENTS.md`. New data access goes
  through `IEntityService<T>`/`IRepository<T>`, not `Db` directly.
- `SimandoDbContext` is registered via
  `AddDbContextFactory<SimandoDbContext>(..., lifetime: ServiceLifetime.Scoped)`,
  not `AddDbContext` — Blazor Server's "scoped" lifetime means scoped to
  the whole circuit (browser-tab session), not to a component or an
  operation. `Repository<T>` opens and disposes its own context per
  method call for exactly this reason. Don't reintroduce a
  directly-injected, long-lived `SimandoDbContext` into new code.
- **Large tables must not use `GetAllAsync()`.** `Village` (~83k rows),
  `District` (~7k), `Regency` (~500) need `GetPagedAsync(page, pageSize,
  orderBy, filter)` with `filter` scoped to the parent (e.g. Village
  filtered by `DistrictId`) once those screens get built. Small
  enum-like tables (Segment, FuelType, Country, IndustryType, MeterSize,
  MrsSpec, ReasonCategory, UnitOfMeasure, Province) are fine with the
  unbounded `GetAllAsync()` + `BbDataTable`'s client-side paging, same as
  today's two screens.

## BlazorBlueprint gotchas hit so far

- **Don't conditionally mount/unmount a whole `BbDataTableColumn` across
  renders** (e.g. `@if (toggle) { <BbDataTableColumn>...</BbDataTableColumn> }`).
  `BbDataTable` leaks column registrations when a column child
  mounts/unmounts repeatedly — toggling N times produced N duplicate
  headers in practice. Put conditional content inside a `CellTemplate` on
  a column that's always present instead.
- **`Property` and `CellTemplate` can coexist** on the same
  `BbDataTableColumn` — `Property` still drives sorting, `CellTemplate`
  overrides the rendered cell. Useful for muted/annotated text without
  losing sortability.
- **`<BbBreadcrumbList AutoSeparator>` didn't render separators** when
  items came from a `@for` loop (visually confirmed — no chevron between
  crumbs). No logged bug for it, but it's the same shape of issue as the
  column leak above (self-registration across dynamically generated
  children). Use manual separators instead — documented as an equally
  supported pattern:
  ```razor
  @for (var i = 0; i < items.Count; i++)
  {
      @if (i > 0) { <BbBreadcrumbSeparator /> }
      <BbBreadcrumbItem>...</BbBreadcrumbItem>
  }
  ```
- Razor collapses whitespace between `@expression` and a following element
  on the next line — don't rely on a literal space for inline spacing
  (e.g. `@entity.Name` then `<span>(dihapus)</span>` renders with no gap).
  Use a margin utility class (`ml-1`) instead.

## Working norms (see also project auto-memory)

- `dotnet build Simando.slnx` clean is the verification bar for a small
  change. Don't spin up `dotnet run` + Playwright unless explicitly
  asked — the user verifies UI changes himself.
- Commit only when explicitly asked. Message style: single-line subject line only following Conventional Commits (`type(scope): description`), no commit body.
- Plan mode on this project tends to run several rounds of `ExitPlanMode`
  rejection before approval — each rejection has consistently been a real
  missing requirement (a transaction primitive, pagination, a scoping
  question), not a stylistic nitpick. Verify claims about framework
  behavior against real docs rather than guessing, and update the plan
  file incrementally rather than starting over each round.
