using Simando.Domain.Attachments;

namespace Simando.Application.Storage;

// Four operations only — put, open-read, exists, delete-orphan. Never a
// plain delete: retention is indefinite, DeleteOrphanAsync is named for
// its one legitimate caller (the nightly sweep of blobs with no matching
// attachment row). Versioning, naming, and access control all stay in the
// application, above this interface — see docs/build/storage.md §1-2.
public interface IAttachmentStore
{
    StorageProvider Provider { get; }

    Task<StoredBlob> PutAsync(BlobWriteRequest request, Stream content, CancellationToken ct);
    Task<Stream> OpenReadAsync(StoredBlobRef blob, CancellationToken ct);
    Task<bool> ExistsAsync(StoredBlobRef blob, CancellationToken ct);
    Task<IReadOnlyList<BlobMetadata>> ListBlobsAsync(CancellationToken ct);
    Task DeleteOrphanAsync(StoredBlobRef blob, CancellationToken ct);
}

public sealed record BlobWriteRequest(string Key, string ContentType);

public sealed record StoredBlob(StorageProvider Provider, string Key, string? ETag, long SizeBytes);

public sealed record StoredBlobRef(StorageProvider Provider, string Key);

public sealed record BlobMetadata(string Key, DateTimeOffset LastModified, long SizeBytes);
