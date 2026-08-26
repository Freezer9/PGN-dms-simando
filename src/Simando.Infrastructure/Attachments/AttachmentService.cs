using Microsoft.EntityFrameworkCore;
using Simando.Application.Attachments;
using Simando.Application.Storage;
using Simando.Domain.Attachments;
using Simando.Infrastructure.Persistence;

namespace Simando.Infrastructure.Attachments;

internal sealed class AttachmentService(
    IDbContextFactory<SimandoDbContext> dbContextFactory,
    IAttachmentStore store) : IAttachmentService
{
    public async Task<IReadOnlyList<AttachmentDetail>> GetCompanyAttachmentsAsync(Guid companyId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var attachments = await db.Attachments.AsNoTracking()
            .Where(a => a.CompanyId == companyId)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync(ct);

        var userIds = attachments.Select(a => a.UploadedBy).ToHashSet();
        var userNames = await db.Users.AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, ct);

        return attachments.Select(a => new AttachmentDetail(
            a.Id,
            a.CompanyId,
            a.Kind,
            a.Filename,
            a.MimeType,
            a.SizeBytes,
            null, // SignatureMethod is stored on A1Registration or derived
            a.Version,
            a.UploadedAt,
            userNames.GetValueOrDefault(a.UploadedBy, "Unknown")
        )).ToList();
    }

    public async Task<UploadAttachmentResult> UploadAttachmentAsync(
        Guid companyId, UploadAttachmentRequest request, Guid actorUserId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies.FirstOrDefaultAsync(c => c.Id == companyId, ct);
        if (company is null)
        {
            return UploadAttachmentResult.Failed("Berkas perusahaan tidak ditemukan.");
        }

        var existingCount = await db.Attachments
            .CountAsync(a => a.CompanyId == companyId && a.Kind == request.Kind, ct);
        var version = existingCount + 1;

        var attachmentId = Guid.NewGuid();
        var key = $"{companyId}/{attachmentId}/v{version}/{request.Filename}";

        var blobWrite = new BlobWriteRequest(key, request.MimeType);
        var storedBlob = await store.PutAsync(blobWrite, request.Content, ct);

        var attachment = new Attachment
        {
            Id = attachmentId,
            CompanyId = companyId,
            AttachableType = "company",
            AttachableId = companyId,
            Kind = request.Kind,
            Filename = request.Filename,
            MimeType = request.MimeType,
            SizeBytes = request.SizeBytes > 0 ? request.SizeBytes : storedBlob.SizeBytes,
            Checksum = storedBlob.ETag ?? "",
            StorageProvider = storedBlob.Provider,
            StorageKey = storedBlob.Key,
            UploadedBy = actorUserId,
            UploadedAt = DateTimeOffset.UtcNow,
            Version = version,
        };

        db.Attachments.Add(attachment);

        // If uploading A1 registration signed file or KK0, link it to A1Registration
        if (request.Kind == AttachmentKind.A1 || request.Kind == AttachmentKind.Kk0)
        {
            var a1 = await db.A1Registrations.FirstOrDefaultAsync(a => a.CompanyId == companyId, ct);
            if (a1 is null)
            {
                a1 = new Domain.Registration.A1Registration { CompanyId = companyId };
                db.A1Registrations.Add(a1);
            }
            a1.SignedDocumentId = attachmentId;
            if (request.SignatureMethod is { } sigMethod)
            {
                a1.SignatureMethod = sigMethod;
            }
        }

        await db.SaveChangesAsync(ct);

        var uploaderName = await db.Users.AsNoTracking()
            .Where(u => u.Id == actorUserId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(ct) ?? "Unknown";

        var detail = new AttachmentDetail(
            attachment.Id,
            attachment.CompanyId,
            attachment.Kind,
            attachment.Filename,
            attachment.MimeType,
            attachment.SizeBytes,
            request.SignatureMethod,
            attachment.Version,
            attachment.UploadedAt,
            uploaderName);

        return UploadAttachmentResult.Success(detail);
    }
}
