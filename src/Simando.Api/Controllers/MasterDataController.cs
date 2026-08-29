using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simando.Application.MasterData;

namespace Simando.Api.Controllers;

[ApiController]
[Route("api/master")]
[Authorize]
public sealed class MasterDataController(IMasterDataLookupService lookupService) : ControllerBase
{
    [HttpGet("industry-types")]
    [ProducesResponseType<IReadOnlyList<IndustryTypeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIndustryTypes(CancellationToken ct)
    {
        var items = await lookupService.GetIndustryTypesAsync(ct);
        return Ok(items);
    }

    [HttpGet("areas")]
    [ProducesResponseType<IReadOnlyList<AreaDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAreas(CancellationToken ct)
    {
        var result = await lookupService.GetAreasAsync(ct);
        return Ok(result);
    }

    [HttpGet("sales-users")]
    [ProducesResponseType<IReadOnlyList<SalesUserDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSalesUsers(CancellationToken ct)
    {
        var users = await lookupService.GetSalesUsersAsync(ct);
        return Ok(users);
    }

    [HttpGet("fuel-types")]
    [ProducesResponseType<IReadOnlyList<FuelTypeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFuelTypes(CancellationToken ct)
    {
        var items = await lookupService.GetFuelTypesAsync(ct);
        return Ok(items);
    }

    [HttpGet("units")]
    [ProducesResponseType<IReadOnlyList<UnitOfMeasureDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnits(CancellationToken ct)
    {
        var items = await lookupService.GetUnitsAsync(ct);
        return Ok(items);
    }

    [HttpGet("countries")]
    [ProducesResponseType<IReadOnlyList<CountryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCountries(CancellationToken ct)
    {
        var items = await lookupService.GetCountriesAsync(ct);
        return Ok(items);
    }

    [HttpGet("segments")]
    [ProducesResponseType<IReadOnlyList<SegmentDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSegments(CancellationToken ct)
    {
        var items = await lookupService.GetSegmentsAsync(ct);
        return Ok(items);
    }

    [HttpGet("reference-documents")]
    [ProducesResponseType<IReadOnlyList<ReferenceDocumentDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReferenceDocuments(CancellationToken ct)
    {
        var items = await lookupService.GetReferenceDocumentsAsync(ct);
        return Ok(items);
    }

    [HttpGet("mrs-specs")]
    [ProducesResponseType<IReadOnlyList<MrsSpecDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMrsSpecs(CancellationToken ct)
    {
        var items = await lookupService.GetMrsSpecsAsync(ct);
        return Ok(items);
    }

    [HttpGet("meter-sizes")]
    [ProducesResponseType<IReadOnlyList<MeterSizeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMeterSizes(CancellationToken ct)
    {
        var items = await lookupService.GetMeterSizesAsync(ct);
        return Ok(items);
    }

    [HttpGet("reviewers")]
    [ProducesResponseType<IReadOnlyList<ReviewerOptionDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviewers(CancellationToken ct)
    {
        var result = await lookupService.GetReviewersAsync(ct);
        return Ok(result);
    }

    [HttpGet("reason-categories")]
    [ProducesResponseType<IReadOnlyList<ReasonCategoryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReasonCategories(CancellationToken ct)
    {
        var items = await lookupService.GetReasonCategoriesAsync(ct);
        return Ok(items);
    }
}
