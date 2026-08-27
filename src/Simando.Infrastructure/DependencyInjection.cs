using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Simando.Application.Attachments;
using Simando.Application.Common;
using Simando.Application.Dashboard;
using Simando.Application.Documents;
using Simando.Application.Notifications;
using Simando.Application.Organisation;
using Simando.Application.Directory;
using Simando.Application.Geography;
using Simando.Application.MasterData;
using Simando.Application.RecordHub;
using Simando.Application.Reports;
using Simando.Application.Security;
using Simando.Application.Storage;
using Simando.Domain.Attachments;
using Simando.Application.Tasks;
using Simando.Application.Workflow;
using Simando.Infrastructure.Directory;
using Simando.Infrastructure.Attachments;
using Simando.Infrastructure.Dashboard;
using Simando.Infrastructure.Documents;
using Simando.Infrastructure.Geography;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.MasterData;
using Simando.Infrastructure.Notifications;
using Simando.Infrastructure.Options;
using Simando.Infrastructure.Persistence;
using Simando.Infrastructure.RecordHub;
using Simando.Infrastructure.Reports;
using Simando.Infrastructure.Security;
using Simando.Infrastructure.Storage;
using Simando.Infrastructure.Tasks;
using Simando.Infrastructure.Workflow;

namespace Simando.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Blazor Server circuits are long-lived; DbContextFactory creates short-lived contexts per operation.
        // Scoped lifetime ensures factory captures circuit-scoped ICurrentUser for RLS query filters.
        services.AddDbContextFactory<SimandoDbContext>(options => options
            .UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsql =>
                {
                    npgsql.UseNetTopologySuite();
                    npgsql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                })
            .UseSnakeCaseNamingConvention(),
            lifetime: ServiceLifetime.Scoped);

        // Custom RoleAssignment-driven RBAC (no IdentityRole).
        // SignInManager and token providers require Microsoft.AspNetCore.App, so they are registered in Simando.Web.
        services.AddIdentityCore<ApplicationUser>(options => ConfigurePasswordAndLockout(options, configuration))
            .AddEntityFrameworkStores<SimandoDbContext>()
            .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();

        services.AddScoped<AdminSeeder>();
        services.AddScoped<DemoUserSeeder>();
        services.AddScoped<GeographySeeder>();
        services.AddScoped<MasterDataSeeder>();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IEntityService<>), typeof(EntityService<>));
        services.AddScoped<IOrganisationService, OrganisationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddSingleton<TemporaryPasswordGenerator>();
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        services.AddScoped<InAppNotificationChannel>();
        services.AddScoped<MailKitEmailNotificationChannel>();
        services.AddScoped<INotificationChannel>(sp => new CompositeNotificationChannel(new INotificationChannel[]
        {
            sp.GetRequiredService<InAppNotificationChannel>(),
            sp.GetRequiredService<MailKitEmailNotificationChannel>()
        }));
        services.AddScoped<IWorkflowService, WorkflowService>();
        services.AddScoped<ITasksService, TasksService>();
        services.AddScoped<ICompanyDetailService, CompanyDetailService>();
        services.AddScoped<IReportsService, ReportsService>();
        services.AddScoped<IGeographyService, GeographyService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IUnitLookupService, UnitLookupService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<IDocumentGenerator, DocxDocumentGenerator>();
        services.AddSingleton<IWorkflowEventNotifier, WorkflowEventNotifier>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IExcelExportService, ExcelExportService>();
        services.AddScoped<IBreakGlassService, BreakGlassService>();
        services.AddScoped<IOrphanBlobSweepJob, OrphanBlobSweepJob>();

        // Only S3 is implemented — OneDrive is storage-onedriveattachmentstore-
        // dual-provider-reso-2026-08-07, blocked on PGN granting tenant access.
        // Selecting it fails fast here, same shape as the unknown-Type case.
        services.AddOptions<StorageOptions>()
            .Bind(configuration.GetSection("Storage"))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<StorageOptions>, StorageOptionsValidator>();
        services.AddSingleton<IAttachmentStore>(sp =>
            sp.GetRequiredService<IOptions<StorageOptions>>().Value.Type switch
            {
                StorageProvider.S3 => ActivatorUtilities.CreateInstance<S3AttachmentStore>(sp),
                var t => throw new InvalidOperationException($"Storage:Type '{t}' has no implementation registered."),
            });
        services.AddOptions<UploadOptions>()
            .Bind(configuration.GetSection("Upload"))
            .ValidateOnStart();

        services.AddOptions<AuthOptions>()
            .Bind(configuration.GetSection("Auth"))
            .ValidateOnStart();

        services.AddHostedService<StorageStartupProbe>();

        return services;
    }

    private static void ConfigurePasswordAndLockout(IdentityOptions options, IConfiguration configuration)
    {
        var password = configuration.GetSection("Auth:Password");
        var requireMixed = password.GetValue("RequireMixed", true);

        options.Password.RequiredLength = password.GetValue("MinLength", 12);
        options.Password.RequireUppercase = requireMixed;
        options.Password.RequireLowercase = requireMixed;
        options.Password.RequireDigit = requireMixed;
        options.Password.RequireNonAlphanumeric = false;

        var lockout = configuration.GetSection("Auth:Lockout");
        options.Lockout.MaxFailedAccessAttempts = lockout.GetValue("MaxAttempts", 10);
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(lockout.GetValue("Minutes", 15));
        options.Lockout.AllowedForNewUsers = true;

        options.User.RequireUniqueEmail = true;
    }
}
