using Simando.Domain.Security;

namespace Simando.Application.Navigation;

// Builds sidebar navigation from resolved capabilities per docs/design/frontend/01-shell-and-navigation.md.
public static class NavigationMenuBuilder
{
    public static NavMenu Build(EffectivePermissions permissions, IReadOnlySet<Role> roles, int? pendingTaskCount = null)
    {
        var sections = new List<NavSection>();

        var caseWork = BuildCaseWorkSection(permissions, roles, pendingTaskCount);
        if (caseWork.Nodes.Count > 0)
        {
            sections.Add(caseWork);
        }

        var extras = BuildExtrasSection(permissions);
        if (extras.Nodes.Count > 0)
        {
            sections.Add(extras);
        }

        var admin = BuildAdminSection(permissions);
        if (admin.Nodes.Count > 0)
        {
            sections.Add(admin);
        }

        return new NavMenu(sections);
    }

    // Operational sidebar for non-System Admin roles (docs/design/roles-permissions.md §2.6).
    private static NavSection BuildCaseWorkSection(EffectivePermissions permissions, IReadOnlySet<Role> roles, int? pendingTaskCount)
    {
        var nodes = new List<NavNode>();

        if (permissions.HasCapability(Capability.ViewDashboardFunnel))
        {
            nodes.Add(new NavItem("Beranda", "/", "house"));
        }

        if (permissions.HasCapability(Capability.ActOnApprovalStep))
        {
            var badgeText = pendingTaskCount is > 0 ? pendingTaskCount.Value.ToString() : null;
            nodes.Add(new NavItem("Tugas Saya", "/tasks", "list-checks", badgeText));
        }

        if (permissions.HasCapability(Capability.ReassignWorkflowStep) && permissions.Scope == AccessScope.Region)
        {
            nodes.Add(new NavItem("Tugas Tertahan", "/tasks/blocked", "octagon-alert"));
        }

        // Direktori/Plotting: Role-gated for Reviewer per docs/design/frontend/01-shell-and-navigation.md.
        if (roles.Overlaps(BrowsePipelineRoles))
        {
            nodes.Add(new NavItem("Direktori", "/directory", "building-2"));
            nodes.Add(new NavItem("Plotting", "/plotting", "map-pin"));
        }

        if (permissions.HasCapability(Capability.ViewCompanyRecords))
        {
            nodes.Add(new NavItem("Peta", "/map", "map"));
        }

        // Evaluasi queue route per docs/design/frontend/01-shell-and-navigation.md.
        if (permissions.HasCapability(Capability.EditEvaluation))
        {
            nodes.Add(new NavItem("Evaluasi", "/directory?stage=7", "calculator"));
        }

        if (permissions.HasCapability(Capability.ViewDashboardFunnel))
        {
            nodes.Add(new NavItem("Laporan", "/reports", "bar-chart-3"));
        }

        return new NavSection(nodes);
    }

    private static readonly IReadOnlySet<Role> BrowsePipelineRoles =
        new HashSet<Role> { Role.SalesArea, Role.AreaHead, Role.RegionalAdmin };

    // Regional Admin & Division Head trailing sidebar section.
    private static NavSection BuildExtrasSection(EffectivePermissions permissions)
    {
        if (permissions.HasCapability(Capability.ManageMasterData))
        {
            return new NavSection([]);
        }

        var nodes = new List<NavNode>();

        if (permissions.HasCapability(Capability.AssignRoles))
        {
            nodes.Add(new NavItem("Pengguna", "/master/users", "users"));
        }

        if (permissions.HasCapability(Capability.ViewBreakGlassActivity) || permissions.HasCapability(Capability.BreakGlassRecordRead))
        {
            nodes.Add(new NavItem("Akses Darurat", "/admin/break-glass", "shield-alert"));
        }

        return new NavSection(nodes);
    }

    // System Admin navigation tree per docs/design/frontend/01-shell-and-navigation.md.
    private static NavSection BuildAdminSection(EffectivePermissions permissions)
    {
        if (!permissions.HasCapability(Capability.ManageMasterData))
        {
            return new NavSection([]);
        }

        NavNode[] nodes =
        [
            new NavItem("Organisasi", "/master/organisation", "network"),
            new NavItem("Pengguna", "/master/users", "users"),
            new NavGroup("Referensi", "map-pinned",
            [
                new NavItem("Negara", "/master/countries", "globe"),
                new NavItem("Jenis Industri", "/master/industry-types", "factory"),
            ]),
            new NavGroup("Komersial", "tags",
            [
                new NavItem("Segmen", "/master/segments", "tags"),
            ]),
            new NavGroup("Energi & Konversi", "fuel",
            [
                new NavItem("Jenis Bahan Bakar", "/master/fuel-types", "fuel"),
                new NavItem("Satuan", "/master/units", "ruler"),
            ]),
            new NavGroup("Teknis", "wrench",
            [
                new NavItem("G-Size / Meter", "/master/meter-sizes", "gauge"),
                new NavItem("Spesifikasi MRS", "/master/mrs-specs", "settings-2"),
            ]),
            new NavGroup("Dokumen", "folder",
            [
                new NavItem("Dokumen Acuan Kerja", "/master/reference-documents", "file-text"),
                new NavItem("Kategori Alasan", "/master/reason-categories", "tags"),
            ]),
            new NavGroup("Pemulihan", "life-buoy",
            [
                new NavItem("Langkah Tertahan (semua region)", "/admin/stuck-steps", "octagon-pause"),
                new NavItem("Akses Darurat (break-glass)", "/admin/break-glass", "shield-alert"),
            ]),
        ];

        return new NavSection(nodes);
    }
}
