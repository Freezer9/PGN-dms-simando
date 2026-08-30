---
id: "add-capex-pre-gr3-fields-document-tagging-a1-kk0-t-2026-08-05"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-30T00:00:00.000Z"
completedAt: "2026-08-30T00:00:00.000Z"
labels: ["epic-nol", "backend", "data-model"]
order: "aQ"
---
# Add Capex Pre-GR3 fields + document tagging (A1, KK0) to NOL application

Add the Capex Pre-GR3 cost breakdown (Biaya Penyambungan Reguler/Extra/Jumlah) to the NOL application, and require/support tagging uploaded documents as "A1" or "KK0" references.

## Context
Spec shows Capex Pre-GR3 with an upload area and a checklist referencing "1. A1" and "2. KK0" as required source documents. This depends on the structured document upload work (epic-documents) for the tagging mechanism.

## Acceptance Criteria
- [x] `BiayaPenyambunganReguler`, `BiayaPenyambunganExtra`, `Jumlah` (computed) fields added to `NolApplication`
- [x] Upload flow requires/allows tagging the Capex Pre-GR3 upload against A1 and/or KK0 source documents
- [x] Depends on `epic-documents` structured upload work

## Depends On
- **Add DocumentType tagging to uploads** (epic-documents) — provides the tagging mechanism this card uses to reference A1/KK0 source documents.
