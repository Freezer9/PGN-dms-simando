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
        await SeedMasterDataAsync(db);

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

    /// <summary>
    /// Starter reference lists. Enough for the stage forms to render real dropdowns;
    /// the full catalogues are maintained through /settings/master-data.
    /// </summary>
    private static async Task SeedMasterDataAsync(ApplicationDbContext db)
    {
        if (await db.MasterData.AnyAsync()) return;

        var entries = new List<MasterDataEntry>();

        void Add(MasterCategory category, params (string Code, string Name, string Attributes)[] items)
        {
            var order = 0;
            foreach (var (code, name, attributes) in items)
                entries.Add(new MasterDataEntry
                {
                    Category = category, Code = code, Name = name,
                    AttributesJson = attributes, SortOrder = order++
                });
        }

        Add(MasterCategory.Segment,
            ("B1", "Bronze 1", ""), ("B2", "Bronze 2", ""), ("B3", "Bronze 3", ""),
            ("SLV", "Silver", ""), ("GLD", "Gold", ""), ("PLT", "Platinum", ""));

        Add(MasterCategory.IndustryType,
            ("MKN", "Makanan & Minuman", ""), ("TKS", "Tekstil", ""), ("KRM", "Keramik", ""),
            ("KIM", "Kimia", ""), ("LOG", "Logam", ""), ("KRT", "Kertas", ""), ("LAI", "Lainnya", ""));

        Add(MasterCategory.Country,
            ("ID", "Indonesia", ""), ("CN", "Tiongkok", ""), ("SG", "Singapura", ""),
            ("MY", "Malaysia", ""), ("JP", "Jepang", ""), ("TH", "Thailand", ""));

        Add(MasterCategory.FuelType,
            ("LPG", "LPG", ""), ("SOL", "Solar / HSD", ""), ("MFO", "Marine Fuel Oil", ""),
            ("BTB", "Batu Bara", ""), ("KAY", "Kayu Bakar", ""), ("LST", "Listrik", ""));

        Add(MasterCategory.UnitOfMeasure,
            ("M3", "m³", ""), ("MMBTU", "MMBtu", ""), ("KG", "kg", ""), ("TON", "ton", ""),
            ("LTR", "liter", ""), ("KWH", "kWh", ""), ("BARG", "barg", ""), ("INCH", "inch", ""));

        // Meter sizes carry nominal/max flow and pressure — the evaluation form reads these.
        Add(MasterCategory.MeterSize,
            ("G4", "G4", """{"nominalFlowM3h":6,"maxFlowM3h":10,"maxPressureBarg":4}"""),
            ("G6", "G6", """{"nominalFlowM3h":10,"maxFlowM3h":16,"maxPressureBarg":4}"""),
            ("G10", "G10", """{"nominalFlowM3h":16,"maxFlowM3h":25,"maxPressureBarg":4}"""),
            ("G16", "G16", """{"nominalFlowM3h":25,"maxFlowM3h":40,"maxPressureBarg":10}"""),
            ("G25", "G25", """{"nominalFlowM3h":40,"maxFlowM3h":65,"maxPressureBarg":10}"""),
            ("G40", "G40", """{"nominalFlowM3h":65,"maxFlowM3h":100,"maxPressureBarg":16}"""),
            ("G65", "G65", """{"nominalFlowM3h":100,"maxFlowM3h":160,"maxPressureBarg":16}"""),
            ("G100", "G100", """{"nominalFlowM3h":160,"maxFlowM3h":250,"maxPressureBarg":16}"""));

        Add(MasterCategory.MrsSpec,
            ("MRS-A", "MRS Tipe A", ""), ("MRS-B", "MRS Tipe B", ""),
            ("MRS-C", "MRS Tipe C", ""), ("RS-ONLY", "Regulating Station", ""));

        Add(MasterCategory.ReferenceDocument,
            ("KK0", "Formulir Survei KK0", ""), ("A1", "Formulir Registrasi A1", ""),
            ("L15", "Lampiran 15 — Nota Dinas", ""), ("L16", "Lampiran 16 — NOL/RL", ""),
            ("L17", "Lampiran 17 — Evaluasi", ""));

        Add(MasterCategory.ReasonCategory,
            ("DOK", "Dokumen tidak lengkap", ""), ("DAT", "Data tidak sesuai", ""),
            ("TEK", "Kendala teknis", ""), ("KOM", "Kendala komersial", ""), ("LAI", "Lainnya", ""));

        db.MasterData.AddRange(entries);
        await db.SaveChangesAsync();
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

    /// <summary>
    /// Map pins for the demo companies, approximated from each street address so the Peta page
    /// and the plotting flow have realistic points to render. Keyed by company name so the same
    /// table serves both first-run seeding and the backfill below.
    /// </summary>
    private static readonly Dictionary<string, (double Lat, double Lng)> DemoCoordinates = new()
    {
        ["PT Maju Jaya Abadi"] = (-6.9210, 107.5940),      // Jl. Sudirman, Bandung
        ["PT Sinar Harapan"] = (-6.9330, 107.6260),        // Jl. Gatot Subroto, Bandung
        ["CV Karya Mandiri"] = (-6.9214, 107.6091),        // Jl. Asia Afrika, Bandung
        ["PT Indo Gas Prima"] = (-6.9490, 107.5850),       // Jl. Cibaduyut, Bandung
        ["PT Energi Nusantara"] = (-6.8890, 107.6130),     // Jl. Dago, Bandung
        ["PT Gasindo Perkasa"] = (-6.2790, 107.1520),      // Jl. Jababeka, Cikarang
        ["CV Baja Sentosa"] = (-6.2450, 106.9920),         // Jl. Kalimalang, Bekasi
        ["PT Cipta Energi"] = (-6.2850, 106.9250),         // Jl. Pondok Gede, Bekasi
        ["PT Bogor Gas Industri"] = (-6.5900, 106.8060),   // Jl. Pajajaran, Bogor
        ["PT Sentosa Alam"] = (-6.6280, 106.8250),         // Jl. Raya Tajur, Bogor
        ["CV Mitra Gas"] = (-6.6600, 106.8420),            // Jl. Ciawi, Bogor
        ["PT Anggun Karya"] = (-6.9260, 107.6040),         // Jl. Otto Iskandardinata, Bandung
    };

    /// <summary>
    /// Fills in map pins on demo companies seeded before coordinates existed. Idempotent and
    /// only touches rows that are still missing a pin, so hand-placed points are never moved.
    /// </summary>
    private static async Task BackfillCoordinatesAsync(ApplicationDbContext db)
    {
        var missing = await db.Subscriptions
            .Where(s => s.Latitude == null || s.Longitude == null)
            .ToListAsync();

        var updated = 0;
        foreach (var sub in missing)
        {
            if (!DemoCoordinates.TryGetValue(sub.CompanyName, out var point)) continue;
            sub.Latitude = point.Lat;
            sub.Longitude = point.Lng;
            updated++;
        }

        if (updated > 0) await db.SaveChangesAsync();
    }

    private static async Task SeedDummySubscriptionsAsync(
        ApplicationDbContext db,
        Dictionary<string, ApplicationUser> users,
        List<Area> areas)
    {
        if (await db.Subscriptions.AnyAsync())
        {
            await BackfillCoordinatesAsync(db);
            return;
        }

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
            var point = DemoCoordinates.TryGetValue(company, out var coords)
                ? ((double?)coords.Lat, (double?)coords.Lng)
                : (null, null);

            var sub = new Subscription
            {
                CompanyName = company, Address = address,
                AreaId = areas[Math.Min(areaIdx, areas.Count - 1)].Id,
                Latitude = point.Item1, Longitude = point.Item2,
                Status = status, CreatedById = salesUser.Id, SignedOff = signedOff,
                CreatedAt = DateTime.UtcNow.AddDays(-daysAgo),
                UpdatedAt = DateTime.UtcNow.AddDays(-daysAgo + 2)
            };
            db.Subscriptions.Add(sub);
            await db.SaveChangesAsync();

            db.ActivityLogs.Add(new ActivityLog { SubscriptionId = sub.Id, ActorName = salesUser.FullName, Action = $"Membuat berlangganan {company}", At = DateTime.UtcNow.AddDays(-daysAgo) });

            if (SubscriptionStages.IsAtOrAfter(status, SubscriptionStatus.Plotting))
                db.SubmissionRecords.Add(new SubmissionRecord { SubscriptionId = sub.Id, Stage = SubscriptionStatus.Directory, FileName = "formulir-directory.pdf", FilePath = "/uploads/dummy-directory.pdf", UploadedById = salesUser.Id, UploadedAt = DateTime.UtcNow.AddDays(-daysAgo + 1) });

            if (SubscriptionStages.IsAtOrAfter(status, SubscriptionStatus.Prospect))
                db.SubmissionRecords.Add(new SubmissionRecord { SubscriptionId = sub.Id, Stage = SubscriptionStatus.Plotting, FileName = "pemetaan-plotting.pdf", FilePath = "/uploads/dummy-plotting.pdf", UploadedById = salesUser.Id, UploadedAt = DateTime.UtcNow.AddDays(-daysAgo + 2) });

            if (SubscriptionStages.IsAtOrAfter(status, SubscriptionStatus.Survey))
            {
                db.SubmissionRecords.Add(new SubmissionRecord { SubscriptionId = sub.Id, Stage = SubscriptionStatus.Prospect, FileName = "rencana-survey.pdf", FilePath = "/uploads/dummy-prospect.pdf", UploadedById = salesUser.Id, UploadedAt = DateTime.UtcNow.AddDays(-daysAgo + 3) });
                db.SubmissionRecords.Add(new SubmissionRecord { SubscriptionId = sub.Id, Stage = SubscriptionStatus.Survey, FileName = "hasil-survey.pdf", FilePath = "/uploads/dummy-survey.pdf", UploadedById = salesUser.Id, UploadedAt = DateTime.UtcNow.AddDays(-daysAgo + 4) });
            }

            if (SubscriptionStages.IsAtOrAfter(status, SubscriptionStatus.A1))
                db.SubmissionRecords.Add(new SubmissionRecord { SubscriptionId = sub.Id, Stage = SubscriptionStatus.A1, FileName = "lampiran-A1.pdf", FilePath = "/uploads/dummy-a1.pdf", UploadedById = salesUser.Id, UploadedAt = DateTime.UtcNow.AddDays(-daysAgo + 5) });

            if (SubscriptionStages.IsAtOrAfter(status, SubscriptionStatus.PermohonanNOL) && reviewerUser is not null && divisionUser is not null)
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
