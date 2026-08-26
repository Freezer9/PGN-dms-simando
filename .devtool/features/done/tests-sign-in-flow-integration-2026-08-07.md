---
id: "tests-sign-in-flow-integration-2026-08-07"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T09:00:00.000Z"
completedAt: "2026-08-07T09:00:00.000Z"
labels: ["testing", "identity"]
order: "a00U"
---

# Tests: sign-in flow (integration)

Integration coverage for the `AccountController` sign-in path — a slice of the A1–A17 authentication matrix in `docs/build/testing.md#authentication--account-lifecycle`.

Files: `tests/Simando.Integration.Tests/SignInFlowTests.cs`.

Remaining: full A1–A17 matrix (lockout auto-expire, password history, timing-safe rejection, `/forgot-password` 404) — track as follow-up once `/master/users` account lifecycle ships.
