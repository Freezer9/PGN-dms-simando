using Microsoft.EntityFrameworkCore;
using Shouldly;
using Simando.Application.Directory;
using Simando.Domain.Audit;
using Simando.Domain.Directory;
using Simando.Domain.Geography;
using Simando.Domain.MasterData;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Directory;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Directory;

// PostGIS integration tests, container-per-test isolation — same pattern as
// CompanyServiceTests.
public class CompanyPlottingContactTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("imresamu/postgis:18-3.6-alpine")
        .WithDatabase("simando")
        .WithUsername("simando")
        .WithPassword("simando")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = NewContext();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    [Fact(DisplayName = "SavePlottingAsync creates the Plotting row and bumps CurrentStage 1->2")]
    public async Task SavePlottingAsync_CreatesRow_BumpsStage()
    {
        var seed = await SeedAsync();
        var service = NewService();

        var result = await service.SavePlottingAsync(
            seed.CompanyId,
            new SavePlottingRequest(seed.CreatorId, PosisiPelanggan.JalurExisting, Kawasan.NonKawasanIndustri),
            seed.CreatorId, seed.CreatorPermissions);

        result.Succeeded.ShouldBeTrue();

        await using var verify = NewContext();
        var company = await verify.Companies.SingleAsync(c => c.Id == seed.CompanyId);
        company.CurrentStage.ShouldBe((byte)2);

        var plotting = await verify.Plottings.SingleAsync(p => p.CompanyId == seed.CompanyId);
        plotting.SalesUserId.ShouldBe(seed.CreatorId);
        plotting.PosisiPelanggan.ShouldBe(PosisiPelanggan.JalurExisting);
        plotting.Kawasan.ShouldBe(Kawasan.NonKawasanIndustri);
    }

    [Fact(DisplayName = "SavePlottingAsync called twice upserts, no duplicate-key error, stage stays 2")]
    public async Task SavePlottingAsync_CalledTwice_Upserts()
    {
        var seed = await SeedAsync();
        var service = NewService();

        await service.SavePlottingAsync(
            seed.CompanyId, new SavePlottingRequest(seed.CreatorId, PosisiPelanggan.JalurExisting, Kawasan.NonKawasanIndustri),
            seed.CreatorId, seed.CreatorPermissions);

        var second = await service.SavePlottingAsync(
            seed.CompanyId, new SavePlottingRequest(seed.CreatorId, PosisiPelanggan.Pengembangan, Kawasan.KawasanIndustri),
            seed.CreatorId, seed.CreatorPermissions);

        second.Succeeded.ShouldBeTrue();

        await using var verify = NewContext();
        (await verify.Plottings.CountAsync(p => p.CompanyId == seed.CompanyId)).ShouldBe(1);
        var company = await verify.Companies.SingleAsync(c => c.Id == seed.CompanyId);
        company.CurrentStage.ShouldBe((byte)2);
        var plotting = await verify.Plottings.SingleAsync(p => p.CompanyId == seed.CompanyId);
        plotting.PosisiPelanggan.ShouldBe(PosisiPelanggan.Pengembangan);
    }

    [Fact(DisplayName = "PromoteToProspekAsync bumps CurrentStage 2->3, rejected outside stage 2")]
    public async Task PromoteToProspekAsync_BumpsStage_RejectedOutsideStage2()
    {
        var seed = await SeedAsync();
        var service = NewService();

        var tooEarly = await service.PromoteToProspekAsync(seed.CompanyId, seed.CreatorId, seed.CreatorPermissions);
        tooEarly.Succeeded.ShouldBeFalse();

        await service.SavePlottingAsync(
            seed.CompanyId, new SavePlottingRequest(seed.CreatorId, PosisiPelanggan.JalurExisting, Kawasan.NonKawasanIndustri),
            seed.CreatorId, seed.CreatorPermissions);

        var promoted = await service.PromoteToProspekAsync(seed.CompanyId, seed.CreatorId, seed.CreatorPermissions);
        promoted.Succeeded.ShouldBeTrue();

        await using var verify = NewContext();
        (await verify.Companies.SingleAsync(c => c.Id == seed.CompanyId)).CurrentStage.ShouldBe((byte)3);

        var events = await verify.StatusEvents.Where(e => e.CompanyId == seed.CompanyId).ToListAsync();
        events.Any(e => e.Action == StatusEventAction.Save && e.ToStage == 2).ShouldBeTrue();
        events.Any(e => e.Action == StatusEventAction.Save && e.ToStage == 3).ShouldBeTrue();

        await service.AddContactAsync(
            seed.CompanyId, new SaveContactRequest("Budi", "Manager", null, null, null, null, null, true),
            seed.CreatorId, seed.CreatorPermissions);

        var again = await service.PromoteToProspekAsync(seed.CompanyId, seed.CreatorId, seed.CreatorPermissions);
        again.Succeeded.ShouldBeFalse();
    }

    [Fact(DisplayName = "Stage 1-3 writes are rejected once the record is no longer Draft")]
    public async Task Writes_RejectedOnceSubmitted()
    {
        var seed = await SeedAsync();
        var service = NewService();

        await using (var db = NewContext())
        {
            await db.Companies.Where(c => c.Id == seed.CompanyId)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.Status, RecordStatus.AreaHead));
        }

        var result = await service.SavePlottingAsync(
            seed.CompanyId, new SavePlottingRequest(seed.CreatorId, PosisiPelanggan.JalurExisting, Kawasan.NonKawasanIndustri),
            seed.CreatorId, seed.CreatorPermissions);

        result.Succeeded.ShouldBeFalse();
    }

    [Fact(DisplayName = "AddContactAsync: setting a second contact as primary unsets the first")]
    public async Task AddContactAsync_SecondPrimary_UnsetsFirst()
    {
        var seed = await SeedAsync();
        var service = NewService();

        await service.AddContactAsync(
            seed.CompanyId, new SaveContactRequest("Budi", "Direktur", null, null, null, null, null, true),
            seed.CreatorId, seed.CreatorPermissions);
        await service.AddContactAsync(
            seed.CompanyId, new SaveContactRequest("Sari", "Manajer", null, null, null, null, null, true),
            seed.CreatorId, seed.CreatorPermissions);

        var contacts = await service.GetContactsAsync(seed.CompanyId);
        contacts.Count.ShouldBe(2);
        contacts.Single(c => c.Nama == "Budi").IsPrimary.ShouldBeFalse();
        contacts.Single(c => c.Nama == "Sari").IsPrimary.ShouldBeTrue();
    }

    [Fact(DisplayName = "DeleteContactAsync removes a row but blocks deleting the last remaining contact")]
    public async Task DeleteContactAsync_BlocksLastContact()
    {
        var seed = await SeedAsync();
        var service = NewService();

        await service.AddContactAsync(
            seed.CompanyId, new SaveContactRequest("Budi", "Direktur", null, null, null, null, null, false),
            seed.CreatorId, seed.CreatorPermissions);
        await service.AddContactAsync(
            seed.CompanyId, new SaveContactRequest("Sari", "Manajer", null, null, null, null, null, false),
            seed.CreatorId, seed.CreatorPermissions);

        var contacts = await service.GetContactsAsync(seed.CompanyId);
        var first = contacts.First();

        var deleted = await service.DeleteContactAsync(seed.CompanyId, first.Id, seed.CreatorId, seed.CreatorPermissions);
        deleted.Succeeded.ShouldBeTrue();

        var remaining = await service.GetContactsAsync(seed.CompanyId);
        remaining.Count.ShouldBe(1);

        var blocked = await service.DeleteContactAsync(seed.CompanyId, remaining[0].Id, seed.CreatorId, seed.CreatorPermissions);
        blocked.Succeeded.ShouldBeFalse();
    }

    [Fact(DisplayName = "UpdateContactAsync edits an existing row's fields")]
    public async Task UpdateContactAsync_EditsFields()
    {
        var seed = await SeedAsync();
        var service = NewService();

        await service.AddContactAsync(
            seed.CompanyId, new SaveContactRequest("Budi", "Direktur", null, null, null, null, null, false),
            seed.CreatorId, seed.CreatorPermissions);
        var contact = (await service.GetContactsAsync(seed.CompanyId)).Single();

        var result = await service.UpdateContactAsync(
            seed.CompanyId, contact.Id,
            new SaveContactRequest("Budi Santoso", "Direktur Operasi", "budi@test.com", null, null, null, null, false),
            seed.CreatorId, seed.CreatorPermissions);

        result.Succeeded.ShouldBeTrue();
        var updated = (await service.GetContactsAsync(seed.CompanyId)).Single();
        updated.Nama.ShouldBe("Budi Santoso");
        updated.Jabatan.ShouldBe("Direktur Operasi");
        updated.Email.ShouldBe("budi@test.com");
    }

    private CompanyService NewService() => new(new SingleContextFactory(_container.GetConnectionString()));

    private sealed record SeedData(Guid CompanyId, Guid CreatorId, EffectivePermissions CreatorPermissions);

    private async Task<SeedData> SeedAsync()
    {
        await using var db = NewContext();

        var provinceId = Guid.NewGuid();
        var regencyId = Guid.NewGuid();
        var districtId = Guid.NewGuid();
        var villageId = Guid.NewGuid();
        var industryTypeId = Guid.NewGuid();
        var regionId = Guid.NewGuid();
        var areaId = Guid.NewGuid();

        db.Provinces.Add(new Province { Id = provinceId, BpsCode = "11", Name = "Test Province" });
        db.Regencies.Add(new Regency { Id = regencyId, ProvinceId = provinceId, BpsCode = "1101", Type = RegencyType.Kabupaten, Name = "Test Regency" });
        db.Districts.Add(new District { Id = districtId, RegencyId = regencyId, BpsCode = "110101", Name = "Test District" });
        db.Villages.Add(new Village { Id = villageId, DistrictId = districtId, BpsCode = "1101012001", Type = VillageType.Desa, Name = "Test Village" });
        db.IndustryTypes.Add(new IndustryType { Id = industryTypeId, Name = $"Test Industry {Guid.NewGuid():N}" });
        db.Regions.Add(new Region { Id = regionId, Code = Guid.NewGuid().ToString("N")[..8], Name = "Test Region", Active = true });
        db.Areas.Add(new Area { Id = areaId, RegionId = regionId, Code = Guid.NewGuid().ToString("N")[..8], Name = "Test Area", Active = true });

        var creator = new ApplicationUser { Id = Guid.NewGuid(), UserName = "creator", FullName = "creator" };
        db.Users.Add(creator);

        await db.SaveChangesAsync();

        var permissions = new EffectivePermissions(
            AccessScope.Area, areaId, null, RoleCapabilities.For(Role.SalesArea).Capabilities);

        var companyId = Guid.NewGuid();
        db.Companies.Add(new Company
        {
            Id = companyId,
            Nomor = $"9999999-11-{Guid.NewGuid().ToString("N")[..2]}",
            NamaPerusahaan = "PT Test",
            VillageId = villageId,
            Alamat = "Jl. Test",
            IndustryTypeId = industryTypeId,
            AreaId = areaId,
            CurrentStage = 1,
            Status = RecordStatus.Draft,
            CreatedBy = creator.Id,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();

        return new SeedData(companyId, creator.Id, permissions);
    }

    private SimandoDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<SimandoDbContext>()
            .UseNpgsql(_container.GetConnectionString(), npgsql => npgsql.UseNetTopologySuite())
            .UseSnakeCaseNamingConvention()
            .Options;

        return new SimandoDbContext(options, new UnrestrictedCurrentUser());
    }

    private sealed class SingleContextFactory(string connectionString) : IDbContextFactory<SimandoDbContext>
    {
        public SimandoDbContext CreateDbContext() => Build();

        public Task<SimandoDbContext> CreateDbContextAsync(CancellationToken ct = default) => Task.FromResult(Build());

        private SimandoDbContext Build()
        {
            var options = new DbContextOptionsBuilder<SimandoDbContext>()
                .UseNpgsql(connectionString, npgsql => npgsql.UseNetTopologySuite())
                .UseSnakeCaseNamingConvention()
                .Options;

            return new SimandoDbContext(options, new UnrestrictedCurrentUser());
        }
    }
}
