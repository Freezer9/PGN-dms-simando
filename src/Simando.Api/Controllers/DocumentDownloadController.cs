using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Simando.Api.Security;
using Simando.Application.Documents;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;

namespace Simando.Api.Controllers;

[ApiController]
[Route("api/documents")]
[Route("documents")]
[Authorize]
[RequireCapability(Capability.GenerateDocuments)]
public sealed class DocumentDownloadController(
    IDbContextFactory<SimandoDbContext> dbContextFactory,
    IDocumentGenerator documentGenerator,
    ICurrentUser currentUser) : ControllerBase
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

        if (!PermissionEvaluator.CanViewRecord(currentUser.Permissions, company.Company.AreaId, company.RegionId))
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

        if (!PermissionEvaluator.CanViewRecord(currentUser.Permissions, company.Company.AreaId, company.RegionId))
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

        if (!PermissionEvaluator.CanViewRecord(currentUser.Permissions, company.Company.AreaId, company.RegionId))
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

        if (!PermissionEvaluator.CanViewRecord(currentUser.Permissions, company.Company.AreaId, company.RegionId))
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

        if (!PermissionEvaluator.CanViewRecord(currentUser.Permissions, company.Company.AreaId, company.RegionId))
            return Forbid();

        var bytes = await documentGenerator.GenerateNolIssuanceDocxAsync(companyId, ct);
        var filename = $"Surat_Penerbitan_NOL_{company.Company.NomorSeq:D4}.docx";

        return File(bytes, DocxMimeType, filename);
    }
}
