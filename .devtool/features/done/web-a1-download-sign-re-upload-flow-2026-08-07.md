---
id: web-a1-download-sign-re-upload-flow-2026-08-07
status: done
priority: high
assignee: null
dueDate: null
created: '2026-08-07T09:00:00.000Z'
modified: '2026-08-09T11:00:00.000Z'
completedAt: '2026-08-09T11:00:00.000Z'
labels:
- a1
- phase-4
- attachments
order: a013
---


# Web: A1 download → sign → re-upload flow

One download → edit if needed → sign outside the system (wet or digital, doesn't matter which) → re-upload loop, the same pattern every signed document in the system uses. Records `signature_method` as declared metadata only — never verified. Re-uploaded file must NOT be validated against the generated content (it's supposed to differ).
