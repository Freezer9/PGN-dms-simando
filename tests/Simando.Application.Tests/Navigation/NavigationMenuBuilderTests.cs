using Shouldly;
using Simando.Application.Navigation;
using Simando.Domain.Security;

namespace Simando.Application.Tests.Navigation;

// Encodes docs/design/frontend/01-shell-and-navigation.md "Sidebar per role"
// as an executable spec, the same way PermissionEvaluatorTests encodes
// docs/design/03-roles-permissions.md — one test per ASCII picture, asserting
// the exact label sequence the builder produces for that role.
public class NavigationMenuBuilderTests
{
    private static readonly Guid AreaSurabaya = Guid.NewGuid();
    private static readonly Guid RegionSorII = Guid.NewGuid();

    private static RoleAssignment Assignment(Role role, Guid? areaId = null, Guid? regionId = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Role = role,
            AreaId = areaId,
            RegionId = regionId,
            Active = true,
            AssignedBy = Guid.NewGuid(),
            AssignedAt = DateTimeOffset.UtcNow,
        };

    private static IReadOnlyList<string> Labels(params RoleAssignment[] assignments)
    {
        var permissions = PermissionEvaluator.Resolve(assignments);
        var roles = assignments.Select(a => a.Role).ToHashSet();
        var menu = NavigationMenuBuilder.Build(permissions, roles);

        var labels = new List<string>();
        foreach (var section in menu.Sections)
        {
            foreach (var node in section.Nodes)
            {
                switch (node)
                {
                    case NavItem link:
                        labels.Add(link.Label);
                        break;
                    case NavGroup group:
                        labels.Add(group.Label);
                        labels.AddRange(group.Children.Select(c => c.Label));
                        break;
                }
            }
        }

        return labels;
    }

    [Fact(DisplayName = "Sales Area: Beranda, Direktori, Plotting, Peta, Laporan")]
    public void SalesArea()
    {
        Labels(Assignment(Role.SalesArea, areaId: AreaSurabaya))
            .ShouldBe(["Beranda", "Direktori", "Plotting", "Peta", "Laporan"]);
    }

    [Fact(DisplayName = "Area Head: adds Tugas Saya; keeps read-only Direktori/Plotting")]
    public void AreaHead()
    {
        Labels(Assignment(Role.AreaHead, areaId: AreaSurabaya))
            .ShouldBe(["Beranda", "Tugas Saya", "Direktori", "Plotting", "Peta", "Laporan"]);
    }

    [Fact(DisplayName = "Reviewer: deliberately minimal — no Direktori/Plotting despite ViewCompanyRecords")]
    public void Reviewer()
    {
        Labels(Assignment(Role.Reviewer, regionId: RegionSorII))
            .ShouldBe(["Beranda", "Tugas Saya", "Peta", "Laporan"]);
    }

    [Fact(DisplayName = "Division Head: adds Akses Darurat, no Pengguna")]
    public void DivisionHead()
    {
        Labels(Assignment(Role.DivisionHead, regionId: RegionSorII))
            .ShouldBe(["Beranda", "Tugas Saya", "Peta", "Laporan", "Akses Darurat"]);
    }

    [Fact(DisplayName = "Regional Admin: the busiest sidebar, matching the busiest role")]
    public void RegionalAdmin()
    {
        Labels(Assignment(Role.RegionalAdmin, regionId: RegionSorII))
            .ShouldBe([
                "Beranda", "Tugas Saya", "Tugas Tertahan", "Direktori", "Plotting", "Peta", "Evaluasi", "Laporan",
                "Pengguna", "Akses Darurat",
            ]);
    }

    [Fact(DisplayName = "System Admin: no case data, own grouped master-data tree")]
    public void SystemAdmin()
    {
        Labels(Assignment(Role.SystemAdmin))
            .ShouldBe([
                "Organisasi", "Pengguna",
                "Referensi", "Negara", "Jenis Industri",
                "Komersial", "Segmen",
                "Energi & Konversi", "Jenis Bahan Bakar", "Satuan",
                "Teknis", "G-Size / Meter", "Spesifikasi MRS",
                "Dokumen", "Dokumen Acuan Kerja", "Kategori Alasan",
                "Pemulihan", "Langkah Tertahan (semua region)", "Akses Darurat (break-glass)",
            ]);
    }

    [Fact(DisplayName = "Multi-role: Reviewer + Sales Area sees the union, not either role alone")]
    public void MultiRole_SeesUnion()
    {
        Labels(
                Assignment(Role.Reviewer, regionId: RegionSorII),
                Assignment(Role.SalesArea, areaId: AreaSurabaya))
            .ShouldBe(["Beranda", "Tugas Saya", "Direktori", "Plotting", "Peta", "Laporan"]);
    }
}
