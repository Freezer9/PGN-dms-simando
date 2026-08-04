using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pgn.Dms.Shared;

namespace Pgn.Dms.Api.Data;

public class IdentitySeeder(IServiceProvider services, ILogger<IdentitySeeder> logger) : IHostedService
{
    public const string DemoPassword = "Passw0rd!";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync(cancellationToken);

        await SeedRegionsAsync(db);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in SimandoRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var areas = await db.Areas.ToListAsync(cancellationToken);
        var area = areas.FirstOrDefault();

        var demoUsers = new (string Email, string FullName, string Role)[]
        {
            ("sales@pgn.id", "Budi Santoso", SimandoRoles.SalesArea),
            ("areahead@pgn.id", "Ahmad Hidayat", SimandoRoles.AreaHead),
            ("admin@pgn.id", "Siti Rahayu", SimandoRoles.AdminRegional),
            ("reviewer@pgn.id", "Dewi Lestari", SimandoRoles.Reviewer),
            ("division@pgn.id", "Eko Prasetyo", SimandoRoles.DivisionHead),
        };

        var createdUsers = new Dictionary<string, ApplicationUser>();

        foreach (var (email, fullName, role) in demoUsers)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = fullName,
                    AreaId = area?.Id
                };

                var result = await userManager.CreateAsync(user, DemoPassword);
                if (!result.Succeeded)
                {
                    logger.LogError("Could not seed {Email}: {Errors}", email,
                        string.Join("; ", result.Errors.Select(e => e.Description)));
                    continue;
                }
            }

            createdUsers[email] = user;
            if (!await userManager.IsInRoleAsync(user, role))
                await userManager.AddToRoleAsync(user, role);
        }

        await SeedDummySubscriptionsAsync(db, createdUsers, areas);
    }

    private static async Task SeedRegionsAsync(ApplicationDbContext db)
    {
        if (await db.Regions.AnyAsync()) return;

        var region = new Region { Name = "Regional Jawa Barat" };
        db.Regions.Add(region);
        await db.SaveChangesAsync();

        db.Areas.AddRange(
            new Area { Name = "Area Bandung", RegionId = region.Id },
            new Area { Name = "Area Bekasi", RegionId = region.Id },
            new Area { Name = "Area Bogor", RegionId = region.Id }
        );
        await db.SaveChangesAsync();
    }

    private static async Task SeedDummySubscriptionsAsync(
        ApplicationDbContext db,
        Dictionary<string, ApplicationUser> users,
        List<Area> areas)
    {
        if (await db.Subscriptions.AnyAsync()) return;

        var salesUser = users.GetValueOrDefault("sales@pgn.id");
        var reviewerUser = users.GetValueOrDefault("reviewer@pgn.id");
        var divisionUser = users.GetValueOrDefault("division@pgn.id");
        if (salesUser is null || areas.Count == 0) return;

        var dummyData = new (string Company, string Address, int AreaIdx, SubscriptionStatus Status, bool SignedOff, int DaysAgo)[]
        {
            ("PT Maju Jaya Abadi", "Jl. Sudirman No. 45, Bandung", 0, SubscriptionStatus.Directory, false, 30),
            ("PT Sinar Harapan", "Jl. Gatot Subroto No. 12, Bandung", 0, SubscriptionStatus.Plotting, false, 25),
            ("CV Karya Mandiri", "Jl. Asia Afrika No. 88, Bandung", 0, SubscriptionStatus.Prospect, false, 20),
            ("PT Indo Gas Prima", "Jl. Cibaduyut No. 33, Bandung", 0, SubscriptionStatus.Survey, false, 15),
            ("PT Energi Nusantara", "Jl. Dago No. 100, Bandung", 0, SubscriptionStatus.A1, true, 10),
            ("PT Gasindo Perkasa", "Jl. Jababeka No. 7, Bekasi", 1, SubscriptionStatus.PermohonanNOL, false, 8),
            ("CV Baja Sentosa", "Jl. Kalimalang No. 55, Bekasi", 1, SubscriptionStatus.Directory, false, 18),
            ("PT Cipta Energi", "Jl. Pondok Gede No. 22, Bekasi", 1, SubscriptionStatus.Plotting, false, 12),
            ("PT Bogor Gas Industri", "Jl. Pajajaran No. 15, Bogor", 2, SubscriptionStatus.A1, false, 5),
            ("PT Sentosa Alam", "Jl. Raya Tajur No. 40, Bogor", 2, SubscriptionStatus.Disetujui, false, 45),
            ("CV Mitra Gas", "Jl. Ciawi No. 8, Bogor", 2, SubscriptionStatus.Ditolak, false, 35),
            ("PT Anggun Karya", "Jl. Otto Iskandardinata No. 60, Bandung", 0, SubscriptionStatus.Survey, false, 7),
        };

        foreach (var (company, address, areaIdx, status, signedOff, daysAgo) in dummyData)
        {
            var sub = new Subscription
            {
                CompanyName = company, Address = address,
                AreaId = areas[Math.Min(areaIdx, areas.Count - 1)].Id,
                Status = status, CreatedById = salesUser.Id, SignedOff = signedOff,
                CreatedAt = DateTime.UtcNow.AddDays(-daysAgo),
                UpdatedAt = DateTime.UtcNow.AddDays(-daysAgo + 2)
            };
            db.Subscriptions.Add(sub);
            await db.SaveChangesAsync();

            db.ActivityLogs.Add(new ActivityLog { SubscriptionId = sub.Id, ActorName = salesUser.FullName, Action = $"Membuat berlangganan {company}", At = DateTime.UtcNow.AddDays(-daysAgo) });

            if (status >= SubscriptionStatus.Plotting)
                db.SubmissionRecords.Add(new SubmissionRecord { SubscriptionId = sub.Id, Stage = SubscriptionStatus.Directory, FileName = "formulir-directory.pdf", FilePath = "/uploads/dummy-directory.pdf", UploadedById = salesUser.Id, UploadedAt = DateTime.UtcNow.AddDays(-daysAgo + 1) });

            if (status >= SubscriptionStatus.Prospect)
                db.SubmissionRecords.Add(new SubmissionRecord { SubscriptionId = sub.Id, Stage = SubscriptionStatus.Plotting, FileName = "pemetaan-plotting.pdf", FilePath = "/uploads/dummy-plotting.pdf", UploadedById = salesUser.Id, UploadedAt = DateTime.UtcNow.AddDays(-daysAgo + 2) });

            if (status >= SubscriptionStatus.Survey)
            {
                db.SubmissionRecords.Add(new SubmissionRecord { SubscriptionId = sub.Id, Stage = SubscriptionStatus.Prospect, FileName = "rencana-survey.pdf", FilePath = "/uploads/dummy-prospect.pdf", UploadedById = salesUser.Id, UploadedAt = DateTime.UtcNow.AddDays(-daysAgo + 3) });
                db.SubmissionRecords.Add(new SubmissionRecord { SubscriptionId = sub.Id, Stage = SubscriptionStatus.Survey, FileName = "hasil-survey.pdf", FilePath = "/uploads/dummy-survey.pdf", UploadedById = salesUser.Id, UploadedAt = DateTime.UtcNow.AddDays(-daysAgo + 4) });
            }

            if (status >= SubscriptionStatus.A1)
                db.SubmissionRecords.Add(new SubmissionRecord { SubscriptionId = sub.Id, Stage = SubscriptionStatus.A1, FileName = "lampiran-A1.pdf", FilePath = "/uploads/dummy-a1.pdf", UploadedById = salesUser.Id, UploadedAt = DateTime.UtcNow.AddDays(-daysAgo + 5) });

            if (status >= SubscriptionStatus.PermohonanNOL && reviewerUser is not null && divisionUser is not null)
            {
                sub.ReviewerIds = $"{reviewerUser.Id},{divisionUser.Id}";
                sub.CurrentReviewerIndex = status == SubscriptionStatus.Disetujui ? 3 : 1;
                db.ReviewSteps.Add(new ReviewStep { SubscriptionId = sub.Id, ReviewerId = reviewerUser.Id, StepOrder = 1, Action = status == SubscriptionStatus.Disetujui ? ReviewAction.Setuju : null, Comment = status == SubscriptionStatus.Disetujui ? "Dokumen lengkap." : "", ReviewedAt = status == SubscriptionStatus.Disetujui ? DateTime.UtcNow.AddDays(-3) : null });
                db.ReviewSteps.Add(new ReviewStep { SubscriptionId = sub.Id, ReviewerId = divisionUser.Id, StepOrder = 2, Action = status == SubscriptionStatus.Disetujui ? ReviewAction.Setuju : null, Comment = status == SubscriptionStatus.Disetujui ? "Disetujui." : "", ReviewedAt = status == SubscriptionStatus.Disetujui ? DateTime.UtcNow.AddDays(-2) : null });
            }

            if (status == SubscriptionStatus.Ditolak && reviewerUser is not null)
                db.ReviewSteps.Add(new ReviewStep { SubscriptionId = sub.Id, ReviewerId = reviewerUser.Id, StepOrder = 1, Action = ReviewAction.Tolak, Comment = "Dokumen tidak lengkap.", ReviewedAt = DateTime.UtcNow.AddDays(-daysAgo + 7) });

            if (status == SubscriptionStatus.Disetujui)
            {
                db.ActivityLogs.Add(new ActivityLog { SubscriptionId = sub.Id, ActorName = divisionUser?.FullName ?? "Division Head", Action = "Disetujui", At = DateTime.UtcNow.AddDays(-2) });
                db.ResumeEvaluasi.Add(new ResumeEvaluasi { SubscriptionId = sub.Id, Content = $"Resume evaluasi untuk {company}:\n\n1. Kelayakan teknis: Memenuhi\n2. Kelayakan finansial: Positif\n3. Rekomendasi: Disetujui", CreatedById = salesUser.Id, CreatedAt = DateTime.UtcNow.AddDays(-1), UpdatedAt = DateTime.UtcNow.AddDays(-1) });
            }

            await db.SaveChangesAsync();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
