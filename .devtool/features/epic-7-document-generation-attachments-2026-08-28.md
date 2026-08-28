---
id: "epic-7-document-generation-attachments-2026-08-28"
status: "backlog"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-28T00:00:00.000Z"
modified: "2026-08-28T00:00:00.000Z"
completedAt: null
labels: ["backend", "frontend", "documents", "attachments", "s3"]
order: "a6"
---

# Epic 7: Document Generation & Attachment Management

Provide document generation (.docx template-merge for 6 official Lampiran) and secure file upload/download integration with S3-compatible storage.

## User Stories & Scope

- [ ] **Story 7.1:** Backend Document Download Endpoints (`GET /api/documents/company/{id}/{docType}`: KK0, A1, Permohonan NOL, Evaluasi, Surat Penerbitan Docx).
- [ ] **Story 7.2:** Backend Attachment API (`POST /api/attachments/upload` multipart, `GET /api/attachments/{id}/download`, `DELETE /api/attachments/{id}`).
- [ ] **Story 7.3:** Frontend Document & Attachment Components: File upload dropzone with progress bar, mime-type validation, download triggers, and Word template download buttons on each stage tab.

## Acceptance Criteria

1. Document endpoints generate valid .docx files matching official Lampiran templates populated with company & stage data.
2. File uploads validate mime types, enforce size limits, and persist metadata and S3 blob keys cleanly.
3. Attachment downloads stream securely through authenticated API endpoints respecting RLS and capabilities.
