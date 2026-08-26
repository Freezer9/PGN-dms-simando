---
id: "frontend-multi-document-upload-ui-with-type-select-2026-08-05"
status: "backlog"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-05T09:00:00.000Z"
completedAt: null
labels: ["epic-documents", "frontend"]
order: "a1"
---

# Frontend multi-document upload UI with type selection, replacing single-file-per-stage model

Rework `DocumentUpload.razor` to support selecting a document type and uploading/managing multiple documents per stage, instead of the current single generic file slot.

## Context
Current `DocumentUpload.razor` filters to `Subscription.Submissions.Where(s => s.Stage == Subscription.Status)` with no type distinction. This card is the UI counterpart to the backend tagging card above, and is a dependency for epic-nol's A1/KK0 tagging requirement and epic-feasibility's summary upload.

## Acceptance Criteria
- [ ] Document type selector added to the upload flow
- [ ] List view groups/labels uploaded documents by type, not just by stage
- [ ] Existing upload size/extension restrictions preserved
