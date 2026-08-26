using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Simando.Application.Storage;
using Simando.Infrastructure.Persistence;

namespace Simando.Infrastructure.Storage;

public sealed class OrphanBlobSweepJob : IOrphanBlobSweepJob
{
    private readonly IAttachmentStore _attachmentStore;
    private readonly IDbContextFactory<SimandoDbContext> _dbContextFactory;
    private readonly ILogger<OrphanBlobSweepJob> _logger;

    public OrphanBlobSweepJob(
        IAttachmentStore attachmentStore,
        IDbContextFactory<SimandoDbContext> dbContextFactory,
        ILogger<OrphanBlobSweepJob> logger)
    {
        _attachmentStore = attachmentStore;
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task<OrphanBlobSweepResult> SweepOrphanBlobsAsync(TimeSpan? ageThreshold = null, CancellationToken ct = default)
    {
        var threshold = ageThreshold ?? TimeSpan.FromHours(24);
        var cutoffTime = DateTimeOffset.UtcNow.Subtract(threshold);

        _logger.LogInformation("Starting orphan blob sweep with threshold {ThresholdHours} hours (cutoff: {Cutoff})", threshold.TotalHours, cutoffTime);

        var allBlobs = await _attachmentStore.ListBlobsAsync(ct);
        var eligibleBlobs = allBlobs.Where(b => b.LastModified <= cutoffTime).ToList();

        if (eligibleBlobs.Count == 0)
        {
            _logger.LogInformation("No blobs older than cutoff time found. Scanned total: {ScannedCount}", allBlobs.Count);
            return new OrphanBlobSweepResult(allBlobs.Count, 0, 0);
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        var registeredStorageKeys = (await db.Attachments.AsNoTracking()
            .Select(a => a.StorageKey)
            .ToListAsync(ct))
            .ToHashSet(StringComparer.Ordinal);

        var orphanBlobs = eligibleBlobs
            .Where(b => !registeredStorageKeys.Contains(b.Key))
            .ToList();

        _logger.LogInformation("Found {OrphanCount} orphan blobs out of {EligibleCount} eligible blobs (Total scanned: {ScannedCount})",
            orphanBlobs.Count, eligibleBlobs.Count, allBlobs.Count);

        var deletedCount = 0;
        foreach (var orphan in orphanBlobs)
        {
            try
            {
                await _attachmentStore.DeleteOrphanAsync(new StoredBlobRef(_attachmentStore.Provider, orphan.Key), ct);
                deletedCount++;
                _logger.LogInformation("Deleted orphan blob {Key}", orphan.Key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete orphan blob {Key}", orphan.Key);
            }
        }

        return new OrphanBlobSweepResult(allBlobs.Count, orphanBlobs.Count, deletedCount);
    }
}
