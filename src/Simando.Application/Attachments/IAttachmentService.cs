using Simando.Domain.Attachments;
using Simando.Domain.Registration;

namespace Simando.Application.Attachments;

public sealed record AttachmentDetail(
    Guid Id,
    Guid? CompanyId,
    AttachmentKind Kind,
    string Filename,
    string MimeType,
    long SizeBytes,
    SignatureMethod? SignatureMethod,
    int Version,
    DateTimeOffset UploadedAt,
    string? UploadedByName
);

public sealed record UploadAttachmentRequest(
    AttachmentKind Kind,
    string Filename,
    string MimeType,
    long SizeBytes,
    SignatureMethod? SignatureMethod,
    Stream Content
);

public sealed record UploadAttachmentResult(bool Succeeded, AttachmentDetail? Attachment, string? Error)
{
    public static UploadAttachmentResult Success(AttachmentDetail attachment) => new(true, attachment, null);
    public static UploadAttachmentResult Failed(string error) => new(false, null, error);
}

public interface IAttachmentService
{
    Task<IReadOnlyList<AttachmentDetail>> GetCompanyAttachmentsAsync(Guid companyId, CancellationToken ct = default);

    Task<UploadAttachmentResult> UploadAttachmentAsync(
        Guid companyId, UploadAttachmentRequest request, Guid actorUserId, CancellationToken ct = default);
}
