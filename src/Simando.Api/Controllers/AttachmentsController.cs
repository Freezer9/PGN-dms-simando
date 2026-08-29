using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Simando.Api.Security;
using Simando.Application.Attachments;
using Simando.Application.Storage;
using Simando.Domain.Attachments;
using Simando.Domain.Registration;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;

namespace Simando.Api.Controllers;

// Authorised attachment download & management — docs/build/storage.md §2, docs/build/web-conventions.md.
// Every download streams through this action; no pre-signed URLs, no Graph downloadUrl.
[ApiController]
[Route("api/attachments")]
[Route("attachments")]
[Authorize]
public sealed class AttachmentsController(
    IDbContextFactory<SimandoDbContext> dbContextFactory,
    IAttachmentStore attachmentStore,
    IAttachmentService attachmentService,
    ICurrentUser currentUser) : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".jpg", ".jpeg", ".png", ".zip"
    };

    [HttpGet("/api/companies/{companyId:guid}/attachments")]
    [HttpGet("/companies/{companyId:guid}/attachments")]
    [ProducesResponseType<IReadOnlyList<AttachmentDetail>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCompanyAttachments(Guid companyId, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies
            .IgnoreQueryFilters()
            .Where(c => c.Id == companyId)
            .Join(db.Areas, c => c.AreaId, a => a.Id,
                (c, a) => new { c.AreaId, a.RegionId })
            .FirstOrDefaultAsync(ct);

        if (company is null)
            return NotFound();

        if (!PermissionEvaluator.CanViewRecord(currentUser.Permissions, company.AreaId, company.RegionId))
            return Forbid();

        var attachments = await attachmentService.GetCompanyAttachmentsAsync(companyId, ct);
        return Ok(attachments);
    }

    [HttpPost("/api/companies/{companyId:guid}/attachments")]
    [HttpPost("/companies/{companyId:guid}/attachments")]
    [RequireCapability(Capability.UploadAttachments)]
    [ProducesResponseType<AttachmentDetail>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadCompanyAttachment(
        Guid companyId,
        IFormFile file,
        [FromForm] AttachmentKind kind,
        [FromForm] SignatureMethod? signatureMethod,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new ProblemDetails { Detail = "Berkas tidak boleh kosong." });
        }

        if (file.Length > 25 * 1024 * 1024)
        {
            return BadRequest(new ProblemDetails { Detail = "Ukuran berkas melebihi batas maksimum 25MB." });
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
        {
            return BadRequest(new ProblemDetails { Detail = $"Format berkas {ext} tidak diizinkan. Gunakan PDF, DOCX, XLSX, JPG, PNG, atau ZIP." });
        }

        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies
            .IgnoreQueryFilters()
            .Where(c => c.Id == companyId)
            .Join(db.Areas, c => c.AreaId, a => a.Id,
                (c, a) => new { c.AreaId, a.RegionId })
            .FirstOrDefaultAsync(ct);

        if (company is null)
            return NotFound();

        if (!PermissionEvaluator.CanViewRecord(currentUser.Permissions, company.AreaId, company.RegionId))
            return Forbid();

        await using var stream = file.OpenReadStream();
        var request = new UploadAttachmentRequest(
            kind,
            file.FileName,
            file.ContentType,
            file.Length,
            signatureMethod,
            stream);

        var result = await attachmentService.UploadAttachmentAsync(companyId, request, currentUser.UserId, ct);
        if (!result.Succeeded || result.Attachment is null)
        {
            return BadRequest(new ProblemDetails { Detail = result.Error ?? "Gagal mengunggah berkas." });
        }

        return CreatedAtAction(nameof(Download), new { id = result.Attachment.Id }, result.Attachment);
    }

    [HttpGet("{id:guid}/download")]
    [RequireCapability(Capability.DownloadAttachments)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var attachment = await db.Attachments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (attachment is null)
            return NotFound();

        if (attachment.CompanyId is Guid companyId)
        {
            var company = await db.Companies
                .IgnoreQueryFilters()
                .Where(c => c.Id == companyId)
                .Join(db.Areas, c => c.AreaId, a => a.Id,
                    (c, a) => new { c.AreaId, a.RegionId })
                .FirstOrDefaultAsync(ct);

            if (company is null || !PermissionEvaluator.CanViewRecord(currentUser.Permissions, company.AreaId, company.RegionId))
                return Forbid();
        }

        Stream stream;
        try
        {
            stream = await attachmentStore.OpenReadAsync(
                new StoredBlobRef(attachment.StorageProvider, attachment.StorageKey), ct);
        }
        catch (BlobNotFoundException)
        {
            return NotFound();
        }

        return File(stream, attachment.MimeType, attachment.Filename);
    }

    [HttpDelete("{id:guid}")]
    [RequireCapability(Capability.UploadAttachments)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var attachment = await db.Attachments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (attachment is null)
            return NotFound();

        if (attachment.CompanyId is Guid companyId)
        {
            var company = await db.Companies
                .IgnoreQueryFilters()
                .Where(c => c.Id == companyId)
                .Join(db.Areas, c => c.AreaId, a => a.Id,
                    (c, a) => new { c.AreaId, a.RegionId })
                .FirstOrDefaultAsync(ct);

            if (company is null || !PermissionEvaluator.CanViewRecord(currentUser.Permissions, company.AreaId, company.RegionId))
                return Forbid();
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

        return NoContent();
    }
}
