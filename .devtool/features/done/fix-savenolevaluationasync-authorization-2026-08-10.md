---
id: "fix-savenolevaluationasync-authorization-2026-08-10"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-10T00:00:00.000Z"
modified: "2026-08-10T12:00:00.000Z"
completedAt: "2026-08-10T12:00:00.000Z"
labels: ["security", "audit"]
order: "a01r"
---

# Add authorization guard to SaveNolEvaluationAsync

`CompanyService.SaveNolEvaluationAsync` (stage-7 evaluation fields) has no capability, status, or scope check. Any authenticated user can overwrite final capex, pipe sizing, IRR/NPV/Payback, and Resume Evaluasi data. The `EditEvaluation` capability is defined and granted only to Regional Admin but is never enforced here.

## What to do

Add an authorization guard to `SaveNolEvaluationAsync` that:

1. **Capability**: requires `Capability.EditEvaluation`
2. **Status**: requires the company's status to be `RegionalAdmin` (RA is the only role that edits stage-7 while the record is in-flight — not Draft)
3. **Scope**: requires the actor's region scope to cover the company's region

Note: the existing `CanEditAsync` helper **cannot be reused** here — it rejects any record with `Status != Draft`, which would incorrectly block Regional Admin. A dedicated `CanEditEvaluationAsync` guard (or an inline check) is needed.

## Acceptance criteria

- [ ] Unauthenticated and unauthorized calls to this method return a `Fail` result (not an exception) and make no changes
- [ ] A Sales Area user, Reviewer, or Division Head who calls this endpoint is rejected
- [ ] Regional Admin can still write stage-7 data when the record is at `RegionalAdmin` status
- [ ] Tested: unit test covering the rejection cases

## References

- `CompanyService.cs:960–1025`
- `Capability.cs:15` (`EditEvaluation`)
- `RoleCapabilities.cs:66`
- `design/roles-permissions.md` §Capability matrix
