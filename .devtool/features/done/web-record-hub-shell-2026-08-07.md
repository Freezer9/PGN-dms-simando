---
id: "web-record-hub-shell-2026-08-07"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-09T00:00:00.000Z"
completedAt: "2026-08-09T00:00:00.000Z"
labels: ["record-hub", "phase-1"]
order: "a00r"
---

# Web: Record hub shell

`/companies/{id}` shipped: header (name, Nomor, industry, location, sales rep, Area, Region), status card, 8-stage stepper (click a completed/current stage to switch tabs), 9-tab strip, and a real action bar — built together with `web-record-timeline-component`, `web-submit-for-approval-action`, `web-approval-action-bar-setujurevisitolak`, and `web-tetapkan-reviewer-action` in one pass, since the action bar's buttons ARE those cards and `WorkflowService` already implemented and tested all three transitions (`StartAsync`/`ActAsync`/`ChooseReviewersAsync`) earlier this session.

New: `Simando.Application/RecordHub/{CompanyDetail,ContactSummary,TimelineEntry,ICompanyDetailService}.cs`, `Simando.Infrastructure/RecordHub/CompanyDetailService.cs`, `Simando.Web/Components/Pages/CompanyHub.razor`. Also: `Simando.Application/Workflow/WorkflowLabels.cs` — `StepKindLabel`/`ActionLabel` moved out of `Tasks.razor` into a shared static class both pages and `CompanyDetailService`'s timeline now call.

**Deviations from the wireframe** (`docs/design/frontend/04-record-hub.md`):
- Only the **Ringkasan** tab has real content (header fields + read-only `CompanyContact` list). Plotting/Prospek/Survei/A1/NOL/Evaluasi/Penerbitan/Dokumen render `BbEmpty Title="Belum tersedia"` — their backing domain entities (Survey, A1, NolRequest, NolEvaluation, NolIssuance) don't exist yet, and Plotting/Prospek content specifically belongs to `web-record-hub-plotting-contacts-tabs-2026-08-07` (still backlog, not touched here).
- No document checklist on Ringkasan (no attachment model exists yet — `storage-iattachmentstore-s3attachmentstore-minio-2026-08-07`).
- No live cross-circuit push ("Record ini baru saja disetujui oleh...") — the page just reloads its own state after an action succeeds.
- No print stylesheet.
- Evaluasi tab requires `Capability.EditEvaluation`, Penerbitan requires `Capability.IssueNolRl` — the doc doesn't spell out the exact capability for per-tab *visibility* (only "RA only"/"DH only" prose), this is a reasonable reading of the existing capability matrix.

**Also fixed while in `WorkflowService.cs`** (user-requested, found during planning): `StartAsync` had zero authorization checks — any authenticated user reaching the page could submit any company by guessing its ID. Now checks creator identity + `Capability.SubmitForApproval` + scope, same shape as `ActAsync`/`ChooseReviewersAsync`, and returns a `SubmitResult` (new, mirrors `WorkflowActResult`) instead of throwing. Rippled through `IWorkflowService`, both existing test files' `StartAsync` call sites, plus 3 new authorization tests in `WorkflowServiceTests.cs`.

New tests: `tests/Simando.Integration.Tests/RecordHub/CompanyDetailServiceTests.cs` (7 tests).
