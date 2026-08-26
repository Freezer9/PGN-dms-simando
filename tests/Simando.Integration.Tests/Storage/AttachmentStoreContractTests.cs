using System.Security.Cryptography;
using System.Text;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Shouldly;
using Simando.Application.Storage;
using Simando.Domain.Attachments;
using Simando.Infrastructure.Storage;
using Testcontainers.Minio;

namespace Simando.Integration.Tests.Storage;

// Contract suite from docs/build/storage.md §7, run against real MinIO via
// Testcontainers — container-per-test-class, same isolation shape as the
// Postgres integration tests. ST5/ST8/ST9 are excluded: they exercise the
// dual-provider resolver and OneDrive-only retry behaviour, both out of
// scope for this task (storage-onedriveattachmentstore-dual-provider-reso).
public class AttachmentStoreContractTests : IAsyncLifetime
{
    private const string Bucket = "simando-test";

    private readonly MinioContainer _container = new MinioBuilder("minio/minio:RELEASE.2023-01-31T02-24-19Z").Build();
    private IAttachmentStore _store = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        using var setupClient = new AmazonS3Client(
            new BasicAWSCredentials(_container.GetAccessKey(), _container.GetSecretKey()),
            new AmazonS3Config { ServiceURL = _container.GetConnectionString(), ForcePathStyle = true });
        await setupClient.PutBucketAsync(Bucket);

        _store = new S3AttachmentStore(Options.Create(new StorageOptions
        {
            Type = StorageProvider.S3,
            S3 = new S3StorageOptions
            {
                ServiceUrl = _container.GetConnectionString(),
                Bucket = Bucket,
                ForcePathStyle = true,
                AccessKey = _container.GetAccessKey(),
                SecretKey = _container.GetSecretKey(),
            },
        }));
    }

    public async Task DisposeAsync()
    {
        (_store as IDisposable)?.Dispose();
        await _container.DisposeAsync();
    }

    [Fact(DisplayName = "ST1: put then read returns a byte-identical stream")]
    public async Task PutThenRead_ByteIdentical()
    {
        var content = "signed KK0 contents"u8.ToArray();

        await _store.PutAsync(new BlobWriteRequest("st1/file.pdf", "application/pdf"), new MemoryStream(content), CancellationToken.None);
        var readBack = await ReadAllAsync(new StoredBlobRef(StorageProvider.S3, "st1/file.pdf"));

        readBack.ShouldBe(content);
    }

    [Fact(DisplayName = "ST2: put 5 MB succeeds, crossing the 4 MB simple-upload boundary")]
    public async Task Put5Mb_Succeeds()
    {
        var content = new byte[5 * 1024 * 1024];
        Random.Shared.NextBytes(content);

        var blob = await _store.PutAsync(new BlobWriteRequest("st2/large.bin", "application/octet-stream"), new MemoryStream(content), CancellationToken.None);

        blob.SizeBytes.ShouldBe(content.LongLength);
        (await ReadAllAsync(new StoredBlobRef(StorageProvider.S3, "st2/large.bin"))).ShouldBe(content);
    }

    [Fact(DisplayName = "ST3: put twice at different versions produces two distinct, both-readable blobs")]
    public async Task PutTwiceAtDifferentVersions_BothReadable()
    {
        var v1 = "version one"u8.ToArray();
        var v2 = "version two"u8.ToArray();

        await _store.PutAsync(new BlobWriteRequest("st3/doc/v1/file.pdf", "application/pdf"), new MemoryStream(v1), CancellationToken.None);
        await _store.PutAsync(new BlobWriteRequest("st3/doc/v2/file.pdf", "application/pdf"), new MemoryStream(v2), CancellationToken.None);

        (await ReadAllAsync(new StoredBlobRef(StorageProvider.S3, "st3/doc/v1/file.pdf"))).ShouldBe(v1);
        (await ReadAllAsync(new StoredBlobRef(StorageProvider.S3, "st3/doc/v2/file.pdf"))).ShouldBe(v2);
    }

    [Fact(DisplayName = "ST4: reading a missing key throws a typed BlobNotFoundException, not a provider exception")]
    public async Task ReadMissingKey_ThrowsBlobNotFound()
    {
        var blob = new StoredBlobRef(StorageProvider.S3, "st4/does-not-exist.pdf");

        (await _store.ExistsAsync(blob, CancellationToken.None)).ShouldBeFalse();
        await Should.ThrowAsync<BlobNotFoundException>(() => _store.OpenReadAsync(blob, CancellationToken.None));
    }

    [Fact(DisplayName = "ST6: filename with spaces, '#', and non-ASCII round-trips without rename")]
    public async Task NonAsciiFilename_RoundTrips()
    {
        const string key = "st6/laporan survei # lapangan (jalan üöä).pdf";
        var content = "isi berkas"u8.ToArray();

        await _store.PutAsync(new BlobWriteRequest(key, "application/pdf"), new MemoryStream(content), CancellationToken.None);
        var blob = new StoredBlobRef(StorageProvider.S3, key);

        (await _store.ExistsAsync(blob, CancellationToken.None)).ShouldBeTrue();
        (await ReadAllAsync(blob)).ShouldBe(content);
    }

    [Fact(DisplayName = "ST7: startup with the selected type's config incomplete fails validation")]
    public void IncompleteS3Config_FailsValidation()
    {
        var validator = new StorageOptionsValidator();

        var result = validator.Validate(
            name: null,
            new StorageOptions { Type = StorageProvider.S3, S3 = new S3StorageOptions { Bucket = Bucket } });

        result.Failed.ShouldBeTrue();
    }

    [Fact(DisplayName = "ST10: checksum after round-trip matches the value computed at upload")]
    public async Task ChecksumAfterRoundTrip_Matches()
    {
        var content = "checksum me"u8.ToArray();
        var uploadChecksum = Convert.ToHexString(SHA256.HashData(content));

        await _store.PutAsync(new BlobWriteRequest("st10/file.txt", "text/plain"), new MemoryStream(content), CancellationToken.None);
        var readBack = await ReadAllAsync(new StoredBlobRef(StorageProvider.S3, "st10/file.txt"));

        Convert.ToHexString(SHA256.HashData(readBack)).ShouldBe(uploadChecksum);
    }

    private async Task<byte[]> ReadAllAsync(StoredBlobRef blob)
    {
        await using var stream = await _store.OpenReadAsync(blob, CancellationToken.None);
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer);
        return buffer.ToArray();
    }
}
