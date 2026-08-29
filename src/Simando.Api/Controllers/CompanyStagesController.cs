using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simando.Application.Directory;
using Simando.Application.Nol;
using Simando.Application.Registration;
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
    ICurrentUser currentUser) : ControllerBase
{
    // ==========================================
    // Stage 1: Survey
    // ==========================================
    [HttpGet("{id:guid}/survey")]
    [ProducesResponseType<SurveyDetail>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSurvey(Guid id, CancellationToken ct)
    {
        var result = await companyService.GetSurveyAsync(id, ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}/survey")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveSurvey(Guid id, [FromBody] SaveSurveyFullPayload payload, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Unauthorized();
        }

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
    public async Task<IActionResult> GetPlotting(Guid id, CancellationToken ct)
    {
        var result = await companyService.GetPlottingAsync(id, ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}/plotting")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SavePlotting(Guid id, [FromBody] SavePlottingRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Unauthorized();
        }

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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PromoteToProspek(Guid id, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Unauthorized();
        }

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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRegistration(Guid id, CancellationToken ct)
    {
        var result = await companyService.GetA1RegistrationAsync(id, ct);
        if (result is null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpPut("{id:guid}/registration")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveRegistration(Guid id, [FromBody] SaveA1RegistrationRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Unauthorized();
        }

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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNolRequest(Guid id, CancellationToken ct)
    {
        var result = await companyService.GetNolRequestAsync(id, ct);
        if (result is null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpPut("{id:guid}/nol-request")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveNolRequest(Guid id, [FromBody] SaveNolRequestRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Unauthorized();
        }

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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNolEvaluation(Guid id, CancellationToken ct)
    {
        var result = await companyService.GetNolEvaluationAsync(id, ct);
        if (result is null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpPut("{id:guid}/nol-evaluation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveNolEvaluation(Guid id, [FromBody] SaveNolEvaluationRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Unauthorized();
        }

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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNolIssuance(Guid id, CancellationToken ct)
    {
        var result = await companyService.GetNolIssuanceAsync(id, ct);
        if (result is null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpPut("{id:guid}/nol-issuance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveNolIssuance(Guid id, [FromBody] SaveNolIssuanceRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Unauthorized();
        }

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
