namespace Simando.Domain.Security;

public sealed record RoleProfile(AccessScope DefaultScope, IReadOnlySet<Capability> Capabilities);

// Transcribes the three capability-matrix tables in
// docs/design/roles-permissions.md §3 (Records / Workflow /
// Reporting & administration) cell by cell, so the matrix is
// machine-checkable instead of only living in a markdown table.
//
// "👁 read-only" rows (e.g. Area Head/Reviewer "View company records")
// are represented by granting the view capability but withholding the
// matching edit capability — there is no separate read-only flag.
// "🔓 break-glass only" rows (System Admin "View company records",
// "View the record timeline") are represented by NOT granting the plain
// capability at all — only BreakGlassRecordRead is granted, so a caller
// must go through that separate, audited path.
public static class RoleCapabilities
{
    private static readonly IReadOnlyDictionary<Role, RoleProfile> Profiles =
        new Dictionary<Role, RoleProfile>
        {
            [Role.SalesArea] = new(AccessScope.Area, new HashSet<Capability>
            {
                Capability.ViewCompanyRecords,
                Capability.CreateCompany,
                Capability.EditStages1To3,
                Capability.SoftDeleteCompany,
                Capability.DropMovePin,
                Capability.EditSurvey,
                Capability.EditA1,
                Capability.SignUploadSignedA1,
                Capability.EditNolRequest,
                Capability.UploadAttachments,
                Capability.DownloadAttachments,
                Capability.GenerateDocuments,
                Capability.SubmitForApproval,
                Capability.ViewTimeline,
                Capability.ViewDashboardFunnel,
                Capability.ViewAgeingReport,
                Capability.ExportExcel,
            }),

            [Role.AreaHead] = new(AccessScope.Area, new HashSet<Capability>
            {
                Capability.ViewCompanyRecords,
                Capability.DownloadAttachments,
                Capability.GenerateDocuments,
                Capability.ActOnApprovalStep,
                Capability.ViewTimeline,
                Capability.ViewDashboardFunnel,
                Capability.ViewAgeingReport,
                Capability.ExportExcel,
            }),

            [Role.RegionalAdmin] = new(AccessScope.Region, new HashSet<Capability>
            {
                Capability.ViewCompanyRecords,
                Capability.CreateCompany,
                Capability.EditStages1To3,
                Capability.SoftDeleteCompany,
                Capability.DropMovePin,
                Capability.EditSurvey,
                Capability.EditA1,
                Capability.SignUploadSignedA1,
                Capability.EditNolRequest,
                Capability.EditEvaluation,
                Capability.ProduceResumeEvaluasi,
                Capability.RecordFeedCheckpoint,
                Capability.UploadAttachments,
                Capability.DownloadAttachments,
                Capability.GenerateDocuments,
                Capability.ActOnApprovalStep,
                Capability.ChooseReviewers,
                Capability.ReassignWorkflowStep,
                Capability.ViewTimeline,
                Capability.ViewDashboardFunnel,
                Capability.ViewAgeingReport,
                Capability.ExportExcel,
                Capability.ExportContactDataPii,
                // Regional Admin may only assign Sales Area / Area Head / Reviewer,
                // and only within its own Region — docs/design/roles-permissions §5 "Who assigns
                // roles". That narrower rule is a caller-side check (it needs the
                // target role and region), not a capability distinction.
                Capability.AssignRoles,
                Capability.ViewBreakGlassActivity,
            }),

            [Role.Reviewer] = new(AccessScope.Region, new HashSet<Capability>
            {
                Capability.ViewCompanyRecords,
                Capability.DownloadAttachments,
                Capability.GenerateDocuments,
                Capability.ActOnApprovalStep,
                Capability.ViewTimeline,
                Capability.ViewDashboardFunnel,
                Capability.ViewAgeingReport,
                Capability.ExportExcel,
            }),

            [Role.DivisionHead] = new(AccessScope.Region, new HashSet<Capability>
            {
                Capability.ViewCompanyRecords,
                Capability.DownloadAttachments,
                Capability.GenerateDocuments,
                Capability.ActOnApprovalStep,
                Capability.IssueNolRl,
                Capability.SetApprovedTerms,
                Capability.ViewTimeline,
                Capability.ViewDashboardFunnel,
                Capability.ViewAgeingReport,
                Capability.ExportExcel,
                Capability.ExportContactDataPii,
                Capability.ViewBreakGlassActivity,
            }),

            [Role.SystemAdmin] = new(AccessScope.All, new HashSet<Capability>
            {
                Capability.SoftDeleteCompany,
                Capability.ReassignWorkflowStep, // via the stuck-steps view only
                Capability.ManageMasterData,
                Capability.BreakGlassRecordRead,
                Capability.AssignRoles,
                Capability.ViewBreakGlassActivity,
            }),
        };

    public static RoleProfile For(Role role) => Profiles[role];
}
