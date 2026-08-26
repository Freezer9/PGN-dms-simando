---
id: "frontend-pic-contact-form-namejabatanemailhplinked-2026-08-05"
status: "backlog"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-05T09:00:00.000Z"
completedAt: null
labels: ["epic-pic", "frontend"]
order: "a8"
---

# Frontend PIC contact form (name/jabatan/email/HP/LinkedIn/IG/FB, up to 2 contacts)

Build the PIC contact section of the registration form in Blazor.

## Context
No contact-entry UI exists today. This is a new form section added to wherever the Subscription/A1 form lives (likely `Frontend/Components/Pages/Subscriptions/Detail.razor` or a new dedicated component).

## Acceptance Criteria
- [ ] Repeatable contact block (add/remove) rendering Nama/Jabatan/Email/No HP/LinkedIn/IG/FB fields
- [ ] Client-side validation for email format and required Nama/Jabatan (marked `*` in spec)
- [ ] Wired to the new PIC CRUD endpoints via a typed service (following the `ISubscriptionService` pattern)
