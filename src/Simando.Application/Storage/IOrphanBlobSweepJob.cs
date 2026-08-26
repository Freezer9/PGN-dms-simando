namespace Simando.Application.Storage;

public sealed record OrphanBlobSweepResult(int ScannedCount, int OrphanCount, int DeletedCount);

public interface IOrphanBlobSweepJob
{
    Task<OrphanBlobSweepResult> SweepOrphanBlobsAsync(TimeSpan? ageThreshold = null, CancellationToken ct = default);
}
