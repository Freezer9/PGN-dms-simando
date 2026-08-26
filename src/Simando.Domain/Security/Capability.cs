namespace Simando.Domain.Security;

public enum Capability
{
    // Records — docs/design/roles-permissions.md §3 "Records"
    ViewCompanyRecords,
    CreateCompany,
    EditStages1To3,
    SoftDeleteCompany,
    DropMovePin,
    EditSurvey,
    EditA1,
    SignUploadSignedA1,
    EditNolRequest,
    EditEvaluation,
    ProduceResumeEvaluasi,
    RecordFeedCheckpoint,
    UploadAttachments,
    DownloadAttachments,
    GenerateDocuments,

    // Workflow — docs/design/roles-permissions.md §3 "Workflow"
    SubmitForApproval,

    // Setuju / Revisi / Tolak collapse into one capability: the matrix marks
    // all three identically (⏱, turn-gated) for every role.
    ActOnApprovalStep,
    IssueNolRl,
    SetApprovedTerms,
    ChooseReviewers,
    ReassignWorkflowStep,
    ViewTimeline,

    // Reporting & administration — docs/design/roles-permissions.md §3
    ViewDashboardFunnel,
    ViewAgeingReport,
    ExportExcel,
    ExportContactDataPii,
    ManageMasterData,
    BreakGlassRecordRead,
    AssignRoles,
    ViewBreakGlassActivity,
}
