namespace Simando.Domain.Attachments;

// Polymorphic — one table for all upload points.
// docs/design/data-model.md#attachment.
//
// CompanyId is a denormalised FK that lets the download controller and the
// RLS query filter resolve scope in a single join, without chasing
// attachable_type → attachable_id → … → company. Null for admin attachments
// (e.g. reference_document files) that have no company scope.
public sealed class Attachment
{
    public required Guid Id { get; init; }
    public Guid? CompanyId { get; init; }
    public required string AttachableType { get; init; }
    public required Guid AttachableId { get; init; }
    public required AttachmentKind Kind { get; init; }
    public required string Filename { get; init; }
    public required string MimeType { get; init; }
    public required long SizeBytes { get; init; }
    public required string Checksum { get; init; }
    public required StorageProvider StorageProvider { get; init; }
    public required string StorageKey { get; init; }
    public required Guid UploadedBy { get; init; }
    public required DateTimeOffset UploadedAt { get; init; }
    public required int Version { get; init; }
}
