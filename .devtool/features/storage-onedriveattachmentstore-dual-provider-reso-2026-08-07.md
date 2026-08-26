---
id: "storage-onedriveattachmentstore-dual-provider-reso-2026-08-07"
status: "backlog"
priority: "low"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T09:00:00.000Z"
completedAt: null
labels: ["storage", "future", "pgn-blocked"]
order: "a01Y"
---

# Storage: OneDriveAttachmentStore + dual-provider resolver

🚧 Blocked on PGN granting tenant access (SharePoint document library, app registration, admin consent for `Files.ReadWrite.All`, target drive id, retention-policy confirmation). `IAttachmentStoreResolver` lets both providers serve side by side during migration — flipping `Storage:Type` is a config change, never a data migration. See `docs/build/storage.md#8-what-we-need-from-pgn`.
