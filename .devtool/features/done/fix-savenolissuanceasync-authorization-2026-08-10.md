---
id: "fix-savenolissuanceasync-authorization-2026-08-10"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-10T00:00:00.000Z"
modified: "2026-08-10T12:00:00.000Z"
completedAt: "2026-08-10T12:00:00.000Z"
labels: ["security", "audit"]
order: "a01s"
---

# Add authorization guard to SaveNolIssuanceAsync

`CompanyService.SaveNolIssuanceAsync` (stage-8 issuance fields) has no capability, status, or scope check. Any authenticated user can write the approved terms, attach Kontrak Bersyarat conditions, set the validity period, and trigger NOL or RL issuance. A NOL is a commercially binding document — this is the highest-severity authorization gap in the codebase.

The `SetApprovedTerms` and `IssueNolRl` capabilities are defined and correctly restricted to Division Head in `RoleCapabilities.cs`, but are never enforced in this method.

## What to do

Add an authorization guard to `SaveNolIssuanceAsync`:

1. **Capability**: requires `Capability.SetApprovedTerms` (for editing terms) and `Capability.IssueNolRl` (for the issuance action itself)
2. **Status**: requires the company's status to be `Approval` (the Division Head step)
3. **Scope**: requires the actor's region scope to cover the company's region

As with `SaveNolEvaluationAsync`, the existing `CanEditAsync` cannot be reused. A dedicated guard is needed.

## Acceptance criteria

- [ ] All unauthorized calls to this method return a `Fail` result and make no changes
- [ ] Only a Division Head whose scope covers the company's region can write to this method when the record is at `Approval` status
- [ ] The issuance action (NOL or RL) is rejected if attempted while the record is not at `Approval` status
- [ ] Tested: unit tests covering rejection cases (wrong role, wrong status, wrong scope)

## References

- `CompanyService.cs:1047–1093`
- `Capability.cs:29–30` (`SetApprovedTerms`, `IssueNolRl`)
- `RoleCapabilities.cs:106–108`
- `design/roles-permissions.md` §2.5 Division Head
