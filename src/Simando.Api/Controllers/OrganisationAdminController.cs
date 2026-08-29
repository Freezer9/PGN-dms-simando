using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Simando.Application.Organisation;
using Simando.Domain.Organisation;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;

namespace Simando.Api.Controllers;

public sealed record RegionWithAreasDto(
    Guid Id,
    string Code,
    string Name,
    bool Active,
    IReadOnlyList<AreaItemDto> Areas
);

public sealed record AreaItemDto(
    Guid Id,
    Guid RegionId,
    string Code,
    string Name,
    bool Active,
    int RecordCount = 0
);

public sealed record CreateRegionRequest(
    string Code,
    string Name
);

public sealed record UpdateRegionRequest(
    string Code,
    string Name,
    bool Active = true
);

public sealed record CreateAreaRequest(
    Guid RegionId,
    string Code,
    string Name
);

public sealed record UpdateAreaRequest(
    Guid RegionId,
    string Code,
    string Name,
    bool Active = true
);

[ApiController]
[Route("api/admin/organisation")]
[Authorize]
public sealed class OrganisationAdminController(
    IOrganisationService organisationService,
    ICurrentUser currentUser,
    IDbContextFactory<SimandoDbContext> dbContextFactory) : ControllerBase
{
    private bool IsAuthorizedAdmin() =>
        currentUser.IsAuthenticated &&
        (currentUser.Scope == AccessScope.All || currentUser.HasCapability(Capability.ManageMasterData));

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<RegionWithAreasDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOrganisation(CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();
        if (!IsAuthorizedAdmin()) return Forbid();

        var regions = await organisationService.GetRegionsAsync(ct);
        var areas = await organisationService.GetAreasAsync(ct);

        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var areaCounts = await db.Companies.IgnoreQueryFilters().AsNoTracking()
            .GroupBy(c => c.AreaId)
            .Select(g => new { AreaId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.AreaId, x => x.Count, ct);

        var areasByRegion = areas.GroupBy(a => a.RegionId).ToDictionary(
            g => g.Key,
            g => g.Select(a => new AreaItemDto(
                a.Id,
                a.RegionId,
                a.Code,
                a.Name,
                a.Active,
                areaCounts.GetValueOrDefault(a.Id, 0)
            )).OrderBy(a => a.Name).ToList()
        );

        var result = regions.Select(r => new RegionWithAreasDto(
            r.Id,
            r.Code,
            r.Name,
            r.Active,
            areasByRegion.GetValueOrDefault(r.Id, [])
        )).OrderBy(r => r.Name).ToList();

        return Ok(result);
    }

    [HttpPost("regions")]
    [ProducesResponseType<RegionWithAreasDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateRegion([FromBody] CreateRegionRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();
        if (!currentUser.HasCapability(Capability.ManageMasterData)) return Forbid();

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
        if (!currentUser.IsAuthenticated) return Unauthorized();
        if (!currentUser.HasCapability(Capability.ManageMasterData)) return Forbid();

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
        if (!currentUser.IsAuthenticated) return Unauthorized();
        if (!currentUser.HasCapability(Capability.ManageMasterData)) return Forbid();

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
        if (!currentUser.IsAuthenticated) return Unauthorized();
        if (!currentUser.HasCapability(Capability.ManageMasterData)) return Forbid();

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
        if (!currentUser.IsAuthenticated) return Unauthorized();
        if (!currentUser.HasCapability(Capability.ManageMasterData)) return Forbid();

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
        if (!currentUser.IsAuthenticated) return Unauthorized();
        if (!currentUser.HasCapability(Capability.ManageMasterData)) return Forbid();

        await organisationService.DeleteAreaAsync(id, ct);
        return NoContent();
    }
}
