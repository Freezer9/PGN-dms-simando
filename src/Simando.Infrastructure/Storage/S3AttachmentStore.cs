using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Simando.Application.Storage;
using Simando.Domain.Attachments;

namespace Simando.Infrastructure.Storage;

// v1 provider — PGN's actual target is OneDrive, but we don't have tenant
// access yet, so this runs against MinIO/RustFS (S3-compatible) instead.
// docs/build/storage.md §0, §3.
public sealed class S3AttachmentStore : IAttachmentStore, IDisposable
{
    private readonly AmazonS3Client _client;
    private readonly string _bucket;

    public S3AttachmentStore(IOptions<StorageOptions> options)
    {
        var s3 = options.Value.S3;
        _bucket = s3.Bucket;
        _client = new AmazonS3Client(
            new BasicAWSCredentials(s3.AccessKey, s3.SecretKey),
            new AmazonS3Config
            {
                ServiceURL = s3.ServiceUrl,
                ForcePathStyle = s3.ForcePathStyle,
                AuthenticationRegion = s3.Region,
                // This app only ever targets S3-compatible servers (MinIO/
                // RustFS), never real AWS S3. The SDK v4 default of
                // computing a request checksum unconditionally sends a
                // chunked streaming payload with a trailing checksum, which
                // these servers don't reliably support and rejects with
                // "x-amz-content-sha256 header does not match what was
                // computed". WHEN_REQUIRED only adds a checksum when the
                // API mandates one, avoiding the incompatible chunked path.
                RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
            });
    }

    public StorageProvider Provider => StorageProvider.S3;

    public async Task EnsureBucketExistsAsync(CancellationToken ct = default)
    {
        try
        {
            await _client.PutBucketAsync(_bucket, ct);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode is "BucketAlreadyOwnedByYou" or "BucketAlreadyExists" || ex.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.OK)
        {
            // Bucket already exists.
        }
    }

    public async Task<StoredBlob> PutAsync(BlobWriteRequest request, Stream content, CancellationToken ct)
    {
        try
        {
            var response = await _client.PutObjectAsync(
                new PutObjectRequest
                {
                    BucketName = _bucket,
                    Key = request.Key,
                    InputStream = content,
                    ContentType = request.ContentType,
                    AutoCloseStream = false,
                },
                ct);

            return new StoredBlob(StorageProvider.S3, request.Key, response.ETag, content.Length);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode is "NoSuchBucket" || ex.StatusCode == HttpStatusCode.NotFound)
        {
            await EnsureBucketExistsAsync(ct);
            if (content.CanSeek)
            {
                content.Position = 0;
            }

            var response = await _client.PutObjectAsync(
                new PutObjectRequest
                {
                    BucketName = _bucket,
                    Key = request.Key,
                    InputStream = content,
                    ContentType = request.ContentType,
                    AutoCloseStream = false,
                },
                ct);

            return new StoredBlob(StorageProvider.S3, request.Key, response.ETag, content.Length);
        }
    }

    public async Task<Stream> OpenReadAsync(StoredBlobRef blob, CancellationToken ct)
    {
        try
        {
            var response = await _client.GetObjectAsync(
                new GetObjectRequest { BucketName = _bucket, Key = blob.Key },
                ct);
            return response.ResponseStream;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new BlobNotFoundException(blob);
        }
    }

    public async Task<bool> ExistsAsync(StoredBlobRef blob, CancellationToken ct)
    {
        try
        {
            await _client.GetObjectMetadataAsync(
                new GetObjectMetadataRequest { BucketName = _bucket, Key = blob.Key },
                ct);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<BlobMetadata>> ListBlobsAsync(CancellationToken ct)
    {
        var blobs = new List<BlobMetadata>();
        string? continuationToken = null;
        do
        {
            var response = await _client.ListObjectsV2Async(
                new ListObjectsV2Request
                {
                    BucketName = _bucket,
                    ContinuationToken = continuationToken,
                },
                ct);

            foreach (var s3Obj in response.S3Objects)
            {
                DateTimeOffset lastModified = s3Obj.LastModified.HasValue
                    ? DateTime.SpecifyKind(s3Obj.LastModified.Value, DateTimeKind.Utc)
                    : DateTimeOffset.UtcNow;

                blobs.Add(new BlobMetadata(s3Obj.Key, lastModified, s3Obj.Size ?? 0L));
            }

            continuationToken = response.IsTruncated == true ? response.NextContinuationToken : null;
        }
        while (continuationToken is not null);

        return blobs;
    }

    public Task DeleteOrphanAsync(StoredBlobRef blob, CancellationToken ct) =>
        _client.DeleteObjectAsync(_bucket, blob.Key, ct);

    public void Dispose() => _client.Dispose();
}
