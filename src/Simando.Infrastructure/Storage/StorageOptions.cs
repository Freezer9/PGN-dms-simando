using Simando.Domain.Attachments;

namespace Simando.Infrastructure.Storage;

// Binds the "Storage" config section (docs/build/storage.md §4). Both
// provider blocks are always present in config so a switch-over deployment
// can register both, but only the selected Type's block is validated on
// start — see StorageOptionsValidator.
public sealed class StorageOptions
{
    public StorageProvider Type { get; init; }
    public S3StorageOptions S3 { get; init; } = new();
    public OneDriveStorageOptions OneDrive { get; init; } = new();
}

public sealed class S3StorageOptions
{
    public string ServiceUrl { get; init; } = "";
    public string Bucket { get; init; } = "";
    public string Region { get; init; } = "us-east-1";
    public bool ForcePathStyle { get; init; } = true;
    public string AccessKey { get; init; } = "";
    public string SecretKey { get; init; } = "";
}

// Fields only — OneDriveAttachmentStore is storage-onedriveattachmentstore-
// dual-provider-reso-2026-08-07, not this task. Kept here so the config
// section binds in full regardless of which provider is selected.
public sealed class OneDriveStorageOptions
{
    public string TenantId { get; init; } = "";
    public string ClientId { get; init; } = "";
    public string ClientSecret { get; init; } = "";
    public string DriveId { get; init; } = "";
}
