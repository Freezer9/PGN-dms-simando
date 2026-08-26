using Microsoft.Extensions.Options;
using Simando.Domain.Attachments;

namespace Simando.Infrastructure.Storage;

// Checks only the selected Type's block (docs/build/storage.md §4) — a
// deployment that hasn't been given OneDrive credentials yet must still
// boot while running S3. Registered via .ValidateOnStart() so a misconfigured
// deployment fails at startup, not on a surveyor's first upload.
public sealed class StorageOptionsValidator : IValidateOptions<StorageOptions>
{
    public ValidateOptionsResult Validate(string? name, StorageOptions options)
    {
        return options.Type switch
        {
            StorageProvider.S3 => ValidateS3(options.S3),
            StorageProvider.OneDrive => ValidateOneDrive(options.OneDrive),
            _ => ValidateOptionsResult.Fail($"Unknown Storage:Type '{options.Type}'"),
        };
    }

    private static ValidateOptionsResult ValidateS3(S3StorageOptions s3)
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(s3.ServiceUrl)) missing.Add("Storage:S3:ServiceUrl");
        if (string.IsNullOrWhiteSpace(s3.Bucket)) missing.Add("Storage:S3:Bucket");
        if (string.IsNullOrWhiteSpace(s3.AccessKey)) missing.Add("Storage:S3:AccessKey");
        if (string.IsNullOrWhiteSpace(s3.SecretKey)) missing.Add("Storage:S3:SecretKey");

        return missing.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail($"Missing required S3 storage configuration: {string.Join(", ", missing)}");
    }

    private static ValidateOptionsResult ValidateOneDrive(OneDriveStorageOptions oneDrive)
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(oneDrive.TenantId)) missing.Add("Storage:OneDrive:TenantId");
        if (string.IsNullOrWhiteSpace(oneDrive.ClientId)) missing.Add("Storage:OneDrive:ClientId");
        if (string.IsNullOrWhiteSpace(oneDrive.ClientSecret)) missing.Add("Storage:OneDrive:ClientSecret");
        if (string.IsNullOrWhiteSpace(oneDrive.DriveId)) missing.Add("Storage:OneDrive:DriveId");

        return missing.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail($"Missing required OneDrive storage configuration: {string.Join(", ", missing)}");
    }
}
