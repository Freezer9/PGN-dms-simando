# Frontend 12 — Patterns & States

Cross-cutting behaviour. Getting these consistent matters more than any individual
screen.

---

## Loading, empty, error

### Loading

- **Skeletons** for tables and cards, not spinners — the shape of what's coming
  reduces perceived wait
- Blazor Server renders fast; only show a loading state past ~300 ms, otherwise it
  flickers
- Long operations (document generation, Excel export) get a determinate progress
  indicator and stay cancellable

### Empty

Empty states say what to do, not just that nothing is there:

| Context | Message |
|---|---|
| No tasks | *"Tidak ada yang menunggu persetujuan Anda."* |
| No records match filters | *"Tidak ada perusahaan yang cocok dengan filter."* + `( Reset filter )` |
| Empty directory | *"Belum ada perusahaan di area ini."* + `[ + Tambah ]` |
| No permission | *"Anda tidak memiliki akses ke data ini."* — never a blank page |

No illustrations. This is a tool people use all day.

### Error

| Kind | Handling |
|---|---|
| **Validation** | Inline, next to the field, on blur and on submit |
| **Permission** | Explain the scope: *"Record ini berada di Area Sidoarjo, di luar lingkup Anda."* |
| **Conflict** | See [concurrent edits](#concurrent-edits) |
| **Server** | Toast with a reference id for support; never a raw stack trace |
| **Network error** | TanStack Query retry toast / offline banner: *"Koneksi terputus. Mencoba menyambungkan kembali…"* |

---

## Validation

Two tiers, matching the two stage gates
([02-process-flow](../../domain/02-process-flow.md#stage-gate-rules)):

**Field-level** — type, format, range. Immediate, on blur.

**Gate-level** — checked on stage advance and on submit. Never blocks typing or
saving a draft; a half-filled KK0 must always be saveable.

```
┌──────────────────────────────────────────────────────────────────────────────────┐
│  ⚠️ Belum dapat diajukan:                                                         │
│     • Dokumen "Bukti Kelayakan" belum diunggah                                    │
│     • PIC Perusahaan: Jabatan wajib diisi                                         │
│     • Peralatan baris 4: Konversi ke Gas wajib diisi                              │
│                                                    ( Lihat detail )               │
└──────────────────────────────────────────────────────────────────────────────────┘
```

- Every gate failure names the **specific** field or document, and links to it
- The submit button stays visible but disabled, with the reason shown — a silently
  dead button generates support calls
- **Enforced server-side regardless.** The UI list is a courtesy

---

## Autosave & drafts

The KK0 survey takes 20+ minutes; the NOL request spans several sittings.

- **Autosave every ~30 s and on field blur** while status is `DRAFT`
- Show the state plainly: `Disimpan otomatis 14:32` / `Menyimpan…` /
  `⚠️ Gagal menyimpan — coba lagi`
- Never autosave a record that is **not** `DRAFT` — records lock on submit
- Warn on navigation with unsaved changes

Autosave does **not** advance the stage. Stage advance is always explicit.

---

## Concurrent edits

Two Sales Area users can open the same record — they share an Area.

**Optimistic concurrency on `xmin`.** On conflict:

```
┌────────────────────────────────────────────────────────────────────┐
│  ⚠️ Record ini telah diubah oleh Rina A. pada 14:35                 │
│                                                                    │
│  Perubahan Anda belum tersimpan. Pilih tindakan:                   │
│                                                                    │
│  ( Muat ulang — perubahan saya dibuang )                           │
│  ( Lihat perbandingan )                                            │
│  [ Timpa dengan perubahan saya ]                                   │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘
```

Never silently last-write-wins on a record that becomes a signed document.

**Live workflow updates.** If another user actions the record while it is open,
the status card, stepper and timeline update in place with a toast:
*"Record ini baru saja disetujui oleh Andi P."* If the current user was mid-edit
on a now-locked record, switch to read-only and say why.

---

## Permission-aware rendering

Three rules, in order of importance:

1. **Hide what is out of scope.** An Area Head has no reason to see an empty
   *Penerbitan* tab.
2. **Disable with a reason where the user might reasonably expect to act.** A
   disabled submit button with *"Dokumen belum lengkap"* teaches; a hidden one
   confuses.
3. **Read-only is the default rendering.** Four of six roles never edit anything
   ([roles-permissions](../roles-permissions.md#3-capability-matrix)). Build the read-only
   view first and layer editing on top.

Read-only mode renders values as text, not as disabled inputs — a page of greyed
boxes is harder to read than plain values.

---

## Responsive

**Desktop-first.** All roles work at desks; the forms are dense by necessity.

| Breakpoint | Behaviour |
|---|---|
| ≥ 1280 px | Full layout, right rail visible |
| 1024–1280 px | Right rail collapses to a toggle |
| 768–1024 px | Sidebar collapses to icons; tables scroll horizontally |
| < 768 px | **Only two flows are supported** |

The two mobile-supported flows, both field activities:

- **Pin-drop** on the company form — a rep standing at the plant, with
  *Gunakan lokasi saya*
- **Attachment upload** — photographing a signed KK0 rather than scanning it

Everything else degrades to a readable but not comfortable layout. Do not spend
effort making the KK0 form pleasant on a phone; it is filled on paper at the site
and transcribed at a desk.

---

## Formatting

| Type | Format | Note |
|---|---|---|
| Numbers | `1.234.567,89` | `id-ID` — `.` thousands, `,` decimal |
| Dates | `18/08/2026` | Per Lampiran 11 |
| Date + time | `18/08/2026 14:32` | 24-hour |
| Currency IDR | `Rp 3.100.000.000` | No decimals |
| Currency USD | `USD 9,18` | Two decimals |
| Gas volume | `116.667 m³/bln` | Unit always shown |
| BBTUD | `45,00 BBTUD` | Two decimals |
| Durations | `11 hari`, `2,4 hari` | Days, one decimal for averages |
| Relative time | `2 jam lalu`, `3 hari lalu` | Timeline and notifications only |

**Timezone:** store UTC (`timestamptz`), display **WIB** by default. If PGN
operates across WIB/WITA/WIT, display in the record's Area timezone and label it.

---

## Accessibility

Not a compliance exercise — this is a tool used for hours a day.

- **Keyboard-navigable throughout.** Repeating-row tables especially: `Tab` across,
  `Enter` to add a row. Data entry staff will not reach for the mouse.
- Visible focus rings; never `outline: none`
- Colour is never the sole signal — status icons (✅/🔴/◐) always carry a label
  or text alongside them, never colour alone
- Form labels properly associated; required fields marked in text as well as with
  `*`
- Sufficient contrast, particularly for the `{{ computed }}` grey — it is
  meaningful content, not decoration
- Live regions announce autosave and workflow updates for screen readers

---

## Performance

Modest scale — thousands of companies, dozens of concurrent users — so the
priorities are narrow:

- **Paginate everything.** Default 25 rows; never load a full directory.
- **Cluster map pins** above ~200 in view.
- **The map endpoint must be scoped and bounded** — an unscoped bounding-box query
  is both a performance problem and a data leak
  ([roles-permissions §6](../roles-permissions.md#6-enforcement-checklist)).
- **Debounce** typeahead search at ~300 ms.
- **Blazor Server:** keep circuit state small. Don't hold a full directory in
  component state; page from the server.
- **Sticky sessions** at the reverse proxy if more than one app instance runs —
  circuits are stateful
  ([architecture](../../build/architecture.md#deployment--self-hosted)).
