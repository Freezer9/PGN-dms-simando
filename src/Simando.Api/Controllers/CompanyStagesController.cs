using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simando.Api.Security;
using Simando.Application.Directory;
using Simando.Application.Nol;
using Simando.Application.Registration;
using Simando.Application.Security;
using Simando.Domain.Security;

namespace Simando.Api.Controllers;

public sealed record SaveSurveyFullPayload(
    SaveSurveyRequest Request,
    IReadOnlyList<SaveSurveyProductRequest> Products,
    IReadOnlyList<SaveSurveyRawMaterialRequest> RawMaterials,
    IReadOnlyList<SaveSurveyMarketRequest> Markets,
    IReadOnlyList<SaveSurveyEquipmentRequest> Equipment);

[ApiController]
[Route("api/companies")]
[Authorize]
public sealed class CompanyStagesController(
    ICompanyService companyService,
    IBreakGlassService breakGlassService,
    ICurrentUser currentUser) : ControllerBase
{
    private async Task<(bool Allowed, bool IsBreakGlass)> CheckReadAccessAsync(Guid companyId, CancellationToken ct)
    {
        var hasViewCapability = currentUser.HasCapability(Capability.ViewCompanyRecords);
        if (hasViewCapability)
        {
            return (true, false);
        }

        var hasBreakGlass = await breakGlassService.HasActiveAccessAsync(currentUser.UserId, companyId, ct);
        return (hasBreakGlass, hasBreakGlass);
    }

    // ==========================================
    // Stage 1: Survey
    // ==========================================
    [HttpGet("{id:guid}/survey")]
    [ProducesResponseType<SurveyDetail>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSurvey(Guid id, CancellationToken ct)
    {
        var (allowed, isBreakGlass) = await CheckReadAccessAsync(id, ct);
        if (!allowed)
        {
            return Forbid();
        }

        var result = await companyService.GetSurveyAsync(id, isBreakGlass, ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}/survey")]
    [RequireCapability(Capability.EditSurvey)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SaveSurvey(Guid id, [FromBody] SaveSurveyFullPayload payload, CancellationToken ct)
    {
        var result = await companyService.SaveSurveyFullAsync(
            id,
            payload.Request,
            payload.Products,
            payload.RawMaterials,
            payload.Markets,
            payload.Equipment,
            currentUser.UserId,
            currentUser.Permissions,
            ct);

        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails { Detail = result.Error });
        }

        return Ok();
    }

    // ==========================================
    // Stage 2: Plotting & Prospek
    // ==========================================
    [HttpGet("{id:guid}/plotting")]
    [ProducesResponseType<PlottingDetail>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPlotting(Guid id, CancellationToken ct)
    {
        var (allowed, isBreakGlass) = await CheckReadAccessAsync(id, ct);
        if (!allowed)
        {
            return Forbid();
        }

        var result = await companyService.GetPlottingAsync(id, isBreakGlass, ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}/plotting")]
    [RequireCapability(Capability.EditStages1To3)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SavePlotting(Guid id, [FromBody] SavePlottingRequest request, CancellationToken ct)
    {
        var result = await companyService.SavePlottingAsync(
            id,
            request,
            currentUser.UserId,
            currentUser.Permissions,
            ct);

        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails { Detail = result.Error });
        }

        return Ok();
    }

    [HttpPost("{id:guid}/promote-to-prospek")]
    [RequireCapability(Capability.EditStages1To3)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PromoteToProspek(Guid id, CancellationToken ct)
    {
        var result = await companyService.PromoteToProspekAsync(
            id,
            currentUser.UserId,
            currentUser.Permissions,
            ct);

        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails { Detail = result.Error });
        }

        return Ok();
    }

    // ==========================================
    // Stage 3: Registration / A1
    // ==========================================
    [HttpGet("{id:guid}/registration")]
    [ProducesResponseType<A1RegistrationDetail>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRegistration(Guid id, CancellationToken ct)
    {
        var (allowed, isBreakGlass) = await CheckReadAccessAsync(id, ct);
        if (!allowed)
        {
            return Forbid();
        }

        var result = await companyService.GetA1RegistrationAsync(id, isBreakGlass, ct);
        if (result is null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpPut("{id:guid}/registration")]
    [RequireCapability(Capability.EditA1)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SaveRegistration(Guid id, [FromBody] SaveA1RegistrationRequest request, CancellationToken ct)
    {
        var result = await companyService.SaveA1RegistrationAsync(
            id,
            request,
            currentUser.UserId,
            currentUser.Permissions,
            ct);

        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails { Detail = result.Error });
        }

        return Ok();
    }

    // ==========================================
    // Stage 4: NOL (Request, Evaluation, Issuance)
    // ==========================================
    [HttpGet("{id:guid}/nol-request")]
    [ProducesResponseType<NolRequestDetail>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNolRequest(Guid id, CancellationToken ct)
    {
        var (allowed, isBreakGlass) = await CheckReadAccessAsync(id, ct);
        if (!allowed)
        {
            return Forbid();
        }

        var result = await companyService.GetNolRequestAsync(id, isBreakGlass, ct);
        if (result is null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpPut("{id:guid}/nol-request")]
    [RequireCapability(Capability.EditNolRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SaveNolRequest(Guid id, [FromBody] SaveNolRequestRequest request, CancellationToken ct)
    {
        var result = await companyService.SaveNolRequestAsync(
            id,
            request,
            currentUser.UserId,
            currentUser.Permissions,
            ct);

        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails { Detail = result.Error });
        }

        return Ok();
    }

    [HttpGet("{id:guid}/nol-evaluation")]
    [ProducesResponseType<NolEvaluationDetail>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNolEvaluation(Guid id, CancellationToken ct)
    {
        var (allowed, isBreakGlass) = await CheckReadAccessAsync(id, ct);
        if (!allowed)
        {
            return Forbid();
        }

        var result = await companyService.GetNolEvaluationAsync(id, isBreakGlass, ct);
        if (result is null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpPut("{id:guid}/nol-evaluation")]
    [RequireCapability(Capability.EditEvaluation)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SaveNolEvaluation(Guid id, [FromBody] SaveNolEvaluationRequest request, CancellationToken ct)
    {
        var result = await companyService.SaveNolEvaluationAsync(
            id,
            request,
            currentUser.UserId,
            currentUser.Permissions,
            ct);

        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails { Detail = result.Error });
        }

        return Ok();
    }

    [HttpGet("{id:guid}/nol-issuance")]
    [ProducesResponseType<NolIssuanceDetail>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNolIssuance(Guid id, CancellationToken ct)
    {
        var (allowed, isBreakGlass) = await CheckReadAccessAsync(id, ct);
        if (!allowed)
        {
            return Forbid();
        }

        var result = await companyService.GetNolIssuanceAsync(id, isBreakGlass, ct);
        if (result is null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpPut("{id:guid}/nol-issuance")]
    [RequireCapability(Capability.IssueNolRl)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SaveNolIssuance(Guid id, [FromBody] SaveNolIssuanceRequest request, CancellationToken ct)
    {
        var result = await companyService.SaveNolIssuanceAsync(
            id,
            request,
            currentUser.UserId,
            currentUser.Permissions,
            ct);

        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails { Detail = result.Error });
        }

        return Ok();
    }
}
