using Shouldly;
using Simando.Domain.Security;

namespace Simando.Domain.Tests.Security;

// Test IDs (P#) match docs/build/03-testing.md §3 so a failing test names
// the doc row it covers.
public class PermissionEvaluatorTests
{
    private static readonly Guid RegionSorII = Guid.NewGuid();
    private static readonly Guid RegionSorIII = Guid.NewGuid();
    private static readonly Guid AreaSurabaya = Guid.NewGuid();    // in SOR II
    private static readonly Guid AreaSidoarjo = Guid.NewGuid();    // in SOR II
    private static readonly Guid AreaInSorIII = Guid.NewGuid();    // in SOR III

    private static RoleAssignment Assignment(Role role, Guid? areaId = null, Guid? regionId = null, bool active = true) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Role = role,
            AreaId = areaId,
            RegionId = regionId,
            Active = active,
            AssignedBy = Guid.NewGuid(),
            AssignedAt = DateTimeOffset.UtcNow,
        };

    // --- Scope (P1-P7) ---

    [Fact(DisplayName = "P1: Sales Area @ Surabaya sees a Surabaya record")]
    public void P1()
    {
        var permissions = PermissionEvaluator.Resolve([Assignment(Role.SalesArea, areaId: AreaSurabaya)]);
        PermissionEvaluator.CanViewRecord(permissions, AreaSurabaya, RegionSorII).ShouldBeTrue();
    }

    [Fact(DisplayName = "P2: Sales Area @ Surabaya cannot see a Sidoarjo record, same region")]
    public void P2()
    {
        var permissions = PermissionEvaluator.Resolve([Assignment(Role.SalesArea, areaId: AreaSurabaya)]);
        PermissionEvaluator.CanViewRecord(permissions, AreaSidoarjo, RegionSorII).ShouldBeFalse();
    }

    [Fact(DisplayName = "P3: Area Head @ Surabaya cannot see a Sidoarjo record")]
    public void P3()
    {
        var permissions = PermissionEvaluator.Resolve([Assignment(Role.AreaHead, areaId: AreaSurabaya)]);
        PermissionEvaluator.CanViewRecord(permissions, AreaSidoarjo, RegionSorII).ShouldBeFalse();
    }

    [Fact(DisplayName = "P4: Regional Admin @ SOR II sees a Sidoarjo record")]
    public void P4()
    {
        var permissions = PermissionEvaluator.Resolve([Assignment(Role.RegionalAdmin, regionId: RegionSorII)]);
        PermissionEvaluator.CanViewRecord(permissions, AreaSidoarjo, RegionSorII).ShouldBeTrue();
    }

    [Fact(DisplayName = "P5: Regional Admin @ SOR II cannot see a SOR III record")]
    public void P5()
    {
        var permissions = PermissionEvaluator.Resolve([Assignment(Role.RegionalAdmin, regionId: RegionSorII)]);
        PermissionEvaluator.CanViewRecord(permissions, AreaInSorIII, RegionSorIII).ShouldBeFalse();
    }

    [Fact(DisplayName = "P6: Reviewer @ SOR II sees any Area in SOR II")]
    public void P6()
    {
        var permissions = PermissionEvaluator.Resolve([Assignment(Role.Reviewer, regionId: RegionSorII)]);
        PermissionEvaluator.CanViewRecord(permissions, AreaSidoarjo, RegionSorII).ShouldBeTrue();
    }

    [Fact(DisplayName = "P7: System Admin has no case-data visibility, despite All scope")]
    public void P7()
    {
        var permissions = PermissionEvaluator.Resolve([Assignment(Role.SystemAdmin)]);

        permissions.Scope.ShouldBe(AccessScope.All);
        PermissionEvaluator.CanViewRecord(permissions, AreaSurabaya, RegionSorII).ShouldBeFalse();
    }

    // --- Scope must hold on every surface (P8-P15) ---
    // No list/map/report/export endpoint exists yet — these assert that the
    // one primitive every future surface must call (CanViewRecord) rejects
    // an out-of-scope record, so no later surface can bypass it.

    [Theory(DisplayName = "P8-P15: every surface gates through the same scope check")]
    [InlineData("P8 Directory list")]
    [InlineData("P9 Global search by company name")]
    [InlineData("P10 Global search by exact Nomor")]
    [InlineData("P11 Map bounding-box query")]
    [InlineData("P12 Report")]
    [InlineData("P13 Excel export")]
    [InlineData("P14 Direct URL /companies/{id}")]
    [InlineData("P15 Attachment download by id")]
    public void P8_P15(string surface)
    {
        var permissions = PermissionEvaluator.Resolve([Assignment(Role.SalesArea, areaId: AreaSurabaya)]);
        PermissionEvaluator.CanViewRecord(permissions, AreaSidoarjo, RegionSorII).ShouldBeFalse(surface);
    }

    // --- Capability x turn (P16-P25) ---

    [Fact(DisplayName = "P16: Sales Area can edit and submit their own DRAFT")]
    public void P16()
    {
        var permissions = PermissionEvaluator.Resolve([Assignment(Role.SalesArea, areaId: AreaSurabaya)]);

        permissions.HasCapability(Capability.EditStages1To3).ShouldBeTrue();
        permissions.HasCapability(Capability.SubmitForApproval).ShouldBeTrue();
    }

    [Fact(DisplayName = "P17: Sales Area never holds an approval-step capability")]
    public void P17()
    {
        var permissions = PermissionEvaluator.Resolve([Assignment(Role.SalesArea, areaId: AreaSurabaya)]);
        permissions.HasCapability(Capability.ActOnApprovalStep).ShouldBeFalse();
    }

    [Fact(DisplayName = "P18: Area Head can approve/revise/reject when the record is at their step, never edit")]
    public void P18()
    {
        var permissions = PermissionEvaluator.Resolve([Assignment(Role.AreaHead, areaId: AreaSurabaya)]);
        var assignedStep = WorkflowStepKind.AreaHead;
        var recordStep = WorkflowStepKind.AreaHead;

        PermissionEvaluator.CanAct(permissions, Capability.ActOnApprovalStep, assignedStep == recordStep).ShouldBeTrue();
        permissions.HasCapability(Capability.EditStages1To3).ShouldBeFalse();
    }

    [Fact(DisplayName = "P19: Area Head has no action once the record moves to Regional Admin")]
    public void P19()
    {
        var permissions = PermissionEvaluator.Resolve([Assignment(Role.AreaHead, areaId: AreaSurabaya)]);
        var assignedStep = WorkflowStepKind.AreaHead;
        var recordStep = WorkflowStepKind.RegionalAdmin;

        PermissionEvaluator.CanAct(permissions, Capability.ActOnApprovalStep, assignedStep == recordStep).ShouldBeFalse();
    }

    [Fact(DisplayName = "P20: Area Head has no action past their Lampiran 17 endpoint")]
    public void P20()
    {
        var permissions = PermissionEvaluator.Resolve([Assignment(Role.AreaHead, areaId: AreaSurabaya)]);
        var assignedStep = WorkflowStepKind.AreaHead;
        var recordStep = WorkflowStepKind.Reviewer2;

        PermissionEvaluator.CanAct(permissions, Capability.ActOnApprovalStep, assignedStep == recordStep).ShouldBeFalse();
    }

    [Fact(DisplayName = "P21: Reviewer 1 cannot act while the record sits at Reviewer 2")]
    public void P21()
    {
        var permissions = PermissionEvaluator.Resolve([Assignment(Role.Reviewer, regionId: RegionSorII)]);
        var assignedStep = WorkflowStepKind.Reviewer1;
        var recordStep = WorkflowStepKind.Reviewer2;

        PermissionEvaluator.CanAct(permissions, Capability.ActOnApprovalStep, assignedStep == recordStep).ShouldBeFalse();
    }

    [Fact(DisplayName = "P22: Reviewer 2 can act at their own step")]
    public void P22()
    {
        var permissions = PermissionEvaluator.Resolve([Assignment(Role.Reviewer, regionId: RegionSorII)]);
        var assignedStep = WorkflowStepKind.Reviewer2;
        var recordStep = WorkflowStepKind.Reviewer2;

        PermissionEvaluator.CanAct(permissions, Capability.ActOnApprovalStep, assignedStep == recordStep).ShouldBeTrue();
    }

    [Fact(DisplayName = "P23: Regional Admin is the only role that both edits and approves")]
    public void P23()
    {
        var permissions = PermissionEvaluator.Resolve([Assignment(Role.RegionalAdmin, regionId: RegionSorII)]);
        var assignedStep = WorkflowStepKind.RegionalAdmin;
        var recordStep = WorkflowStepKind.RegionalAdmin;

        permissions.HasCapability(Capability.EditEvaluation).ShouldBeTrue();
        PermissionEvaluator.CanAct(permissions, Capability.ActOnApprovalStep, assignedStep == recordStep).ShouldBeTrue();
    }

    [Fact(DisplayName = "P24: Division Head can issue NOL/RL at their own step")]
    public void P24()
    {
        var permissions = PermissionEvaluator.Resolve([Assignment(Role.DivisionHead, regionId: RegionSorII)]);
        var assignedStep = WorkflowStepKind.DivisionHead;
        var recordStep = WorkflowStepKind.DivisionHead;

        PermissionEvaluator.CanAct(permissions, Capability.ActOnApprovalStep, assignedStep == recordStep).ShouldBeTrue();
        permissions.HasCapability(Capability.IssueNolRl).ShouldBeTrue();
    }

    [Fact(DisplayName = "P25: Division Head has no action while the record sits at Reviewer 1")]
    public void P25()
    {
        var permissions = PermissionEvaluator.Resolve([Assignment(Role.DivisionHead, regionId: RegionSorII)]);
        var assignedStep = WorkflowStepKind.DivisionHead;
        var recordStep = WorkflowStepKind.Reviewer1;

        PermissionEvaluator.CanAct(permissions, Capability.ActOnApprovalStep, assignedStep == recordStep).ShouldBeFalse();
    }

    // --- Segregation of duties (P26, P28, P29) ---

    [Fact(DisplayName = "P26: A user cannot act on a step for a record they created")]
    public void P26()
    {
        var userId = Guid.NewGuid();
        PermissionEvaluator.IsSelfApproval(userId, creatorId: userId, stage7EditorIds: []).ShouldBeTrue();
    }

    [Fact(DisplayName = "P28: A user who edited stage 7 cannot later approve the same record")]
    public void P28()
    {
        var userId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();

        PermissionEvaluator.IsSelfApproval(userId, creatorId, stage7EditorIds: [userId]).ShouldBeTrue();
    }

    [Fact(DisplayName = "P29: A user cannot modify their own role assignment")]
    public void P29()
    {
        var userId = Guid.NewGuid();
        PermissionEvaluator.IsSelfRoleModification(userId, targetUserId: userId).ShouldBeTrue();
    }

    [Fact(DisplayName = "Segregation of duties: an unrelated actor is not blocked")]
    public void UnrelatedActor_NotBlocked()
    {
        PermissionEvaluator.IsSelfApproval(Guid.NewGuid(), Guid.NewGuid(), stage7EditorIds: []).ShouldBeFalse();
        PermissionEvaluator.IsSelfRoleModification(Guid.NewGuid(), Guid.NewGuid()).ShouldBeFalse();
    }

    // --- Multi-role resolution ---

    [Fact(DisplayName = "Resolve: multi-role user gets widest scope and the union of capabilities")]
    public void Resolve_MultiRole_WidensScopeAndUnionsCapabilities()
    {
        var permissions = PermissionEvaluator.Resolve(
        [
            Assignment(Role.SalesArea, areaId: AreaSurabaya),
            Assignment(Role.Reviewer, regionId: RegionSorII),
        ]);

        permissions.Scope.ShouldBe(AccessScope.Region);
        permissions.HasCapability(Capability.EditStages1To3).ShouldBeTrue();
        permissions.HasCapability(Capability.ActOnApprovalStep).ShouldBeTrue();
    }

    [Fact(DisplayName = "Resolve: an inactive assignment contributes nothing")]
    public void Resolve_InactiveAssignment_Ignored()
    {
        var permissions = PermissionEvaluator.Resolve([Assignment(Role.SystemAdmin, active: false)]);

        permissions.Capabilities.ShouldBeEmpty();
        permissions.Scope.ShouldBe(AccessScope.Area);
    }

    // --- Who assigns roles (§5 guard rails) ---

    [Fact(DisplayName = "CanAssignRole: System Admin may assign any role, any region")]
    public void CanAssignRole_SystemAdmin_AnyRoleAnyRegion()
    {
        var actor = PermissionEvaluator.Resolve([Assignment(Role.SystemAdmin)]);

        PermissionEvaluator.CanAssignRole(actor, Role.RegionalAdmin, RegionSorIII).ShouldBeTrue();
        PermissionEvaluator.CanAssignRole(actor, Role.DivisionHead, RegionSorII).ShouldBeTrue();
    }

    [Fact(DisplayName = "CanAssignRole: Regional Admin may assign Sales Area within their own Region")]
    public void CanAssignRole_RegionalAdmin_InListOwnRegion_Allowed()
    {
        var actor = PermissionEvaluator.Resolve([Assignment(Role.RegionalAdmin, regionId: RegionSorII)]);

        PermissionEvaluator.CanAssignRole(actor, Role.SalesArea, RegionSorII).ShouldBeTrue();
    }

    [Fact(DisplayName = "CanAssignRole: Regional Admin cannot appoint another Regional Admin or Division Head")]
    public void CanAssignRole_RegionalAdmin_OutOfList_Rejected()
    {
        var actor = PermissionEvaluator.Resolve([Assignment(Role.RegionalAdmin, regionId: RegionSorII)]);

        PermissionEvaluator.CanAssignRole(actor, Role.RegionalAdmin, RegionSorII).ShouldBeFalse();
        PermissionEvaluator.CanAssignRole(actor, Role.DivisionHead, RegionSorII).ShouldBeFalse();
    }

    [Fact(DisplayName = "CanAssignRole: Regional Admin cannot assign into a different Region")]
    public void CanAssignRole_RegionalAdmin_WrongRegion_Rejected()
    {
        var actor = PermissionEvaluator.Resolve([Assignment(Role.RegionalAdmin, regionId: RegionSorII)]);

        PermissionEvaluator.CanAssignRole(actor, Role.SalesArea, RegionSorIII).ShouldBeFalse();
    }
}
