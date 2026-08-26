---
id: "storage-iattachmentstore-s3attachmentstore-minio-2026-08-07"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-09T00:00:00.000Z"
completedAt: "2026-08-09T00:00:00.000Z"
labels: ["storage", "phase-3"]
order: "a00v"
---

# Storage: IAttachmentStore + S3AttachmentStore (MinIO)

Four operations only — put, open-read, exists, delete-orphan (never a business-operation delete). `storage_provider`/`storage_key` per attachment row, not per deployment. See `docs/build/storage.md#1`.

`Simando.Application/Storage/IAttachmentStore.cs` (interface + `StoredBlob`/`StoredBlobRef`/`BlobWriteRequest`/`StorageProvider`) + `BlobNotFoundException`; `Simando.Infrastructure/Storage/{StorageOptions,StorageOptionsValidator,S3AttachmentStore,StorageStartupProbe}.cs`. DI: `AddOptions<StorageOptions>().Bind(...).ValidateOnStart()`, `IAttachmentStore` registered by `Storage:Type` (only `S3` implemented — `OneDrive` throws at registration time), `StorageStartupProbe` as an `IHostedService` that writes+deletes a tiny object under `__healthcheck/` on boot so a misconfigured deployment fails to start rather than 500ing on a surveyor's first upload.

**Deviations / deliberately excluded** (each owned by its own separate backlog card):
- No `Attachment` domain entity/migration — the interface is blob-only; inserting the row is the caller's job (§6), first candidate `web-signed-kk0-upload-flow` (a00u, still backlog).
- No `IAttachmentStoreResolver`/`OneDriveAttachmentStore` — `storage-onedriveattachmentstore-dual-provider-reso` (a01Y), blocked on PGN granting tenant access.
- No authorising download controller — `storage-authorising-attachment-download-controller` (a00w) needs a real `Attachment` row to authorise against, which doesn't exist until an upload flow writes one.

**Also fixed here**: AWSSDK.S3 v4's default request-checksum behaviour sends a chunked streaming payload with a trailing checksum that MinIO/RustFS don't reliably validate (`x-amz-content-sha256 header does not match what was computed`) — `RequestChecksumCalculation.WHEN_REQUIRED` on `AmazonS3Config` avoids it. Also added `.ConfigureServices(services => services.RemoveAll<IHostedService>())` to `SignInFlowTests`/`ShellNavigationTests`' `WebApplicationFactory` setup — those boot the full app host and would otherwise run `StorageStartupProbe` against whatever `Storage:S3` config happens to be on the machine running the suite, unrelated to what either test actually exercises.

New tests: `tests/Simando.Integration.Tests/Storage/AttachmentStoreContractTests.cs` (7 tests, real MinIO via Testcontainers) — the docs/build/storage.md §7 contract suite minus ST5/ST8/ST9 (resolver and OneDrive-only cases, out of scope here).
