namespace Simando.Domain.MasterData;

// Dokumen Acuan Kerja — versioned policy documents a NOL request cites
// (Ketentuan Harga Gas etc.), not a document template. Editing creates a
// new row; it never overwrites — EffectiveTo null means currently in
// force, so there's no separate Active flag. docs/domain/master-data.md §9.
//
// BlobKey is a plain string for now, not an Attachment FK — attachment
// storage (IAttachmentStore) is phase-3 scope and doesn't exist yet.
public sealed class ReferenceDocument
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required int Version { get; init; }
    public required DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; set; }
    public string? BlobKey { get; init; }
}
