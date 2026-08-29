using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simando.Api.Security;
using Simando.Application.MasterData;
using Simando.Domain.Security;

namespace Simando.Api.Controllers;

[ApiController]
[Route("api/admin/master")]
[Authorize]
[RequireCapability(Capability.ManageMasterData)]
public sealed class MasterDataAdminController(
    IMasterDataAdminService masterDataAdminService) : ControllerBase
{
    // ==========================================
    // 1. Industry Types
    // ==========================================
    [HttpPost("industry-types")]
    [ProducesResponseType<IndustryTypeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateIndustryType([FromBody] CreateIndustryTypeRequest request, CancellationToken ct)
    {
        var result = await masterDataAdminService.CreateIndustryTypeAsync(request, ct);
        return Ok(new IndustryTypeDto(result.Id, result.Name, result.ContohProduk));
    }

    [HttpPut("industry-types/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateIndustryType(Guid id, [FromBody] UpdateIndustryTypeRequest request, CancellationToken ct)
    {
        var updated = await masterDataAdminService.UpdateIndustryTypeAsync(id, request, ct);
        if (!updated) return NotFound();

        return NoContent();
    }

    [HttpDelete("industry-types/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteIndustryType(Guid id, CancellationToken ct)
    {
        var deleted = await masterDataAdminService.DeleteIndustryTypeAsync(id, ct);
        if (!deleted) return NotFound();

        return NoContent();
    }

    // ==========================================
    // 2. Segments
    // ==========================================
    [HttpPost("segments")]
    [ProducesResponseType<SegmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateSegment([FromBody] CreateSegmentRequest request, CancellationToken ct)
    {
        var result = await masterDataAdminService.CreateSegmentAsync(request, ct);
        return Ok(new SegmentDto(result.Id, result.Name, result.SortOrder));
    }

    [HttpPut("segments/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSegment(Guid id, [FromBody] UpdateSegmentRequest request, CancellationToken ct)
    {
        var updated = await masterDataAdminService.UpdateSegmentAsync(id, request, ct);
        if (!updated) return NotFound();

        return NoContent();
    }

    [HttpDelete("segments/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSegment(Guid id, CancellationToken ct)
    {
        var deleted = await masterDataAdminService.DeleteSegmentAsync(id, ct);
        if (!deleted) return NotFound();

        return NoContent();
    }

    // ==========================================
    // 3. Fuel Types
    // ==========================================
    [HttpPost("fuel-types")]
    [ProducesResponseType<FuelTypeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateFuelType([FromBody] CreateFuelTypeRequest request, CancellationToken ct)
    {
        var result = await masterDataAdminService.CreateFuelTypeAsync(request, ct);
        return Ok(new FuelTypeDto(result.Id, result.Name));
    }

    [HttpPut("fuel-types/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFuelType(Guid id, [FromBody] UpdateFuelTypeRequest request, CancellationToken ct)
    {
        var updated = await masterDataAdminService.UpdateFuelTypeAsync(id, request, ct);
        if (!updated) return NotFound();

        return NoContent();
    }

    [HttpDelete("fuel-types/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFuelType(Guid id, CancellationToken ct)
    {
        var deleted = await masterDataAdminService.DeleteFuelTypeAsync(id, ct);
        if (!deleted) return NotFound();

        return NoContent();
    }

    // ==========================================
    // 4. Units of Measure
    // ==========================================
    [HttpPost("units")]
    [ProducesResponseType<UnitOfMeasureDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateUnit([FromBody] CreateUnitRequest request, CancellationToken ct)
    {
        var result = await masterDataAdminService.CreateUnitAsync(request, ct);
        return Ok(new UnitOfMeasureDto(result.Id, result.Code, result.Name, result.Dimension));
    }

    [HttpPut("units/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUnit(Guid id, [FromBody] UpdateUnitRequest request, CancellationToken ct)
    {
        var updated = await masterDataAdminService.UpdateUnitAsync(id, request, ct);
        if (!updated) return NotFound();

        return NoContent();
    }

    [HttpDelete("units/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUnit(Guid id, CancellationToken ct)
    {
        var deleted = await masterDataAdminService.DeleteUnitAsync(id, ct);
        if (!deleted) return NotFound();

        return NoContent();
    }

    // ==========================================
    // 5. Meter Sizes
    // ==========================================
    [HttpPost("meter-sizes")]
    [ProducesResponseType<MeterSizeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateMeterSize([FromBody] CreateMeterSizeRequest request, CancellationToken ct)
    {
        var result = await masterDataAdminService.CreateMeterSizeAsync(request, ct);
        return Ok(new MeterSizeDto(result.Id, result.GSize, result.NominalFlow, result.MaxFlow, result.PressureRating));
    }

    [HttpPut("meter-sizes/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMeterSize(Guid id, [FromBody] UpdateMeterSizeRequest request, CancellationToken ct)
    {
        var updated = await masterDataAdminService.UpdateMeterSizeAsync(id, request, ct);
        if (!updated) return NotFound();

        return NoContent();
    }

    [HttpDelete("meter-sizes/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMeterSize(Guid id, CancellationToken ct)
    {
        var deleted = await masterDataAdminService.DeleteMeterSizeAsync(id, ct);
        if (!deleted) return NotFound();

        return NoContent();
    }

    // ==========================================
    // 6. MRS Specs
    // ==========================================
    [HttpPost("mrs-specs")]
    [ProducesResponseType<MrsSpecDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateMrsSpec([FromBody] CreateMrsSpecRequest request, CancellationToken ct)
    {
        var result = await masterDataAdminService.CreateMrsSpecAsync(request, ct);
        return Ok(new MrsSpecDto(result.Id, result.Name));
    }

    [HttpPut("mrs-specs/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMrsSpec(Guid id, [FromBody] UpdateMrsSpecRequest request, CancellationToken ct)
    {
        var updated = await masterDataAdminService.UpdateMrsSpecAsync(id, request, ct);
        if (!updated) return NotFound();

        return NoContent();
    }

    [HttpDelete("mrs-specs/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMrsSpec(Guid id, CancellationToken ct)
    {
        var deleted = await masterDataAdminService.DeleteMrsSpecAsync(id, ct);
        if (!deleted) return NotFound();

        return NoContent();
    }

    // ==========================================
    // 7. Reason Categories
    // ==========================================
    [HttpPost("reason-categories")]
    [ProducesResponseType<ReasonCategoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateReasonCategory([FromBody] CreateReasonCategoryRequest request, CancellationToken ct)
    {
        var result = await masterDataAdminService.CreateReasonCategoryAsync(request, ct);
        return Ok(new ReasonCategoryDto(result.Id, result.Name));
    }

    [HttpPut("reason-categories/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReasonCategory(Guid id, [FromBody] UpdateReasonCategoryRequest request, CancellationToken ct)
    {
        var updated = await masterDataAdminService.UpdateReasonCategoryAsync(id, request, ct);
        if (!updated) return NotFound();

        return NoContent();
    }

    [HttpDelete("reason-categories/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReasonCategory(Guid id, CancellationToken ct)
    {
        var deleted = await masterDataAdminService.DeleteReasonCategoryAsync(id, ct);
        if (!deleted) return NotFound();

        return NoContent();
    }

    // ==========================================
    // 8. Reference Documents
    // ==========================================
    [HttpPost("reference-documents")]
    [ProducesResponseType<ReferenceDocumentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateReferenceDocument([FromBody] CreateReferenceDocumentRequest request, CancellationToken ct)
    {
        var result = await masterDataAdminService.CreateReferenceDocumentAsync(request, ct);
        return Ok(new ReferenceDocumentDto(result.Id, result.Name, result.Version, result.EffectiveFrom, result.EffectiveTo));
    }

    [HttpPut("reference-documents/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReferenceDocument(Guid id, [FromBody] UpdateReferenceDocumentRequest request, CancellationToken ct)
    {
        var updated = await masterDataAdminService.UpdateReferenceDocumentAsync(id, request, ct);
        if (!updated) return NotFound();

        return NoContent();
    }

    [HttpDelete("reference-documents/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReferenceDocument(Guid id, CancellationToken ct)
    {
        var deleted = await masterDataAdminService.DeleteReferenceDocumentAsync(id, ct);
        if (!deleted) return NotFound();

        return NoContent();
    }
}
