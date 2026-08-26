using System.Security.Claims;
using BlazorBlueprint.Components;
using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Simando.Application.Common;
using Simando.Application.Directory;
using Simando.Application.RecordHub;
using Simando.Application.Security;
using Simando.Application.Workflow;
using Simando.Domain.Audit;
using Simando.Domain.Directory;
using Simando.Domain.Security;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Simando.Web.Components.Layout;
using Simando.Web.Components.Pages.Companies;
using Simando.Web.Components.Pages.StageGates;
using Simando.Web.Security;

namespace Simando.Web.Tests.Pages.Companies;

public class CompanyHubTests : TestContext
{
    private readonly ICompanyDetailService _detailService = Substitute.For<ICompanyDetailService>();
    private readonly ICompanyService _companyService = Substitute.For<ICompanyService>();
    private readonly IWorkflowService _workflowService = Substitute.For<IWorkflowService>();
    private readonly IUserService _userService = Substitute.For<IUserService>();

    public CompanyHubTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazorBlueprintComponents();

        Services.AddSingleton(_detailService);
        Services.AddSingleton(_companyService);
        Services.AddSingleton(_workflowService);
        Services.AddSingleton(_userService);
        Services.AddSingleton<BreadcrumbState>();
    }

    [Fact(DisplayName = "CompanyHub renders company details and header tab options")]
    public async Task CompanyHub_RendersHeader_AndTabs()
    {
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var dbOptions = new DbContextOptionsBuilder<SimandoDbContext>()
            .UseInMemoryDatabase($"HubTestDb_{Guid.NewGuid()}")
            .Options;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, "sales_rep")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var authStateProvider = Substitute.For<AuthenticationStateProvider>();
        authStateProvider.GetAuthenticationStateAsync()
            .Returns(Task.FromResult(new AuthenticationState(principal)));

        var currentUser = new CurrentUser(authStateProvider);
        var dbContext = new SimandoDbContext(dbOptions, currentUser);

        dbContext.Users.Add(new ApplicationUser
        {
            Id = userId,
            UserName = "sales_rep",
            FullName = "Sales Representative",
            Email = "sales@pgn.co.id"
        });

        dbContext.RoleAssignments.Add(new RoleAssignment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Role = Role.SalesArea,
            AreaId = Guid.NewGuid(),
            Active = true,
            AssignedBy = Guid.NewGuid(),
            AssignedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync();

        Services.AddSingleton(dbContext);
        Services.AddSingleton(currentUser);

        var companyDetail = new CompanyDetail(
            CompanyId: companyId,
            Nomor: "COMP-001",
            NamaPerusahaan: "PT Merdeka Gas",
            IndustryTypeName: "Tekstil",
            LocationLabel: "Surabaya",
            CreatedBy: userId,
            SalesRepName: "sales_rep",
            AreaId: Guid.NewGuid(),
            AreaName: "Area Surabaya",
            RegionId: Guid.NewGuid(),
            RegionName: "SOR II",
            CurrentStage: 1,
            Status: RecordStatus.Draft,
            HolderLabel: null,
            HolderName: null,
            StatusSince: DateTimeOffset.UtcNow,
            CurrentStepId: null,
            CurrentStepKind: null,
            WorkflowInstanceId: null,
            CanSubmit: false,
            CanAct: false,
            CanChooseReviewers: false,
            Contacts: []
        );

        _detailService.GetDetailAsync(companyId, Arg.Any<Guid>(), Arg.Any<EffectivePermissions>(), Arg.Any<IReadOnlySet<Role>>(), Arg.Any<CancellationToken>())
            .Returns(companyDetail);

        var cut = RenderComponent<CompanyHub>(parameters => parameters
            .Add(p => p.CompanyId, companyId)
            .Add(p => p.Tab, "ringkasan"));

        var markup = cut.Markup;
        markup.ShouldContain("PT Merdeka Gas");
        markup.ShouldContain("COMP-001");
        markup.ShouldContain("Area Surabaya");
    }
}
