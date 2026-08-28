using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Simando.Application.Documents;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;

namespace Simando.Api.Controllers;

[ApiController]
[Route("documents")]
[Authorize]
public sealed class DocumentDownloadController(
    IDbContextFactory<SimandoDbContext> dbContextFactory,
    IDocumentGenerator documentGenerator) : ControllerBase
{
    private const string DocxMimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    [HttpGet("company/{companyId:guid}/kk0")]
    public async Task<IActionResult> DownloadKk0(Guid companyId, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies
            .IgnoreQueryFilters()
            .Where(c => c.Id == companyId)
            .Join(db.Areas, c => c.AreaId, a => a.Id, (c, a) => new { Company = c, a.RegionId })
            .FirstOrDefaultAsync(ct);

        if (company is null)
            return NotFound();

        var permissions = await ResolvePermissionsAsync(db, ct);
        if (permissions is null)
            return Unauthorized();

        if (!PermissionEvaluator.CanViewRecord(permissions, company.Company.AreaId, company.RegionId))
            return Forbid();

        var bytes = await documentGenerator.GenerateKk0DocxAsync(companyId, ct);
        var filename = $"KK0_{company.Company.NomorSeq:D4}.docx";

        return File(bytes, DocxMimeType, filename);
    }

    [HttpGet("company/{companyId:guid}/a1")]
    public async Task<IActionResult> DownloadA1(Guid companyId, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies
            .IgnoreQueryFilters()
            .Where(c => c.Id == companyId)
            .Join(db.Areas, c => c.AreaId, a => a.Id, (c, a) => new { Company = c, a.RegionId })
            .FirstOrDefaultAsync(ct);

        if (company is null)
            return NotFound();

        var permissions = await ResolvePermissionsAsync(db, ct);
        if (permissions is null)
            return Unauthorized();

        if (!PermissionEvaluator.CanViewRecord(permissions, company.Company.AreaId, company.RegionId))
            return Forbid();

        var bytes = await documentGenerator.GenerateA1DocxAsync(companyId, ct);
        var filename = $"Formulir_A1_{company.Company.NomorSeq:D4}.docx";

        return File(bytes, DocxMimeType, filename);
    }

    [HttpGet("company/{companyId:guid}/nol-request")]
    public async Task<IActionResult> DownloadNolRequest(Guid companyId, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies
            .IgnoreQueryFilters()
            .Where(c => c.Id == companyId)
            .Join(db.Areas, c => c.AreaId, a => a.Id, (c, a) => new { Company = c, a.RegionId })
            .FirstOrDefaultAsync(ct);

        if (company is null)
            return NotFound();

        var permissions = await ResolvePermissionsAsync(db, ct);
        if (permissions is null)
            return Unauthorized();

        if (!PermissionEvaluator.CanViewRecord(permissions, company.Company.AreaId, company.RegionId))
            return Forbid();

        var bytes = await documentGenerator.GenerateNolRequestDocxAsync(companyId, ct);
        var filename = $"Permohonan_NOL_{company.Company.NomorSeq:D4}.docx";

        return File(bytes, DocxMimeType, filename);
    }

    [HttpGet("company/{companyId:guid}/evaluation")]
    public async Task<IActionResult> DownloadEvaluationResume(Guid companyId, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies
            .IgnoreQueryFilters()
            .Where(c => c.Id == companyId)
            .Join(db.Areas, c => c.AreaId, a => a.Id, (c, a) => new { Company = c, a.RegionId })
            .FirstOrDefaultAsync(ct);

        if (company is null)
            return NotFound();

        var permissions = await ResolvePermissionsAsync(db, ct);
        if (permissions is null)
            return Unauthorized();

        if (!PermissionEvaluator.CanViewRecord(permissions, company.Company.AreaId, company.RegionId))
            return Forbid();

        var bytes = await documentGenerator.GenerateEvaluationResumeDocxAsync(companyId, ct);
        var filename = $"Resume_Evaluasi_{company.Company.NomorSeq:D4}.docx";

        return File(bytes, DocxMimeType, filename);
    }

    [HttpGet("company/{companyId:guid}/nol-issuance")]
    public async Task<IActionResult> DownloadNolIssuance(Guid companyId, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies
            .IgnoreQueryFilters()
            .Where(c => c.Id == companyId)
            .Join(db.Areas, c => c.AreaId, a => a.Id, (c, a) => new { Company = c, a.RegionId })
            .FirstOrDefaultAsync(ct);

        if (company is null)
            return NotFound();

        var permissions = await ResolvePermissionsAsync(db, ct);
        if (permissions is null)
            return Unauthorized();

        if (!PermissionEvaluator.CanViewRecord(permissions, company.Company.AreaId, company.RegionId))
            return Forbid();

        var bytes = await documentGenerator.GenerateNolIssuanceDocxAsync(companyId, ct);
        var filename = $"Surat_Penerbitan_NOL_{company.Company.NomorSeq:D4}.docx";

        return File(bytes, DocxMimeType, filename);
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
