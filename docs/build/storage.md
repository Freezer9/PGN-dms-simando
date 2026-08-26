# Build — Attachment Storage

> **Canonical.** This document owns the storage abstraction, its configuration and
> the provider-specific behaviour. [architecture](architecture.md) references
> it rather than restating it.

PGN intends to keep attachments in **OneDrive**. We do not have access to their
tenant, so v1 runs on **MinIO** (S3-compatible, self-hosted) — see
[docs/future](../future/README.md#onedrive-attachment-storage) for what's
needed to switch. Both are supported behind one interface, selected by
configuration.

```jsonc
"Storage": { "Type": "S3" }        // or "OneDrive"
```

---

## 1. What is actually being abstracted

The temptation is to model "a filesystem". Resist it — the two providers disagree
about directories, identity, versioning and auth, and an abstraction that pretends
otherwise will leak at exactly the wrong moment.

**The application needs four things from storage, and nothing else:**

1. Put a byte stream somewhere and get back a durable handle.
2. Open that handle for reading later.
3. Ask whether it still exists.
4. Delete — **only** for orphan cleanup, never as a business operation.

Everything else — versioning, naming, access control, audit — stays in the
application, where it already lives. That is the whole design.

```csharp
public interface IAttachmentStore
{
    StorageProvider Provider { get; }

    Task<StoredBlob> PutAsync(BlobWriteRequest request, Stream content, CancellationToken ct);
    Task<Stream>     OpenReadAsync(StoredBlobRef blob, CancellationToken ct);
    Task<bool>       ExistsAsync(StoredBlobRef blob, CancellationToken ct);
    Task             DeleteOrphanAsync(StoredBlobRef blob, CancellationToken ct);
}

public sealed record StoredBlob(StorageProvider Provider, string Key, string? ETag, long SizeBytes);
public sealed record StoredBlobRef(StorageProvider Provider, string Key);

public enum StorageProvider { S3, OneDrive }
```

`DeleteOrphanAsync` is named for its only legitimate caller. Retention policy is
**never hard-delete** ([architecture](architecture.md#non-functional)), so a
plain `DeleteAsync` on this interface would be an invitation.

---

## 2. Three things the application keeps, and must not delegate

### Versioning is ours

`attachment` is versioned already — re-upload supersedes, never overwrites — and
**each version is a distinct blob under its own key.** Neither provider's own
version history is used.

This matters more for OneDrive than for S3, because OneDrive *has* a good version
history and it is user-facing. If we relied on it, anyone with access to the
document library could "restore previous version" from the web UI and silently
change what a signed NOL points at. The record would still say version 3; the bytes
would be version 2. Nothing in the application would notice.

One blob per version, written once, never modified.

### Identity is the provider's, not the path

The key we store is **whatever that provider needs to find the blob again**, and
the two are not the same kind of thing:

| Provider | `storage_key` holds | Stable under |
|---|---|---|
| **S3 / MinIO** | the object key — `{company_id}/{attachment_id}/v{version}/{filename}` | everything; we own the keyspace |
| **OneDrive** | the Graph **`itemId`** | rename, move, folder reorganisation |

**Do not address OneDrive by path.** A document library is a real folder tree that
real people reorganise, and a path-addressed attachment breaks the first time
someone drags a folder in the browser. `itemId` survives that; the path does not.

Folder layout on OneDrive is therefore **cosmetic** — chosen so a human browsing
the library can find things, never depended on by the application.

### Access control is ours

Downloads stream **through an authorising endpoint** in both cases
([architecture](architecture.md#security)). No pre-signed S3 URLs, no Graph
`@microsoft.graph.downloadUrl` handed to the browser.

Both providers can mint a short-lived unauthenticated URL, and both are the wrong
answer here: the endpoint re-checks **scope + capability** against the record the
attachment hangs off, and a URL that has escaped that check is a URL that leaks a
competitor analysis to another Area. The cost is that the app sits in the data
path — irrelevant at a 25 MB cap and dozens of concurrent users.

That endpoint is an ASP.NET Core controller action, not a Razor component —
streaming a file needs direct control over the response headers, which an
interactive component's own handlers don't reliably have
([web-conventions](web-conventions.md#where-this-applies-next)).

---

## 3. Where the providers genuinely differ

These are the implementation details that do *not* abstract away, listed so the
second implementation is not a surprise.

| Concern | S3 / MinIO | OneDrive (Graph) |
|---|---|---|
| **Auth** | Static access key + secret, no expiry | OAuth2 **client credentials**; token cached and refreshed |
| **Upload ≤ 4 MB** | `PutObject` | `PUT /content` |
| **Upload > 4 MB** | Same `PutObject` (multipart only above 5 GB) | **Upload session required** — chunked `PUT` with `Content-Range` |
| **Throttling** | Effectively none at our volume | **429 with `Retry-After`**, routinely |
| **Consistency** | Read-after-write | Read-after-write, but item metadata can lag |
| **Duplicate name** | Overwrites the key | Renames to `file 1.pdf` unless told otherwise |

Two of these have teeth:

**The 4 MB boundary is inside our size limit.** `Upload:MaxSizeMb` defaults to 25
([domain/master-data §11](../domain/master-data.md#11-business-constants-and-deployment-configuration)),
so the OneDrive implementation **must** implement upload sessions from day one —
this is not a "later, for big files" path. Scanned KK0s routinely exceed 4 MB.

**Graph throttles and S3 does not.** Wrap only the Graph client in a retry policy
that honours `Retry-After` (Polly). Do not apply a blanket retry to both — retrying
an S3 write that failed for a real reason just delays the error.

**Name collisions must be suppressed.** Set the conflict behaviour to `replace` on
the upload session, or Graph invents `KK0-0000042-ttd 1.pdf` and the `itemId` we
store belongs to a file whose name no longer matches `attachment.filename`.

---

## 4. Configuration

Selected by discriminator; only the selected block is required.

```jsonc
// appsettings.json — committed defaults, no secrets
{
  "Storage": {
    "Type": "S3",                       // "S3" | "OneDrive"

    "S3": {
      "ServiceUrl":     "http://minio:9000",
      "Bucket":         "simando-attachments",
      "Region":         "us-east-1",     // MinIO ignores it; the SDK requires one
      "ForcePathStyle":  true            // required for MinIO
      // AccessKey / SecretKey — injected, not committed
    },

    "OneDrive": {
      "TenantId":  "",
      "ClientId":  "",
      "DriveId":   "",                   // the target document library
      "RootFolder": "Simando/Attachments"
      // ClientSecret — injected, not committed
    }
  }
}
```

Tenant id, client id and drive id are not secret and can sit in the committed
file. **The access key, secret key and client secret cannot** — not because
`appsettings.json` is the wrong *shape* for them, but because that file is
committed and baked into the image
([domain/master-data §11](../domain/master-data.md#secrets--out-of-source-control-out-of-the-image)).
Inject them at deployment, as environment variables or as mounted secret files:

```bash
Storage__Type=OneDrive
Storage__OneDrive__TenantId=…
Storage__OneDrive__ClientId=…
Storage__OneDrive__DriveId=…
Storage__OneDrive__ClientSecret=…        # ← the only secret here
```

```yaml
# docker-compose.yml — preferred: a mounted file, not an env var
secrets:
  onedrive_client_secret:
    file: ./secrets/onedrive_client_secret
services:
  app:
    secrets: [onedrive_client_secret]
```

```csharp
builder.Configuration.AddKeyPerFile("/run/secrets", optional: true);
```

`AddKeyPerFile` maps `/run/secrets/Storage__OneDrive__ClientSecret` onto the same
key the env var would set, so nothing downstream changes.

On a developer machine, put the same keys in **`appsettings.Local.json`** — not
committed, not copied into the image
([layering](architecture.md#the-layering-and-where-a-developer-puts-their-own-values)).
Flipping `Storage:Type` there is also how you test the other provider without
touching the committed defaults.

> ⚠️ **Guard the OneDrive client secret harder than the MinIO keys.** A leaked
> MinIO key exposes this system's attachments. A leaked client secret with
> `Files.ReadWrite.All` — an **application**, tenant-wide permission — exposes
> **every file in PGN's OneDrive and SharePoint**, most of which has nothing to do
> with this system. Ask PGN's IT for a **certificate** instead of a shared secret
> if their process allows it; Graph supports both and a certificate cannot be
> copy-pasted out of a terminal.

### Registration

```csharp
services.AddOptions<StorageOptions>()
        .Bind(config.GetSection("Storage"))
        .ValidateOnStart();                       // see below

services.AddSingleton<IValidateOptions<StorageOptions>, StorageOptionsValidator>();

services.AddSingleton<IAttachmentStore>(sp =>
    sp.GetRequiredService<IOptions<StorageOptions>>().Value.Type switch
    {
        StorageProvider.S3       => ActivatorUtilities.CreateInstance<S3AttachmentStore>(sp),
        StorageProvider.OneDrive => ActivatorUtilities.CreateInstance<OneDriveAttachmentStore>(sp),
        var t => throw new InvalidOperationException($"Unknown Storage:Type '{t}'")
    });
```

### Validation must fail at startup

`StorageOptionsValidator` checks **only the selected type's block** and fails the
host if it is incomplete — missing `DriveId`, empty `ClientSecret`, an S3 bucket
that does not exist.

The alternative is discovering the misconfiguration when a surveyor presses *Unggah*
at the end of a two-hour KK0. A container that refuses to start is a deployment
problem; a storage layer that fails on first write is a data-loss problem.

Include a **startup probe** — write and delete a tiny object under
`__healthcheck/` — so the failure surfaces as "cannot write to storage" rather than
a 500 six hours later.

---

## 5. Running both at once

`attachment` carries the provider **per row**:

| Column | Meaning |
|---|---|
| `storage_provider` | `s3` \| `onedrive` — which store wrote this blob |
| `storage_key` | S3 object key, or Graph `itemId` |

Reads resolve through the row, not through configuration:

```csharp
public interface IAttachmentStoreResolver
{
    IAttachmentStore For(StorageProvider provider);   // reads
    IAttachmentStore Current { get; }                 // writes — the configured default
}
```

**This is what makes the OneDrive switch a config change rather than a migration.**
When PGN grants tenant access, flip `Storage:Type` to `OneDrive`: new uploads land
in the document library, existing MinIO blobs keep serving from MinIO, and nothing
needs to move on the day of the change.

Both stores must be registered whenever any row references them, so the switch-over
deployment configures **both** blocks even though only one is `Type`. Retiring MinIO
later is then a separate, unhurried backfill: copy blobs, rewrite
`storage_provider` and `storage_key` per row, verify checksums, and only then
decommission.

> ⚠️ **Do not "clean up" by deleting the S3 block once OneDrive is live.** Every
> attachment uploaded before the switch becomes unreadable, and the failure is
> silent until someone opens an old NOL.

---

## 6. Write ordering and orphans

Same in both providers, so it is handled once, above the interface:

```
1. generate attachment_id
2. write blob at the key derived from it     ← may leave an orphan
3. insert the attachment row                  ← commits the reference
```

Never the reverse. An orphaned blob is invisible and cheap; a database row pointing
at a blob that was never written is a broken download on a signed document.

A daily Hangfire sweep lists blobs with no matching row, older than 24 hours, and
calls `DeleteOrphanAsync`. The age threshold matters — without it the sweep races
uploads in flight.

`checksum` is computed while streaming the upload and stored on the row, so a
later integrity check does not require trusting either provider's ETag (S3's is
not an MD5 for multipart uploads, and Graph's is not an MD5 at all).

---

## 7. Testing

| Provider | Approach |
|---|---|
| **S3 / MinIO** | Real MinIO in **Testcontainers**. The full contract suite runs against it |
| **OneDrive** | No emulator exists. Contract suite runs against a **recorded HTTP fixture**, plus a manual smoke checklist against a real tenant before go-live |

Write the contract suite against `IAttachmentStore`, not against either
implementation, and run it twice:

| # | Case | Expected |
|---|---|---|
| ST1 | Put then read | Byte-identical stream returned |
| ST2 | Put 5 MB | Succeeds — **crosses Graph's 4 MB simple-upload boundary** |
| ST3 | Put twice at different versions | Two distinct blobs, both readable |
| ST4 | Read a key that does not exist | Typed `BlobNotFound`, not a provider exception |
| ST5 | Read a blob written by the *other* provider | Resolver picks the right store |
| ST6 | Filename containing spaces, `#`, and non-ASCII | Round-trips; no rename |
| ST7 | Startup with the selected type's config incomplete | **Host fails to start** |
| ST8 | Startup with the *unselected* type's config incomplete | Starts normally |
| ST9 | Provider returns 429 with `Retry-After` | Retried, honoured (OneDrive only) |
| ST10 | Checksum after round-trip | Matches the value stored at upload |

**ST5 and ST8 are the ones that protect the migration.** ST5 proves mixed-provider
reads work; ST8 proves a deployment that has not yet been given OneDrive credentials
still boots.

> The OneDrive implementation cannot be honestly called *tested* until it has run
> against PGN's real tenant. Recorded fixtures catch regressions in our code; they
> cannot catch a permission that was never granted. Treat the smoke checklist as a
> go-live gate, not a formality.

---

## 8. What we need from PGN

Blocking the OneDrive implementation, and worth asking early because the consent
step involves their IT:

1. **Which OneDrive.** *"OneDrive"* usually means a personal drive tied to one
   person's account — **if that person leaves PGN, the archive leaves with them.**
   What this needs is a **SharePoint document library** (OneDrive for Business),
   owned by the organisation. Confirm which, explicitly.
2. **App registration** in their Entra ID tenant: tenant id, client id, client
   secret or certificate.
3. **Admin consent** for the `Files.ReadWrite.All` **application** permission —
   app-only, not delegated. A background job and a document generator have no
   signed-in user to act as.
4. **The target drive/library id**, and confirmation we may create a folder tree
   under it.
5. **Retention and legal hold** on that library. If PGN's tenant policy deletes or
   archives items after N years, it will do so to signed NOLs — which contradicts
   the indefinite-retention rule this system assumes.

Item 5 is the one that gets missed. The others fail loudly at startup; a retention
policy fails silently, years later.
