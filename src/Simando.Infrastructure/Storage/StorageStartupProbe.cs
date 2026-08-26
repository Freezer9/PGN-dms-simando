using System.Text;
using Microsoft.Extensions.Hosting;
using Simando.Application.Storage;

namespace Simando.Infrastructure.Storage;

// Writes and deletes a tiny object under __healthcheck/ on boot, per
// docs/build/storage.md §4: a misconfigured storage layer should surface
// as "container refuses to start", not as a 500 the first time someone
// presses Unggah.
public sealed class StorageStartupProbe(IAttachmentStore store) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        var blob = new StoredBlobRef(store.Provider, $"__healthcheck/{Guid.NewGuid()}");

        await store.PutAsync(
            new BlobWriteRequest(blob.Key, "text/plain"),
            new MemoryStream(Encoding.UTF8.GetBytes("simando storage startup probe")),
            ct);

        await store.DeleteOrphanAsync(blob, ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
