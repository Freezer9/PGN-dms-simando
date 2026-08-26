namespace Simando.Infrastructure.Options;

public sealed class UploadOptions
{
    public long MaxSizeMb { get; set; } = 25;
    public string[] AllowedTypes { get; set; } = [];

    public long MaxSizeBytes => MaxSizeMb * 1024 * 1024;
}
