using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simando.Application.Geography;

namespace Simando.Api.Controllers;

[ApiController]
[Route("api/geography")]
[Authorize]
public sealed class GeographyController(IGeographyService geographyService) : ControllerBase
{
    [HttpGet("provinces")]
    [ProducesResponseType<IReadOnlyList<GeographyOption>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProvinces(CancellationToken ct)
    {
        var result = await geographyService.GetProvincesAsync(ct);
        return Ok(result);
    }

    [HttpGet("regencies")]
    [ProducesResponseType<IReadOnlyList<GeographyOption>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRegencies([FromQuery] Guid provinceId, CancellationToken ct)
    {
        var result = await geographyService.GetRegenciesAsync(provinceId, ct);
        return Ok(result);
    }

    [HttpGet("districts")]
    [ProducesResponseType<IReadOnlyList<GeographyOption>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDistricts([FromQuery] Guid regencyId, CancellationToken ct)
    {
        var result = await geographyService.GetDistrictsAsync(regencyId, ct);
        return Ok(result);
    }

    [HttpGet("villages")]
    [ProducesResponseType<IReadOnlyList<GeographyOption>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVillages([FromQuery] Guid districtId, CancellationToken ct)
    {
        var result = await geographyService.GetVillagesAsync(districtId, ct);
        return Ok(result);
    }
}
