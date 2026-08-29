using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simando.Api.Security;
using Simando.Application.Documents;
using Simando.Domain.Security;

namespace Simando.Api.Controllers;

[ApiController]
[Route("api/documents")]
[Route("documents")]
[Authorize]
[RequireCapability(Capability.GenerateDocuments)]
public sealed class DocumentDownloadController(
    IDocumentGenerator documentGenerator,
    ICurrentUser currentUser) : ControllerBase
{
    private const string DocxMimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    [HttpGet("company/{companyId:guid}/kk0")]
    public async Task<IActionResult> DownloadKk0(Guid companyId, CancellationToken ct)
    {
        var result = await documentGenerator.GenerateKk0DocxAsync(companyId, currentUser.Permissions, ct);
        if (!result.CompanyFound) return NotFound();
        if (!result.Authorized) return Forbid();

        return File(result.Content, DocxMimeType, result.Filename);
    }

    [HttpGet("company/{companyId:guid}/a1")]
    public async Task<IActionResult> DownloadA1(Guid companyId, CancellationToken ct)
    {
        var result = await documentGenerator.GenerateA1DocxAsync(companyId, currentUser.Permissions, ct);
        if (!result.CompanyFound) return NotFound();
        if (!result.Authorized) return Forbid();

        return File(result.Content, DocxMimeType, result.Filename);
    }

    [HttpGet("company/{companyId:guid}/nol-request")]
    public async Task<IActionResult> DownloadNolRequest(Guid companyId, CancellationToken ct)
    {
        var result = await documentGenerator.GenerateNolRequestDocxAsync(companyId, currentUser.Permissions, ct);
        if (!result.CompanyFound) return NotFound();
        if (!result.Authorized) return Forbid();

        return File(result.Content, DocxMimeType, result.Filename);
    }

    [HttpGet("company/{companyId:guid}/evaluation")]
    public async Task<IActionResult> DownloadEvaluationResume(Guid companyId, CancellationToken ct)
    {
        var result = await documentGenerator.GenerateEvaluationResumeDocxAsync(companyId, currentUser.Permissions, ct);
        if (!result.CompanyFound) return NotFound();
        if (!result.Authorized) return Forbid();

        return File(result.Content, DocxMimeType, result.Filename);
    }

    [HttpGet("company/{companyId:guid}/nol-issuance")]
    public async Task<IActionResult> DownloadNolIssuance(Guid companyId, CancellationToken ct)
    {
        var result = await documentGenerator.GenerateNolIssuanceDocxAsync(companyId, currentUser.Permissions, ct);
        if (!result.CompanyFound) return NotFound();
        if (!result.Authorized) return Forbid();

        return File(result.Content, DocxMimeType, result.Filename);
    }
}
