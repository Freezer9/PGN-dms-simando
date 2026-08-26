---
id: "workflow-state-machine-workflowtransitions-2026-08-07"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T09:00:00.000Z"
completedAt: "2026-08-07T09:00:00.000Z"
labels: ["workflow", "domain"]
order: "a005"
---

# Workflow state machine (WorkflowTransitions)

Core transition logic: `Setuju` (+1 step), `Revisi` (-1 step, comment required), `Tolak` (jump sideways to Regional Admin, comment required — **not** back to the creator). Implements the counter-intuitive rule from `docs/design/approval-workflow.md#the-three-transitions` that a generic BPMN engine would fight.

Files: `Simando.Domain/Workflow/WorkflowTransitions.cs` (163 lines — the piece most likely to be argued over with the client).
