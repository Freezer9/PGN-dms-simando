using Shouldly;
using Simando.Domain.Security;

namespace Simando.Domain.Tests.Security;

// One test per role, each asserting the full expected capability set against
// docs/design/03-roles-permissions.md §3 — a SetEquals-style diff pinpoints
// exactly which capability is missing or extra, which 30-plus individual
// [InlineData] rows would not do any more reliably.
public class RoleCapabilitiesTests
{
    [Fact(DisplayName = "Sales Area — matrix transcription")]
    public void SalesArea_MatchesMatrix()
    {
        var profile = RoleCapabilities.For(Role.SalesArea);

        profile.DefaultScope.ShouldBe(AccessScope.Area);
        profile.Capabilities.ShouldBe(
        [
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
        ], ignoreOrder: true);
    }

    [Fact(DisplayName = "Area Head — matrix transcription")]
    public void AreaHead_MatchesMatrix()
    {
        var profile = RoleCapabilities.For(Role.AreaHead);

        profile.DefaultScope.ShouldBe(AccessScope.Area);
        profile.Capabilities.ShouldBe(
        [
            Capability.ViewCompanyRecords,
            Capability.DownloadAttachments,
            Capability.GenerateDocuments,
            Capability.ActOnApprovalStep,
            Capability.ViewTimeline,
            Capability.ViewDashboardFunnel,
            Capability.ViewAgeingReport,
            Capability.ExportExcel,
        ], ignoreOrder: true);
    }

    [Fact(DisplayName = "Regional Admin — matrix transcription")]
    public void RegionalAdmin_MatchesMatrix()
    {
        var profile = RoleCapabilities.For(Role.RegionalAdmin);

        profile.DefaultScope.ShouldBe(AccessScope.Region);
        profile.Capabilities.ShouldBe(
        [
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
            Capability.AssignRoles,
            Capability.ViewBreakGlassActivity,
        ], ignoreOrder: true);
    }

    [Fact(DisplayName = "Reviewer — matrix transcription")]
    public void Reviewer_MatchesMatrix()
    {
        var profile = RoleCapabilities.For(Role.Reviewer);

        profile.DefaultScope.ShouldBe(AccessScope.Region);
        profile.Capabilities.ShouldBe(
        [
            Capability.ViewCompanyRecords,
            Capability.DownloadAttachments,
            Capability.GenerateDocuments,
            Capability.ActOnApprovalStep,
            Capability.ViewTimeline,
            Capability.ViewDashboardFunnel,
            Capability.ViewAgeingReport,
            Capability.ExportExcel,
        ], ignoreOrder: true);
    }

    [Fact(DisplayName = "Division Head — matrix transcription")]
    public void DivisionHead_MatchesMatrix()
    {
        var profile = RoleCapabilities.For(Role.DivisionHead);

        profile.DefaultScope.ShouldBe(AccessScope.Region);
        profile.Capabilities.ShouldBe(
        [
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
        ], ignoreOrder: true);
    }

    [Fact(DisplayName = "System Admin — matrix transcription; never plain case-data view")]
    public void SystemAdmin_MatchesMatrix()
    {
        var profile = RoleCapabilities.For(Role.SystemAdmin);

        profile.DefaultScope.ShouldBe(AccessScope.All);
        profile.Capabilities.ShouldBe(
        [
            Capability.SoftDeleteCompany,
            Capability.ReassignWorkflowStep,
            Capability.ManageMasterData,
            Capability.BreakGlassRecordRead,
            Capability.AssignRoles,
            Capability.ViewBreakGlassActivity,
        ], ignoreOrder: true);

        // The 🔓 rows: System Admin never holds these plainly, only via the
        // dedicated break-glass / stuck-steps paths.
        profile.Capabilities.ShouldNotContain(Capability.ViewCompanyRecords);
        profile.Capabilities.ShouldNotContain(Capability.ViewTimeline);
        profile.Capabilities.ShouldNotContain(Capability.DownloadAttachments);
        profile.Capabilities.ShouldNotContain(Capability.GenerateDocuments);
    }
}
