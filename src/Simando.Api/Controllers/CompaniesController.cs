using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simando.Application.Common;
using Simando.Application.Directory;
using Simando.Application.RecordHub;
using Simando.Domain.Directory;
using Simando.Domain.Security;

namespace Simando.Api.Controllers;

[ApiController]
[Route("api/companies")]
[Authorize]
public sealed class CompaniesController(
    ICompanyService companyService,
    ICompanyDetailService companyDetailService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<CompanyListItem>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] byte? stage = null,
        [FromQuery] Guid? industryTypeId = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? provinceId = null,
        [FromQuery] Guid? regencyId = null,
        [FromQuery] Guid? districtId = null,
        [FromQuery] Guid? villageId = null,
        [FromQuery] PosisiPelanggan? posisiPelanggan = null,
        [FromQuery] Kawasan? kawasan = null,
        CancellationToken ct = default)
    {
        var filter = new CompanyListFilter(
            provinceId,
            regencyId,
            districtId,
            villageId,
            industryTypeId,
            stage,
            posisiPelanggan,
            kawasan,
            searchTerm,
            page,
            pageSize);

        var result = await companyService.GetPagedListAsync(filter, ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType<CreateCompanyResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateCompanyRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Unauthorized();
        }

        if (!currentUser.HasCapability(Capability.CreateCompany))
        {
            return Forbid();
        }

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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Unauthorized();
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCompanyRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Unauthorized();
        }

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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Unauthorized();
        }

        if (!currentUser.HasCapability(Capability.SoftDeleteCompany))
        {
            return Forbid();
        }

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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateLocation(Guid id, [FromBody] UpdateLocationRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Unauthorized();
        }

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
    public async Task<IActionResult> GetTimeline(Guid id, CancellationToken ct)
    {
        var timeline = await companyDetailService.GetTimelineAsync(id, ct);
        return Ok(timeline);
    }

    [HttpGet("map-pins")]
    [ProducesResponseType<IReadOnlyList<CompanyMapPinDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMapPins(CancellationToken ct)
    {
        var pins = await companyService.GetMapPinsAsync(ct);
        return Ok(pins);
    }
}
