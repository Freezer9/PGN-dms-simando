using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Simando.Api.Security;
using Simando.Application.Attachments;
using Simando.Application.Security;
using Simando.Domain.Attachments;
using Simando.Domain.Registration;
using Simando.Domain.Security;

namespace Simando.Api.Controllers;

// Authorised attachment download & management — docs/build/storage.md §2, docs/build/web-conventions.md.
// Every download streams through this action; no pre-signed URLs, no Graph downloadUrl.
[ApiController]
[Route("api/attachments")]
[Route("attachments")]
[Authorize]
public sealed class AttachmentsController(
    IAttachmentService attachmentService,
    IBreakGlassService breakGlassService,
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
        var hasViewCapability = currentUser.HasCapability(Capability.ViewCompanyRecords);
        var hasActiveBreakGlass = !hasViewCapability && await breakGlassService.HasActiveAccessAsync(currentUser.UserId, companyId, ct);

        if (!hasViewCapability && !hasActiveBreakGlass) return Forbid();

        var result = await attachmentService.GetCompanyAttachmentsAsync(companyId, currentUser.Permissions, hasActiveBreakGlass, ct);
        if (result.NotFound) return NotFound();
        if (result.Forbidden) return Forbid();

        return Ok(result.Attachments);
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

        await using var stream = file.OpenReadStream();
        var request = new UploadAttachmentRequest(
            kind,
            file.FileName,
            file.ContentType,
            file.Length,
            signatureMethod,
            stream);

        var result = await attachmentService.UploadAttachmentAsync(companyId, request, currentUser.UserId, currentUser.Permissions, ct);
        if (result.NotFound) return NotFound();
        if (result.Forbidden) return Forbid();
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
        var result = await attachmentService.DownloadAttachmentAsync(id, currentUser.Permissions, ct);
        if (result.NotFound) return NotFound();
        if (result.Forbidden) return Forbid();

        return File(result.ContentStream, result.MimeType, result.Filename);
    }

    [HttpDelete("{id:guid}")]
    [RequireCapability(Capability.UploadAttachments)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await attachmentService.DeleteAttachmentAsync(id, currentUser.Permissions, ct);
        if (result.NotFound) return NotFound();
        if (result.Forbidden) return Forbid();
        if (!result.Succeeded) return BadRequest(new ProblemDetails { Detail = result.Error });

        return NoContent();
    }
}
