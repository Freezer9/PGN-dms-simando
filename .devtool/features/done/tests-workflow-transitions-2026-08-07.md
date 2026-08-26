---
id: "tests-workflow-transitions-2026-08-07"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T09:00:00.000Z"
completedAt: "2026-08-07T09:00:00.000Z"
labels: ["testing", "workflow"]
order: "a00O"
---

# Tests: workflow transitions

Unit tests for `WorkflowTransitions` — the counter-intuitive rules (sideways reject, one-step revisi). This is the module `docs/build/testing.md` calls out for 100% branch coverage.

Files: `tests/Simando.Domain.Tests/Workflow/WorkflowTransitionsTests.cs`.

Remaining: full W1–W20 matrix from `docs/build/testing.md#4` needs `WorkflowInstance`/`WorkflowStep` persistence to test end-to-end (tracked separately under NOL workflow).
