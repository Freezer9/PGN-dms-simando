---
id: "fix-stage-gates-server-side-enforcement-2026-08-10"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-10T00:00:00.000Z"
modified: "2026-08-10T12:00:00.000Z"
completedAt: "2026-08-10T12:00:00.000Z"
labels: ["security", "audit"]
order: "a01q"
---

# Enforce stage gates server-side

All three document gate checks are currently UI-only (`StageGateEvaluator` is called only from `DokumenTab.razor`). Any HTTP client that bypasses the UI can advance a record past Survey without a signed KK0, past A1 without Bukti Kelayakan, and submit for approval without the required A1 + KK0 + Capex Pre GR3 attachments.

## What to do

Move gate enforcement into the service layer. The spec is explicit: *"Enforce both gates server-side, on the transition endpoint. A greyed-out button in the UI is a convenience, not the control."*

### Gate 1 — Survey → A1
Enforce in `CompanyService` before advancing to the A1 stage: require `AttachmentKind.Kk0` present and marked signed.

### Gate 2 — A1 → Permohonan NOL
Enforce in `CompanyService` before advancing: require signed A1 / Formulir Registrasi, Bukti Kelayakan, and (if `PricingScheme == SiGas`) the MOM SiGas document.

### Gate 3 — Permohonan NOL → Ajukan (submit)
Enforce in `WorkflowService.StartAsync` before transitioning to `AREA_HEAD`: require A1, KK0, and Capex Pre GR3 attached to the NOL request record.

## Acceptance criteria

- [ ] All three gates checked server-side before the relevant transition proceeds
- [ ] `StageGateEvaluator` (or equivalent logic) called from the service layer, not only from Razor components
- [ ] Missing-document error returned as a structured `Fail` result with the document name named explicitly (for the UI to surface as *"Dokumen 'X' belum diunggah"*)
- [ ] UI gate checks remain as convenience UX on top — they are not removed, just no longer the only enforcement

## References

- `StageGateEvaluator.cs:14–54`
- `DokumenTab.razor:216, 219`
- `WorkflowService.cs` — `StartAsync`
- `domain/02-process-flow.md` §Gate B
