using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Simando.Application.Storage;
using Simando.Domain.Attachments;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;

namespace Simando.Web.Controllers;

// Authorised attachment download — docs/build/storage.md §2, docs/build/web-conventions.md.
// Every download streams through this action; no pre-signed URLs, no Graph downloadUrl.
//
// Permissions are resolved inline from HttpContext.User + DB rather than through the
// circuit-scoped CurrentUser (which LoadAsync is never called on an HTTP request).
// IgnoreQueryFilters() is used so out-of-scope attachments return an explicit 403 rather
// than silently 404-ing through the RLS filter — matching the test cases in testing.md.
[ApiController]
[Route("attachments")]
[Authorize]
public sealed class AttachmentsController(
    IDbContextFactory<SimandoDbContext> dbContextFactory,
    IAttachmentStore attachmentStore) : ControllerBase
{
    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var attachment = await db.Attachments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (attachment is null)
            return NotFound();

        var permissions = await ResolvePermissionsAsync(db, ct);
        if (permissions is null)
            return Unauthorized();

        if (!permissions.HasCapability(Capability.DownloadAttachments))
            return Forbid();

        if (attachment.CompanyId is Guid companyId)
        {
            var company = await db.Companies
                .IgnoreQueryFilters()
                .Where(c => c.Id == companyId)
                .Join(db.Areas, c => c.AreaId, a => a.Id,
                    (c, a) => new { c.AreaId, a.RegionId })
                .FirstOrDefaultAsync(ct);

            if (company is null || !PermissionEvaluator.CanViewRecord(permissions, company.AreaId, company.RegionId))
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

    private async Task<EffectivePermissions?> ResolvePermissionsAsync(SimandoDbContext db, CancellationToken ct)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (idClaim is null || !Guid.TryParse(idClaim, out var userId))
            return null;

        var assignments = await db.RoleAssignments
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.Active)
            .ToListAsync(ct);

        return PermissionEvaluator.Resolve(assignments);
    }
}
