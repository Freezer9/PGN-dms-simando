using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simando.Api.Security;
using Simando.Application.Organisation;
using Simando.Domain.Organisation;
using Simando.Domain.Security;

namespace Simando.Api.Controllers;

[ApiController]
[Route("api/admin/organisation")]
[Authorize]
[RequireCapability(Capability.ManageMasterData)]
public sealed class OrganisationAdminController(IOrganisationService organisationService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<RegionWithAreasDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOrganisation(CancellationToken ct)
    {
        var result = await organisationService.GetOrganisationHierarchyAsync(ct);
        return Ok(result);
    }

    [HttpPost("regions")]
    [ProducesResponseType<RegionWithAreasDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateRegion([FromBody] CreateRegionRequest request, CancellationToken ct)
    {
        var region = new Region
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Active = true
        };

        await organisationService.AddRegionAsync(region, ct);
        return Ok(new RegionWithAreasDto(region.Id, region.Code, region.Name, region.Active, []));
    }

    [HttpPut("regions/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateRegion(Guid id, [FromBody] UpdateRegionRequest request, CancellationToken ct)
    {
        await organisationService.UpdateRegionAsync(id, r =>
        {
            r.Code = request.Code.Trim();
            r.Name = request.Name.Trim();
            r.Active = request.Active;
        }, ct);

        return NoContent();
    }

    [HttpDelete("regions/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteRegion(Guid id, CancellationToken ct)
    {
        await organisationService.DeleteRegionAsync(id, ct);
        return NoContent();
    }

    [HttpPost("areas")]
    [ProducesResponseType<AreaItemDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateArea([FromBody] CreateAreaRequest request, CancellationToken ct)
    {
        var area = new Area
        {
            Id = Guid.NewGuid(),
            RegionId = request.RegionId,
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Active = true
        };

        await organisationService.AddAreaAsync(area, ct);
        return Ok(new AreaItemDto(area.Id, area.RegionId, area.Code, area.Name, area.Active, 0));
    }

    [HttpPut("areas/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateArea(Guid id, [FromBody] UpdateAreaRequest request, CancellationToken ct)
    {
        await organisationService.UpdateAreaAsync(id, a =>
        {
            a.RegionId = request.RegionId;
            a.Code = request.Code.Trim();
            a.Name = request.Name.Trim();
            a.Active = request.Active;
        }, ct);

        return NoContent();
    }

    [HttpDelete("areas/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteArea(Guid id, CancellationToken ct)
    {
        await organisationService.DeleteAreaAsync(id, ct);
        return NoContent();
    }
}
