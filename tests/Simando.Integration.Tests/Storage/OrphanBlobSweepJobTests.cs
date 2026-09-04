using Amazon.Runtime;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;
using Simando.Application.Storage;
using Simando.Domain.Attachments;
using Simando.Domain.Directory;
using Simando.Domain.Geography;
using Simando.Domain.MasterData;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Simando.Infrastructure.Storage;
using Testcontainers.Minio;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Storage;

public class OrphanBlobSweepJobTests : IAsyncLifetime
{
    private const string Bucket = "simando-sweep-test";

    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("imresamu/postgis:18-3.6-alpine")
        .WithDatabase("simando_sweep_test")
        .WithUsername("simando")
        .WithPassword("simando_pass")
        .Build();

    private readonly MinioContainer _minioContainer = new MinioBuilder("minio/minio:RELEASE.2023-01-31T02-24-19Z").Build();

    private IDbContextFactory<SimandoDbContext> _dbContextFactory = null!;
    private IAttachmentStore _store = null!;
    private OrphanBlobSweepJob _job = null!;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_dbContainer.StartAsync(), _minioContainer.StartAsync());

        _dbContextFactory = new SingleContextFactory(_dbContainer.GetConnectionString());

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        await db.Database.MigrateAsync();

        // Setup MinIO S3 bucket
        using var setupClient = new AmazonS3Client(
            new BasicAWSCredentials(_minioContainer.GetAccessKey(), _minioContainer.GetSecretKey()),
            new AmazonS3Config { ServiceURL = _minioContainer.GetConnectionString(), ForcePathStyle = true });
        await setupClient.PutBucketAsync(Bucket);

        _store = new S3AttachmentStore(Options.Create(new StorageOptions
        {
            Type = StorageProvider.S3,
            S3 = new S3StorageOptions
            {
                ServiceUrl = _minioContainer.GetConnectionString(),
                Bucket = Bucket,
                ForcePathStyle = true,
                AccessKey = _minioContainer.GetAccessKey(),
                SecretKey = _minioContainer.GetSecretKey(),
            },
        }));

        _job = new OrphanBlobSweepJob(_store, _dbContextFactory, NullLogger<OrphanBlobSweepJob>.Instance);
    }

    public async Task DisposeAsync()
    {
        (_store as IDisposable)?.Dispose();
        await Task.WhenAll(_dbContainer.DisposeAsync().AsTask(), _minioContainer.DisposeAsync().AsTask());
    }

    [Fact(DisplayName = "SweepOrphanBlobs deletes orphan blobs older than threshold and preserves registered blobs")]
    public async Task SweepOrphanBlobs_DeletesOrphans_PreservesRegistered()
    {
        // 1. Put orphan blob
        const string orphanKey = "company1/att1/v1/orphan.pdf";
        await _store.PutAsync(new BlobWriteRequest(orphanKey, "application/pdf"), new MemoryStream("orphan content"u8.ToArray()), CancellationToken.None);

        // 2. Put registered blob (and insert into db.Attachments)
        const string registeredKey = "company1/att2/v1/registered.pdf";
        await _store.PutAsync(new BlobWriteRequest(registeredKey, "application/pdf"), new MemoryStream("registered content"u8.ToArray()), CancellationToken.None);

        var companyId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();

        await using (var db = await _dbContextFactory.CreateDbContextAsync())
        {
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

            db.Companies.Add(new Company
            {
                Id = companyId,
                Nomor = "COMP-001",
                NamaPerusahaan = "PT Test Sweep",
                VillageId = villageId,
                Alamat = "Jl. Sweep Test",
                IndustryTypeId = industryTypeId,
                AreaId = areaId,
                CurrentStage = 1,
                Status = RecordStatus.Draft,
                CreatedBy = creator.Id,
                CreatedAt = DateTimeOffset.UtcNow,
            });

            db.Attachments.Add(new Attachment
            {
                Id = attachmentId,
                CompanyId = companyId,
                AttachableType = "company",
                AttachableId = companyId,
                Kind = AttachmentKind.Kk0,
                Version = 1,
                Filename = "registered.pdf",
                StorageProvider = StorageProvider.S3,
                StorageKey = registeredKey,
                MimeType = "application/pdf",
                SizeBytes = 100,
                Checksum = "checksum",
                UploadedBy = creator.Id,
                UploadedAt = DateTimeOffset.UtcNow,
            });

            await db.SaveChangesAsync();
        }

        // Run sweep with 0 threshold
        var result = await _job.SweepOrphanBlobsAsync(ageThreshold: TimeSpan.Zero, CancellationToken.None);

        result.ScannedCount.ShouldBe(2);
        result.OrphanCount.ShouldBe(1);
        result.DeletedCount.ShouldBe(1);

        (await _store.ExistsAsync(new StoredBlobRef(StorageProvider.S3, orphanKey), CancellationToken.None)).ShouldBeFalse();
        (await _store.ExistsAsync(new StoredBlobRef(StorageProvider.S3, registeredKey), CancellationToken.None)).ShouldBeTrue();
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
