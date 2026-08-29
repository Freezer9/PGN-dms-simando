using Microsoft.EntityFrameworkCore;
using Simando.Application.Attachments;
using Simando.Application.Storage;
using Simando.Domain.Attachments;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;

namespace Simando.Infrastructure.Attachments;

internal sealed class AttachmentService(
    IDbContextFactory<SimandoDbContext> dbContextFactory,
    IAttachmentStore store) : IAttachmentService
{
    public async Task<GetCompanyAttachmentsResult> GetCompanyAttachmentsAsync(
        Guid companyId, EffectivePermissions permissions, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies
            .IgnoreQueryFilters()
            .Where(c => c.Id == companyId)
            .Join(db.Areas, c => c.AreaId, a => a.Id,
                (c, a) => new { c.AreaId, a.RegionId })
            .FirstOrDefaultAsync(ct);

        if (company is null)
            return GetCompanyAttachmentsResult.NotFoundResult();

        if (!PermissionEvaluator.CanViewRecord(permissions, company.AreaId, company.RegionId))
            return GetCompanyAttachmentsResult.ForbiddenResult();

        var attachments = await db.Attachments.AsNoTracking()
            .Where(a => a.CompanyId == companyId)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync(ct);

        var userIds = attachments.Select(a => a.UploadedBy).ToHashSet();
        var userNames = await db.Users.AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, ct);

        var items = attachments.Select(a => new AttachmentDetail(
            a.Id,
            a.CompanyId,
            a.Kind,
            a.Filename,
            a.MimeType,
            a.SizeBytes,
            null,
            a.Version,
            a.UploadedAt,
            userNames.GetValueOrDefault(a.UploadedBy, "Unknown")
        )).ToList();

        return GetCompanyAttachmentsResult.Success(items);
    }

    public async Task<UploadAttachmentResult> UploadAttachmentAsync(
        Guid companyId, UploadAttachmentRequest request, Guid actorUserId, EffectivePermissions permissions, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies
            .IgnoreQueryFilters()
            .Where(c => c.Id == companyId)
            .Join(db.Areas, c => c.AreaId, a => a.Id,
                (c, a) => new { c.AreaId, a.RegionId })
            .FirstOrDefaultAsync(ct);

        if (company is null)
            return UploadAttachmentResult.NotFoundResult();

        if (!PermissionEvaluator.CanViewRecord(permissions, company.AreaId, company.RegionId))
            return UploadAttachmentResult.ForbiddenResult();

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

    public async Task<AttachmentDownloadResult> DownloadAttachmentAsync(
        Guid id, EffectivePermissions permissions, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var attachment = await db.Attachments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (attachment is null)
            return AttachmentDownloadResult.NotFoundResult();

        if (attachment.CompanyId is Guid companyId)
        {
            var company = await db.Companies
                .IgnoreQueryFilters()
                .Where(c => c.Id == companyId)
                .Join(db.Areas, c => c.AreaId, a => a.Id,
                    (c, a) => new { c.AreaId, a.RegionId })
                .FirstOrDefaultAsync(ct);

            if (company is null)
                return AttachmentDownloadResult.NotFoundResult();

            if (!PermissionEvaluator.CanViewRecord(permissions, company.AreaId, company.RegionId))
                return AttachmentDownloadResult.ForbiddenResult();
        }

        Stream stream;
        try
        {
            stream = await store.OpenReadAsync(
                new StoredBlobRef(attachment.StorageProvider, attachment.StorageKey), ct);
        }
        catch (BlobNotFoundException)
        {
            return AttachmentDownloadResult.NotFoundResult();
        }

        return AttachmentDownloadResult.Success(stream, attachment.MimeType, attachment.Filename);
    }

    public async Task<AttachmentDeleteResult> DeleteAttachmentAsync(
        Guid id, EffectivePermissions permissions, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var attachment = await db.Attachments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (attachment is null)
            return AttachmentDeleteResult.NotFoundResult();

        if (attachment.CompanyId is Guid companyId)
        {
            var company = await db.Companies
                .IgnoreQueryFilters()
                .Where(c => c.Id == companyId)
                .Join(db.Areas, c => c.AreaId, a => a.Id,
                    (c, a) => new { c.AreaId, a.RegionId })
                .FirstOrDefaultAsync(ct);

            if (company is null)
                return AttachmentDeleteResult.NotFoundResult();

            if (!PermissionEvaluator.CanViewRecord(permissions, company.AreaId, company.RegionId))
                return AttachmentDeleteResult.ForbiddenResult();
        }

        var a1 = await db.A1Registrations.FirstOrDefaultAsync(a => a.SignedDocumentId == id, ct);
        if (a1 is not null)
        {
            a1.SignedDocumentId = null;
        }

        var resumes = await db.EvaluationResumes.Where(r => r.AttachmentId == id).ToListAsync(ct);
        if (resumes.Count > 0)
        {
            db.EvaluationResumes.RemoveRange(resumes);
        }

        var issuance = await db.NolIssuances.FirstOrDefaultAsync(i => i.DocumentId == id, ct);
        if (issuance is not null)
        {
            issuance.DocumentId = null;
        }

        db.Attachments.Remove(attachment);
        await db.SaveChangesAsync(ct);

        return AttachmentDeleteResult.Success();
    }
}
