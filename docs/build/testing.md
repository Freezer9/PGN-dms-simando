# Build — Testing Strategy

Two pieces of logic in this system are worth more test effort than everything else
combined:

1. **The permission model** — it enforces commercial confidentiality between
   Areas and prevents self-approval of binding documents.
2. **The workflow state machine** — it has two counter-intuitive rules that
   reviewers will otherwise "fix".

Everything else is CRUD over forms. Both the equipment table's gas-demand figure
and the 24-hour load profile that used to sit here are manual entry / a document
upload, not computed — see [§2](#2-gas-demand-and-load-profile--manual-entry)
for what that leaves to test.

---

## 1. Test pyramid

| Layer | Scope | Tooling | Where |
|---|---|---|---|
| **Domain Unit** | Workflow transitions, permission resolution, `Nomor` rendering | xUnit, no I/O | `Simando.Domain.Tests` |
| **Backend Integration** | Controllers, Query filters, RLS, sequence allocation, document generation | xUnit + **Testcontainers** (real PostGIS + S3) + `WebApplicationFactory<Program>` | `Simando.Integration.Tests` |
| **Frontend Unit & Component** | TanStack Form schemas with Zod, custom hooks, TanStack Table filters | Vitest + React Testing Library | `frontend.tests` / `frontend/src/**/*.test.tsx` |
| **Smoke / E2E** | Sign-in → create → survey → submit → approve → issue | Playwright, full sales pipeline happy path | `Simando.E2E.Tests` |

The domain project has **no EF Core dependency**
([architecture](architecture.md#solution-structure)) precisely so layer 1
runs in milliseconds without a database. The workflow rules are the thing a
client will argue about — they must be demonstrable without spinning up
infrastructure.

---

## 2. Gas demand and load profile — manual entry

`Konversi ke Gas` is a plain field on `survey_equipment`, typed by the user,
not computed. There is no conversion function and no `conversion_factor`
table, so there's nothing here for the kind of fixture-driven engine testing
this section used to describe. The 24-hour load profile (Lampiran 17 §8) is
the same shape now — a document upload
([06-nol §8](../domain/06-nol.md#stage-6--permohonan-nol)), not a keyed grid,
so there is no `Laju Alir` aggregation left to test either. What's left is
ordinary form validation, plus one genuine piece of derived arithmetic.

### Equipment row — plain validation

| # | Case | Expected behaviour |
|---|---|---|
| E1 | `Konversi ke Gas` left blank | Rejected — required field, same as any other row value |
| E2 | Negative `Konversi ke Gas` | Rejected at validation |
| E3 | `JUMLAH KEBUTUHAN ENERGI` total | Live sum of every row's `Konversi ke Gas`, recomputes on each row edit or delete |

---

## 3. Permission model

Three gates — **scope + capability + turn**
([roles-permissions](../design/roles-permissions.md#1-the-permission-model)) —
plus stage as a practical fourth for editing.

### Scope

| # | Role | Record location | Expected |
|---|---|---|---|
| P1 | Sales Area @ Surabaya | Surabaya | visible |
| P2 | Sales Area @ Surabaya | Sidoarjo (same region) | **not visible** |
| P3 | Area Head @ Surabaya | Sidoarjo | **not visible** |
| P4 | Regional Admin @ SOR II | Sidoarjo | visible |
| P5 | Regional Admin @ SOR II | record in SOR III | **not visible** |
| P6 | Reviewer @ SOR II | any Area in SOR II | visible |
| P7 | System Admin | any | **not visible** — no case data |

P7 is easy to get wrong: "admin sees everything" is the reflex, and it is wrong
here ([roles-permissions §2.6](../design/roles-permissions.md#26-system-admin)).

### Scope must hold on every surface

The same record, the same user, across every read path — this is where leaks
actually happen:

| # | Surface | Assertion |
|---|---|---|
| P8 | Directory list | out-of-scope record absent |
| P9 | Global search by company name | absent |
| P10 | Global search by exact `Nomor` | absent |
| P11 | **Map bounding-box query** | absent |
| P12 | Every report | absent |
| P13 | Excel export | absent |
| P14 | Direct URL `/companies/{id}` | `/access-denied` |
| P15 | Attachment download by id | 403 |

**P11 deserves its own attention.** A map query is "give me everything in these
coordinates" — the one endpoint whose natural shape has no scope in it. An
unscoped version returns every prospect in Indonesia.

### Capability × turn

| # | Role | Record state | Expected |
|---|---|---|---|
| P16 | Sales Area (own area) | `DRAFT` | edit + submit |
| P17 | Sales Area | at Area Head | **read-only** |
| P18 | Area Head | at Area Head | approve/revise/reject, **no edit** |
| P19 | Area Head | at Regional Admin | read-only, **no action bar** |
| P20 | Area Head | at Reviewer 2 | visible, **no action** — past their endpoint |
| P21 | Reviewer 1 | at Reviewer 2 | **no action** — not their turn |
| P22 | Reviewer 2 | at Reviewer 2 | approve/revise/reject |
| P23 | Regional Admin | at Regional Admin | **edit *and* approve** — the only role with both |
| P24 | Division Head | at Division Head | issue NOL or RL |
| P25 | Division Head | at Reviewer 1 | no action |

### Authentication & account lifecycle

Now that identity is ours, these are our bugs to have:

| # | Case | Expected |
|---|---|---|
| A1 | Valid credentials | Signed in |
| A2 | Wrong password | Rejected, **same message and timing** as unknown user |
| A3 | Unknown username | Rejected, indistinguishable from A2 |
| A4 | Deactivated user, valid password | Rejected |
| A5 | `must_change_password` set | Every route redirects to `/change-password` |
| A6 | New password below `Auth:Password:MinLength` | Rejected |
| A7 | New password matches one of last `Auth:Password:HistoryCount` | Rejected |
| A8 | `Auth:Lockout:MaxAttempts` failures | Locked |
| A9 | Wait `Auth:Lockout:Minutes` | **Auto-unlocked** — no admin needed |
| A10 | Password stored | Hash only; plaintext appears in no column, log or audit row |
| A11 | Temporary password after admin reset | Forces change on first use |
| A12 | Admin resets own password | **Rejected** |
| A13 | Regional Admin creates user outside own region | **Rejected** |
| A14 | Regional Admin assigns `Division Head` | **Rejected** |
| A15 | User deactivated | Active sessions terminated; in-flight steps appear in `/tasks/blocked` |
| A16 | `/forgot-password` requested | **404** — the route must not exist |

A2/A3 are one test with two inputs: differing responses let an attacker enumerate
usernames. A10 is worth an explicit assertion because a debug log line is the usual
way plaintext escapes.

### System Admin boundaries

System Admin is a platform role, not a super user — the boundary only exists
if it is tested.

| # | Case | Expected |
|---|---|---|
| S1 | SYS opens `/companies/{id}` | `/access-denied` |
| S2 | SYS calls the record API directly | 403 |
| S3 | SYS downloads an attachment | 403 |
| S4 | SYS attempts any workflow action | **Rejected — no break-glass path exists for approval** |
| S5 | SYS grants themselves an approver role, then acts | Role assignment allowed; **action still rejected** (self-modification blocked at A12/P29) |
| S6 | SYS opens `/admin/stuck-steps` | Company name, step, assignee — **no survey, price or document field in the payload** |
| S7 | SYS reassigns from that view | Succeeds; `REASSIGN` audited |
| S8 | SYS requests break-glass without a reason | Rejected |
| S9 | Break-glass granted | Read-only on **one** record; write and approve still 403 |
| S10 | Break-glass after 60 minutes | Expired, access revoked |
| S11 | Break-glass granted | `BREAK_GLASS` event on the record timeline |
| S12 | Regional Admin views the record afterwards | **Sees that break-glass occurred, by whom and why** |
| S13 | Seed account after a real admin exists | Deactivated (go-live check) |

S6 is worth asserting on the **serialised payload**, not the rendered page — the
usual way this leaks is an over-fetched DTO carrying fields the UI never shows.

S12 is the whole point of the mechanism: an admin with database credentials can
read anything regardless, so the control is not prevention but **visibility to the
business**.

### Segregation of duties

| # | Case | Expected |
|---|---|---|
| P26 | User is both creator and assigned Reviewer | **Action rejected server-side** |
| P27 | Reviewer picker excludes the record's creator | Creator absent from list |
| P28 | User edited stage 7, then assigned as approver | Rejected |
| P29 | User modifies own role assignment | Rejected |
| P30 | Regional Admin assigns `Regional Admin` role | Rejected |
| P31 | Regional Admin assigns `Sales Area` in own region | Allowed |
| P32 | Regional Admin assigns role in another region | Rejected |

P26 is the control that stops one person driving a NOL end to end. Test it at the
**service layer**, not through the UI — the UI hiding the button is not the control
([roles-permissions §6](../design/roles-permissions.md#6-enforcement-checklist)).

---

## 4. Workflow state machine

Two rules here are counter-intuitive and specified by the client. Tests exist so
that a future developer "correcting" them fails the build.

| # | From | Action | Expected |
|---|---|---|---|
| W1 | `DRAFT` | submit | → Area Head; chain **snapshotted** |
| W2 | Area Head | setuju | → Regional Admin |
| W3 | Regional Admin | setuju | → Reviewer 1 |
| W4 | Reviewer 1 | setuju | → Reviewer 2 |
| W5 | last Reviewer | setuju | → Division Head |
| W6 | Division Head | setuju | → `ISSUED_NOL` |
| W7 | Division Head | tidak layak | → `ISSUED_RL` |
| W8 | Reviewer 2 | revisi | → Reviewer 1 (**exactly one step back**) |
| W9 | Area Head | revisi | → `DRAFT`, back to creator |
| W10 | **Reviewer 2** | **tolak** | **→ Regional Admin, not the creator** |
| W11 | **Division Head** | **tolak** | **→ Regional Admin** |
| W12 | Regional Admin | tolak | → own queue, still recorded |
| W13 | any | revisi with empty comment | **rejected** |
| W14 | any | tolak with empty comment | **rejected** |
| W15 | `ISSUED_NOL` | any action | rejected — terminal |
| W16 | 2-reviewer chain | setuju at Reviewer 2 | → Division Head, skips slot 3 |

**W10 and W11 are the ones to guard.** Reject routes *sideways* to Regional Admin
rather than back to the submitter
([approval-workflow](../design/approval-workflow.md#the-three-transitions)).
Every developer's instinct is "reject = return to sender".

### Snapshotting

| # | Case | Expected |
|---|---|---|
| W17 | Template edited after submission | In-flight record keeps its original chain |
| W18 | Reviewer deactivated mid-flight | Step appears in `/tasks/blocked` |
| W19 | Step reassigned | `REASSIGN` event written; completed steps untouched |
| W20 | `status_event` `UPDATE` attempted | **Rejected by database trigger** |

---

## 5. Other high-value cases

### `Nomor`

| # | Case | Expected |
|---|---|---|
| N1 | Format | `0000042-35-78` — 7-digit seq, BPS prov, BPS regency |
| N2 | Global sequence across regions | `…-35-78` then `…-32-73` continue one series |
| N3 | 100 concurrent creations | 100 distinct numbers, no duplicates |
| N4 | Regency changed while `DRAFT` | Suffix re-rendered, `nomor_seq` unchanged |
| N5 | Regency changed after leaving `DRAFT` | `Nomor` **unchanged** |
| N6 | Rolled-back insert | Gap allowed, no duplicate |

N3 is the one that justifies the `SEQUENCE` choice — run it against real
PostgreSQL in Testcontainers, not a mock.

### Stage gates

| # | Case | Expected |
|---|---|---|
| G1 | Advance to A1 without signed KK0 | Blocked, reason names the document |
| G2 | Advance to NOL without A1 + bukti kelayakan | Blocked |
| G3 | `Skema = SiGas` without MOM upload | Blocked |
| G4 | `Skema = Reguler`, no MOM | Allowed |
| G5 | Gate bypassed via direct API call | **Rejected server-side** |
| G6 | Re-uploaded signed file differs from the generated `.docx` | **Accepted** — editing before signing is expected |
| G7 | Signed document re-uploaded a second time | Supersedes; prior version still retrievable, `status_event` records both |

G5 matters: the UI disabling a button is a courtesy, the endpoint is the control.

### Notification

| # | Case | Expected |
|---|---|---|
| T1 | Any transition | In-app notification created for the next actor |
| T2 | Email channel | **Not dispatched** while disabled |
| T3 | Badge count | Counts only steps where it is this user's turn |
| T4 | Enabling the email channel | Dispatches without other code changes |

T4 protects the seam: email is built and switched off, so turning it on later must
not require touching the workflow service.

### Documents

| # | Case | Expected |
|---|---|---|
| D1 | Generated `.docx` opens in Word | Valid OOXML |
| D2 | Header block renders | `No. Dok. \| Revisi \| Tgl. Berlaku \| Hal.` present |
| D3 | Repeating periods | N rows render, not just the first |
| D4 | Template replaced with a new version | Old records regenerate with the **old** template |

### Money and units

| # | Case | Expected |
|---|---|---|
| M1 | All monetary fields | `decimal`, never `double` |
| M2 | Amount without currency | Rejected at the type level |
| M3 | `id-ID` parse of `1.234.567,89` | 1234567.89 |

No IDR/USD conversion case — capex and gas price are both typed directly by
the user (`harga_nilai`/`harga_currency`), and IRR/NPV/Payback are manual
entry too, so nothing in the system mixes currencies.

---

## 6. What not to test heavily

Being explicit, so effort lands where it matters:

- **CRUD on master data.** One happy path each; the framework handles the rest.
- **Blazor rendering details.** Test permission gating and repeating-row behaviour;
  not markup.
- **Third-party libraries.** Trust the SDKs (Open XML, ClosedXML, the S3 client)
  to do their job; test our usage of them, not their internals.
- **Exhaustive E2E.** One smoke path end to end. E2E suites that mirror unit tests
  are slow and get disabled.

---

## 7. Coverage targets

| Area | Target | Rationale |
|---|---|---|
| Workflow transitions | **100 % branch** | Counter-intuitive rules, commercial consequence |
| Permission resolution | **100 % branch** | Confidentiality and self-approval |
| `Nomor` allocation | 100 % | Uniqueness on signed documents |
| Everything else | ~60 % line | Diminishing returns |

A blanket percentage across the solution would be met by testing form bindings
while leaving the workflow's counter-intuitive rules untested. Target the
three things that hurt.

---

## 8. Pre-go-live checks

Beyond automated tests — the seeding gaps
([master-data §13](../domain/master-data.md#13-seeding-checklist)) are as
likely to break go-live as any bug:

- [ ] All six document templates packaged with the release, including Lampiran 16
      (🚧 not yet supplied by PGN — [master-data §13](../domain/master-data.md#13-seeding-checklist))
- [ ] Document numbering format for KK0 and Nota Dinas confirmed with PGN — ships
      with a best-guess default otherwise
      ([master-data §9](../domain/master-data.md#format-penomoran-dokumen--not-master-data))
- [ ] Every Area has an Area Head, and every Region has a Regional Admin, a
      Division Head and at least two Reviewer-capable users — a role gap here
      blocks every submission that reaches it (there is no `workflow_template`
      to seed instead — [master-data §10](../domain/master-data.md#workflow-template--not-master-data))
- [ ] Every intended user has an account, a role, a scope and a temporary password
- [ ] No account still has `must_change_password` set on go-live day
- [ ] Password policy constants agreed with PGN
- [ ] Someone at PGN owns the leaver process — revocation is manual now
- [ ] **Seed/bootstrap admin account deactivated** once a real System Admin exists
- [ ] No System Admin account also holds an approver role
- [ ] No user holds both creator and approver roles on any live record
- [ ] Restore from backup **tested**, not just configured
- [ ] A real KK0 and a real NOL generated and reviewed by PGN against the paper form
