namespace Simando.Application.Storage;

// Typed, not a provider exception leaking through IAttachmentStore — S3's
// AmazonS3Exception and (eventually) Graph's ServiceException must not
// escape the abstraction. docs/build/storage.md §7, ST4.
public sealed class BlobNotFoundException(StoredBlobRef blob)
    : Exception($"Blob not found: {blob.Provider}/{blob.Key}")
{
    public StoredBlobRef Blob { get; } = blob;
}
