---
id: "add-documenttype-tagging-to-uploads-a1-kk0-mom-pri-2026-08-05"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-30T00:00:00.000Z"
completedAt: "2026-08-30T00:00:00.000Z"
labels: ["epic-documents", "backend", "data-model"]
order: "a0"
---
# Add DocumentType tagging to uploads (A1, KK0, MOM, pricing/connection-fee/payment-guarantee/land-lease reference docs, competitor data)

Extend the upload/submission model with a document-type tag, replacing the current "one arbitrary file per workflow stage" model with structured, multi-document, typed uploads.

## Context
`SubmissionRecord` (via `POST /api/subscriptions/{id}/upload`) currently stores one file per stage with no type metadata. Spec calls for multiple distinctly-tagged uploads at various points: MOM (Penetapan Harga SiGas), A1/KK0 source docs for Capex Pre-GR3, reference docs (Ketentuan Produk-Sub Produk/Harga Gas/Biaya Penyambungan/Jaminan Pembayaran/Sewa Lahan), and competitor data.

## Acceptance Criteria
- [x] `DocumentType` enum/lookup added, and `SubmissionRecord` gets a `DocumentType` field
- [x] Multiple documents per stage supported (remove the implicit one-per-stage constraint in `DocumentUpload.razor:35`'s filtering logic)
- [x] Existing upload endpoint extended to accept a document type on upload

## Blocks
- **Add Capex Pre-GR3 fields + document tagging (A1, KK0)** (epic-nol) — needs the DocumentType tagging mechanism to reference A1/KK0 source documents.
- **Frontend feasibility results display + upload** (epic-feasibility) — needs typed upload for the feasibility-summary document.
