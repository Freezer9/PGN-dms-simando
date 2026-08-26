---
id: "email-link-based-review-action-magic-link-for-revi-2026-08-05"
status: "backlog"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-05T09:00:00.000Z"
completedAt: null
labels: ["epic-approval", "backend", "frontend"]
order: "aL"
---

# Email-link based review action (magic link) for reviewers

Support reviewing and acting on a subscription via a link sent by email, per the spec's "Review 1: PIC Area Support (by email link)" requirement, as an alternative to logging into the app.

## Context
Currently all review actions require an authenticated app session (`POST /api/subscriptions/{id}/review`, roles `Reviewer,DivisionHead`). No email notification or magic-link mechanism exists anywhere in the codebase.

## Acceptance Criteria
- [ ] Email sent to the assigned reviewer when a subscription reaches their step, containing a secure, time-limited action link
- [ ] Link resolves to a lightweight review page allowing Setuju/Tolak/Revisi without a full login (token-based, scoped to that one action)
- [ ] Requires selecting/configuring an email delivery mechanism (not present anywhere in the codebase today)
