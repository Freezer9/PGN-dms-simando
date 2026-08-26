---
id: "ops-quarterly-access-review-dormant-account-flaggi-2026-08-07"
status: "done"
priority: "low"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-10T15:00:00.000Z"
completedAt: "2026-08-10T15:00:00.000Z"
labels: ["ops", "identity"]
order: "a01e"
---

# Ops: quarterly access review / dormant-account flagging

Leaver revocation is now entirely manual (no directory to auto-revoke from) — the biggest operational risk of owning identity locally. Surface `last_login_at` in `/master/users` and recommend a quarterly review cadence to PGN.

`last_login_at` is now surfaced (see `admin-ui-masterusers-account-lifecycle-2026-08-07`, done) — the user list sorts dormant-first and flags never-logged-in/>90-day-dormant counts. What's left here is the actual recommendation to PGN to run the review quarterly; nothing further to build.
