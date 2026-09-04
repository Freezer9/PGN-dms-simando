using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simando.Api.Security;
using Simando.Application.Common;
using Simando.Application.Directory;
using Simando.Application.RecordHub;
using Simando.Application.Security;
using Simando.Domain.Directory;
using Simando.Domain.Security;

namespace Simando.Api.Controllers;

[ApiController]
[Route("api/companies")]
[Authorize]
public sealed class CompaniesController(
    ICompanyService companyService,
    ICompanyDetailService companyDetailService,
    IBreakGlassService breakGlassService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    [RequireCapability(Capability.ViewCompanyRecords)]
    [ProducesResponseType<PagedResult<CompanyListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetList(
        [FromQuery] CompanyListFilter filter,
        CancellationToken ct = default)
    {
        var result = await companyService.GetPagedListAsync(filter, ct);
        return Ok(result);
    }

    [HttpPost]
    [RequireCapability(Capability.CreateCompany)]
    [ProducesResponseType<CreateCompanyResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateCompanyRequest request, CancellationToken ct)
    {
        // If actor is Area-scoped, ensure company belongs to their Area
        if (currentUser.Scope == AccessScope.Area && currentUser.AreaId is { } actorAreaId)
        {
            request = request with { AreaId = actorAreaId };
        }

        var result = await companyService.CreateAsync(request, currentUser.UserId, currentUser.Permissions, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<CompanyRecordDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var hasViewCapability = currentUser.HasCapability(Capability.ViewCompanyRecords);
        var hasActiveBreakGlass = !hasViewCapability && await breakGlassService.HasActiveAccessAsync(currentUser.UserId, id, ct);

        if (!hasViewCapability && !hasActiveBreakGlass)
        {
            return Forbid();
        }

        var record = await companyDetailService.GetCompanyRecordAsync(
            id,
            currentUser.UserId,
            currentUser.Permissions,
            currentUser.Roles,
            ct);

        if (record is null)
        {
            return NotFound();
        }

        return Ok(record);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCompanyRequest request, CancellationToken ct)
    {
        var result = await companyService.UpdateBasicInfoAsync(
            id,
            request,
            currentUser.UserId,
            currentUser.Permissions,
            ct);

        if (!result.Succeeded)
        {
            if (result.Error == "Berkas tidak ditemukan.")
            {
                return NotFound();
            }

            return BadRequest(new ProblemDetails
            {
                Title = "Edit Rejected",
                Detail = result.Error
            });
        }

        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [RequireCapability(Capability.SoftDeleteCompany)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await companyService.SoftDeleteAsync(id, currentUser.UserId, ct);
        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Delete Failed",
                Detail = result.Error
            });
        }

        return NoContent();
    }

    [HttpPut("{id:guid}/location")]
    [RequireCapability(Capability.DropMovePin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateLocation(Guid id, [FromBody] UpdateLocationRequest request, CancellationToken ct)
    {
        var result = await companyService.UpdateLocationAsync(
            id,
            request.Latitude,
            request.Longitude,
            currentUser.UserId,
            currentUser.Permissions,
            ct);

        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Update Location Failed",
                Detail = result.Error
            });
        }

        return Ok();
    }

    [HttpGet("{id:guid}/timeline")]
    [ProducesResponseType<IReadOnlyList<TimelineEntry>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTimeline(Guid id, CancellationToken ct)
    {
        var hasTimelineCapability = currentUser.HasCapability(Capability.ViewTimeline);
        var hasActiveBreakGlass = !hasTimelineCapability && await breakGlassService.HasActiveAccessAsync(currentUser.UserId, id, ct);

        if (!hasTimelineCapability && !hasActiveBreakGlass)
        {
            return Forbid();
        }

        var timeline = await companyDetailService.GetTimelineAsync(id, hasActiveBreakGlass, ct);
        return Ok(timeline);
    }

    [HttpGet("map-pins")]
    [RequireCapability(Capability.ViewCompanyRecords)]
    [ProducesResponseType<IReadOnlyList<CompanyMapPinDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMapPins(
        [FromQuery] CompanyListFilter filter,
        CancellationToken ct)
    {
        var pins = await companyService.GetMapPinsAsync(filter, ct);
        return Ok(pins);
    }
}
