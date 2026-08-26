using System.Security.Claims;
using BlazorBlueprint.Components;
using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Simando.Application.Tasks;
using Simando.Application.Workflow;
using Simando.Domain.Security;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Simando.Web.Components.Layout;
using Simando.Web.Components.Pages.Tasks;
using Simando.Web.Security;

namespace Simando.Web.Tests.Pages.Tasks;

public class TasksBlockedTests : TestContext
{
    private readonly ITasksService _tasksService = Substitute.For<ITasksService>();
    private readonly IWorkflowService _workflowService = Substitute.For<IWorkflowService>();

    public TasksBlockedTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazorBlueprintComponents();

        Services.AddSingleton(_tasksService);
        Services.AddSingleton(_workflowService);
        Services.AddSingleton<BreadcrumbState>();
    }

    [Fact(DisplayName = "TasksBlocked page renders table of stuck workflow steps for Regional Admin")]
    public async Task TasksBlocked_RendersBlockedTasksTable()
    {
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var stepId = Guid.NewGuid();

        var dbOptions = new DbContextOptionsBuilder<SimandoDbContext>()
            .UseInMemoryDatabase($"BlockedTasksDb_{Guid.NewGuid()}")
            .Options;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, "reg_admin")
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
            UserName = "reg_admin",
            FullName = "Regional Administrator",
            Email = "admin@pgn.co.id"
        });

        dbContext.RoleAssignments.Add(new RoleAssignment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Role = Role.RegionalAdmin,
            RegionId = Guid.NewGuid(),
            Active = true,
            AssignedBy = Guid.NewGuid(),
            AssignedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync();

        var dbFactory = Substitute.For<IDbContextFactory<SimandoDbContext>>();
        dbFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(new SimandoDbContext(dbOptions, currentUser)));

        Services.AddSingleton(dbContext);
        Services.AddSingleton(currentUser);
        Services.AddSingleton(dbFactory);

        var blockedTask = new TaskListItem(
            CompanyId: companyId,
            Nomor: "COMP-BLOCKED-001",
            NamaPerusahaan: "PT Tertahan Utama",
            IndustryTypeName: "Tekstil",
            StepId: stepId,
            StepKind: WorkflowStepKind.AreaHead,
            AreaName: "Area Surabaya",
            RegionName: "SOR II",
            SubmittedByName: "sales_rep",
            WaitingSince: DateTimeOffset.UtcNow.AddDays(-10)
        );

        _tasksService.GetBlockedTasksAsync(Arg.Any<EffectivePermissions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<TaskListItem>>([blockedTask]));

        var cut = RenderComponent<TasksBlocked>();

        var markup = cut.Markup;
        markup.ShouldContain("Tugas Tertahan");
        markup.ShouldContain("PT Tertahan Utama");
        markup.ShouldContain("COMP-BLOCKED-001");
    }
}
