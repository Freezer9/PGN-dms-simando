# Future — Not Planned for v1

Six things the client has touched on but never confirmed as requirements. None
of them block v1. Each has a cheap seam already in the v1 schema so that
building the real thing later is a migration-free addition, not a rewrite —
that seam is listed under **v1 seam** below and is the only trace of these
features in the rest of the docs.

Nothing here is designed in any depth. If PGN confirms one of these, it needs
real design work before it's built — treat this page as "worth asking about",
not a spec.

---

## Amendment / extension workflow

Existing customers renewing or amending an agreement, rather than registering
fresh. The Resume Evaluasi has a full `#Untuk Pelanggan` branch for it
(*Perubahan Perjanjian*, *Perpanjangan Perjanjian*, review of the last 6
months' usage), and Lampiran 15/16/17 all read *"Registrasi Berlangganan Gas
**/ Amendemen Perjanjian**"* — so the official forms support it, but the
client meeting never discussed it as a requirement.

**v1 seam:** `nol_request.registration_type` (`registrasi_baru` | `amendemen`
| `perpanjangan`), defaulting to `registrasi_baru`. Cheap to add now; a later
migration on a table holding signed documents is not.

## Daily contract basis (`Harian`)

`Harian` appears in `Entry Apps` and in Lampiran 15/16/17 alongside `Bulanan`
and `Tahunan`, and needs a 7-row Senin–Minggu min/max table instead of a
single figure. But **Lampiran 11 (A1) offers only `Bulanan` and `Tahunan`** —
the one place `Harian` would originate from doesn't offer it, which suggests
it may not actually be in use.

**v1 seam:** `nol_request_daily` (7 rows) exists in the schema; the UI stays
hidden unless `basis_kontrak = harian`, which nothing in v1 sets.

## Two-scenario feasibility comparison

Resume Evaluasi §6 shows **two price columns side by side**, each with its
own IRR, Payback and Hasil Analisis — a feasibility comparison the client's
worksheet only models once.

**v1 seam:** `nol_evaluation_scenario` is already a child table, not a flat
set of columns, so v1 rendering one scenario and a future second scenario is
a UI change, not a migration. (Also: the official resume prints IRR and
Payback only, no NPV, while the worksheet has NPV — NPV is captured as an
internal field but never printed, regardless of scenario count.)

## Gate Review integration

The NOL depends on Gate Review outputs — Total Nilai Capex, pipe specs,
G-Size MR/S, Durasi Pelaksanaan, Maksimum Kapasitas Meter — and one docx
annotation reads *"Otomasi Dari Data Gate Review"*, implying some kind of
integration exists or is planned.

**v1 seam:** all Gate Review fields are manual entry, with `gate_review_date`
and an attachment slot for the Gate Review document, behind an
`IGateReviewSource` interface with a `ManualGateReviewSource` implementation
— so a real integration can be added without touching the evaluation form.

## Approver delegation (out-of-office cover)

A chain of five sequential approvers stalls the first time someone takes
planned leave. Regional Admin can already reassign a stuck step after the
fact as an emergency recovery; delegation would let someone arrange cover
*in advance* instead. Nobody has asked for this — it's a risk the approval
chain's own shape creates, not a stated requirement.

**v1 seam:** `acted_on_behalf_of` ships now on the audit trail, nullable and
unwritten. It costs nothing today and is painful to retrofit into an audit
trail that already has signed documents hanging off it — you cannot honestly
backfill who *really* approved something.

## OneDrive attachment storage

PGN intends to keep attachments in OneDrive, but there's no tenant access yet
— no confirmation of personal drive vs. SharePoint document library, no app
registration, no admin consent, no target drive id. See
[build/storage §8](../build/storage.md#8-what-we-need-from-pgn) for the
full list of what's needed from PGN before this can start.

**v1 seam:** attachments run entirely on S3-compatible storage (MinIO) behind
`IAttachmentStore`. `OneDriveAttachmentStore` stays unimplemented behind the
same interface; switching to it later is a configuration change
(`Storage:Type`), not a data migration — see
[build/storage](../build/storage.md).

Also unresolved once OneDrive access exists: which retention policy applies
to the target library. If PGN's tenant expires or archives items after N
years, that would apply to signed NOLs too, which contradicts this system's
indefinite-retention assumption — worth confirming before go-live on
OneDrive, not after.
