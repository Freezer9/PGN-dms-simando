using Simando.Domain.Attachments;
using Simando.Domain.Registration;
using Simando.Domain.Security;

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

public sealed record UploadAttachmentResult(
    bool Succeeded,
    bool NotFound,
    bool Forbidden,
    AttachmentDetail? Attachment,
    string? Error)
{
    public static UploadAttachmentResult Success(AttachmentDetail attachment) => new(true, false, false, attachment, null);
    public static UploadAttachmentResult NotFoundResult() => new(false, true, false, null, "Perusahaan tidak ditemukan.");
    public static UploadAttachmentResult ForbiddenResult() => new(false, false, true, null, "Tidak memiliki hak akses.");
    public static UploadAttachmentResult Failed(string error) => new(false, false, false, null, error);
}

public sealed record AttachmentDownloadResult(
    Stream ContentStream,
    string MimeType,
    string Filename,
    bool Succeeded,
    bool NotFound,
    bool Forbidden)
{
    public static AttachmentDownloadResult Success(Stream stream, string mimeType, string filename) =>
        new(stream, mimeType, filename, true, false, false);
    public static AttachmentDownloadResult NotFoundResult() =>
        new(Stream.Null, string.Empty, string.Empty, false, true, false);
    public static AttachmentDownloadResult ForbiddenResult() =>
        new(Stream.Null, string.Empty, string.Empty, false, false, true);
}

public sealed record AttachmentDeleteResult(
    bool Succeeded,
    bool NotFound,
    bool Forbidden,
    string? Error)
{
    public static AttachmentDeleteResult Success() => new(true, false, false, null);
    public static AttachmentDeleteResult NotFoundResult() => new(false, true, false, "Lampiran tidak ditemukan.");
    public static AttachmentDeleteResult ForbiddenResult() => new(false, false, true, "Tidak memiliki hak akses.");
    public static AttachmentDeleteResult Failed(string error) => new(false, false, false, error);
}

public sealed record GetCompanyAttachmentsResult(
    bool Succeeded,
    bool NotFound,
    bool Forbidden,
    IReadOnlyList<AttachmentDetail> Attachments)
{
    public static GetCompanyAttachmentsResult Success(IReadOnlyList<AttachmentDetail> attachments) =>
        new(true, false, false, attachments);
    public static GetCompanyAttachmentsResult NotFoundResult() =>
        new(false, true, false, []);
    public static GetCompanyAttachmentsResult ForbiddenResult() =>
        new(false, false, true, []);
}

public interface IAttachmentService
{
    Task<GetCompanyAttachmentsResult> GetCompanyAttachmentsAsync(
        Guid companyId, EffectivePermissions permissions, CancellationToken ct = default);

    Task<UploadAttachmentResult> UploadAttachmentAsync(
        Guid companyId, UploadAttachmentRequest request, Guid actorUserId, EffectivePermissions permissions, CancellationToken ct = default);

    Task<AttachmentDownloadResult> DownloadAttachmentAsync(
        Guid id, EffectivePermissions permissions, CancellationToken ct = default);

    Task<AttachmentDeleteResult> DeleteAttachmentAsync(
        Guid id, EffectivePermissions permissions, CancellationToken ct = default);
}
