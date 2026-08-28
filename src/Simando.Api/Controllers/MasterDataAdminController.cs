using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Simando.Domain.MasterData;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;

namespace Simando.Api.Controllers;

public sealed record CreateIndustryTypeRequest(string Name, string? ContohProduk);
public sealed record UpdateIndustryTypeRequest(string Name, string? ContohProduk);

public sealed record CreateSegmentRequest(string Name, int SortOrder);
public sealed record UpdateSegmentRequest(string Name, int SortOrder);

public sealed record CreateFuelTypeRequest(string Name);
public sealed record UpdateFuelTypeRequest(string Name);

public sealed record CreateUnitRequest(string Code, string Name, UnitDimension Dimension);
public sealed record UpdateUnitRequest(string Code, string Name, UnitDimension Dimension);

public sealed record CreateMeterSizeRequest(string GSize, decimal NominalFlow, decimal MaxFlow, decimal PressureRating);
public sealed record UpdateMeterSizeRequest(string GSize, decimal NominalFlow, decimal MaxFlow, decimal PressureRating);

public sealed record CreateMrsSpecRequest(string Name);
public sealed record UpdateMrsSpecRequest(string Name);

public sealed record CreateReasonCategoryRequest(string Name);
public sealed record UpdateReasonCategoryRequest(string Name);

public sealed record CreateReferenceDocumentRequest(string Name, int Version, DateOnly EffectiveFrom, DateOnly? EffectiveTo, string? BlobKey = null);
public sealed record UpdateReferenceDocumentRequest(string Name, int Version, DateOnly EffectiveFrom, DateOnly? EffectiveTo, string? BlobKey = null);

[ApiController]
[Route("api/admin/master")]
[Authorize]
public sealed class MasterDataAdminController(
    IDbContextFactory<SimandoDbContext> dbContextFactory) : ControllerBase
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
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = new IndustryType
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            ContohProduk = request.ContohProduk?.Trim()
        };

        db.IndustryTypes.Add(entity);
        await db.SaveChangesAsync(ct);
        return Ok(new IndustryTypeDto(entity.Id, entity.Name, entity.ContohProduk));
    }

    [HttpPut("industry-types/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateIndustryType(Guid id, [FromBody] UpdateIndustryTypeRequest request, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = await db.IndustryTypes.FirstOrDefaultAsync(i => i.Id == id, ct);
        if (entity is null) return NotFound();

        entity.Name = request.Name.Trim();
        entity.ContohProduk = request.ContohProduk?.Trim();
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("industry-types/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteIndustryType(Guid id, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = await db.IndustryTypes.FirstOrDefaultAsync(i => i.Id == id, ct);
        if (entity is null) return NotFound();

        entity.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    // ==========================================
    // 2. Segments
    // ==========================================
    [HttpPost("segments")]
    [ProducesResponseType<SegmentDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateSegment([FromBody] CreateSegmentRequest request, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = new Segment
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            SortOrder = request.SortOrder
        };

        db.Segments.Add(entity);
        await db.SaveChangesAsync(ct);
        return Ok(new SegmentDto(entity.Id, entity.Name, entity.SortOrder));
    }

    [HttpPut("segments/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateSegment(Guid id, [FromBody] UpdateSegmentRequest request, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = await db.Segments.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (entity is null) return NotFound();

        entity.Name = request.Name.Trim();
        entity.SortOrder = request.SortOrder;
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("segments/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteSegment(Guid id, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = await db.Segments.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (entity is null) return NotFound();

        entity.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    // ==========================================
    // 3. Fuel Types
    // ==========================================
    [HttpPost("fuel-types")]
    [ProducesResponseType<FuelTypeDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateFuelType([FromBody] CreateFuelTypeRequest request, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = new FuelType
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim()
        };

        db.FuelTypes.Add(entity);
        await db.SaveChangesAsync(ct);
        return Ok(new FuelTypeDto(entity.Id, entity.Name));
    }

    [HttpPut("fuel-types/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateFuelType(Guid id, [FromBody] UpdateFuelTypeRequest request, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = await db.FuelTypes.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (entity is null) return NotFound();

        entity.Name = request.Name.Trim();
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("fuel-types/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteFuelType(Guid id, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = await db.FuelTypes.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (entity is null) return NotFound();

        entity.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    // ==========================================
    // 4. Units
    // ==========================================
    [HttpPost("units")]
    [ProducesResponseType<UnitOfMeasureDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateUnit([FromBody] CreateUnitRequest request, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = new UnitOfMeasure
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Dimension = request.Dimension
        };

        db.UnitsOfMeasure.Add(entity);
        await db.SaveChangesAsync(ct);
        return Ok(new UnitOfMeasureDto(entity.Id, entity.Code, entity.Name, entity.Dimension));
    }

    [HttpPut("units/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateUnit(Guid id, [FromBody] UpdateUnitRequest request, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = await db.UnitsOfMeasure.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (entity is null) return NotFound();

        entity.Code = request.Code.Trim();
        entity.Name = request.Name.Trim();
        entity.Dimension = request.Dimension;
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("units/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteUnit(Guid id, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = await db.UnitsOfMeasure.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (entity is null) return NotFound();

        entity.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    // ==========================================
    // 5. Meter Sizes
    // ==========================================
    [HttpPost("meter-sizes")]
    [ProducesResponseType<MeterSizeDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateMeterSize([FromBody] CreateMeterSizeRequest request, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = new MeterSize
        {
            Id = Guid.NewGuid(),
            GSize = request.GSize.Trim(),
            NominalFlow = request.NominalFlow,
            MaxFlow = request.MaxFlow,
            PressureRating = request.PressureRating
        };

        db.MeterSizes.Add(entity);
        await db.SaveChangesAsync(ct);
        return Ok(new MeterSizeDto(entity.Id, entity.GSize, entity.NominalFlow, entity.MaxFlow, entity.PressureRating));
    }

    [HttpPut("meter-sizes/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateMeterSize(Guid id, [FromBody] UpdateMeterSizeRequest request, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = await db.MeterSizes.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (entity is null) return NotFound();

        entity.GSize = request.GSize.Trim();
        entity.NominalFlow = request.NominalFlow;
        entity.MaxFlow = request.MaxFlow;
        entity.PressureRating = request.PressureRating;
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("meter-sizes/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteMeterSize(Guid id, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = await db.MeterSizes.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (entity is null) return NotFound();

        entity.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    // ==========================================
    // 6. MRS Specs
    // ==========================================
    [HttpPost("mrs-specs")]
    [ProducesResponseType<MrsSpecDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateMrsSpec([FromBody] CreateMrsSpecRequest request, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = new MrsSpec
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim()
        };

        db.MrsSpecs.Add(entity);
        await db.SaveChangesAsync(ct);
        return Ok(new MrsSpecDto(entity.Id, entity.Name));
    }

    [HttpPut("mrs-specs/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateMrsSpec(Guid id, [FromBody] UpdateMrsSpecRequest request, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = await db.MrsSpecs.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (entity is null) return NotFound();

        entity.Name = request.Name.Trim();
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("mrs-specs/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteMrsSpec(Guid id, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = await db.MrsSpecs.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (entity is null) return NotFound();

        entity.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    // ==========================================
    // 7. Reason Categories
    // ==========================================
    [HttpPost("reason-categories")]
    [ProducesResponseType<ReasonCategoryDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateReasonCategory([FromBody] CreateReasonCategoryRequest request, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = new ReasonCategory
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim()
        };

        db.ReasonCategories.Add(entity);
        await db.SaveChangesAsync(ct);
        return Ok(new ReasonCategoryDto(entity.Id, entity.Name));
    }

    [HttpPut("reason-categories/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateReasonCategory(Guid id, [FromBody] UpdateReasonCategoryRequest request, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = await db.ReasonCategories.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (entity is null) return NotFound();

        entity.Name = request.Name.Trim();
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("reason-categories/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteReasonCategory(Guid id, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = await db.ReasonCategories.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (entity is null) return NotFound();

        entity.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    // ==========================================
    // 8. Reference Documents
    // ==========================================
    [HttpPost("reference-documents")]
    [ProducesResponseType<ReferenceDocumentDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateReferenceDocument([FromBody] CreateReferenceDocumentRequest request, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = new ReferenceDocument
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Version = request.Version,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            BlobKey = request.BlobKey
        };

        db.ReferenceDocuments.Add(entity);
        await db.SaveChangesAsync(ct);
        return Ok(new ReferenceDocumentDto(entity.Id, entity.Name, entity.Version, entity.EffectiveFrom, entity.EffectiveTo));
    }

    [HttpPut("reference-documents/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateReferenceDocument(Guid id, [FromBody] UpdateReferenceDocumentRequest request, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = await db.ReferenceDocuments.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (entity is null) return NotFound();

        // EffectiveTo or update fields
        db.Entry(entity).CurrentValues.SetValues(new
        {
            Name = request.Name.Trim(),
            Version = request.Version,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            BlobKey = request.BlobKey
        });

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("reference-documents/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteReferenceDocument(Guid id, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        if (!await AuthorizeAdminAsync(db, ct)) return Forbid();

        var entity = await db.ReferenceDocuments.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (entity is null) return NotFound();

        db.ReferenceDocuments.Remove(entity);
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task<bool> AuthorizeAdminAsync(SimandoDbContext db, CancellationToken ct)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (idClaim is null || !Guid.TryParse(idClaim, out var userId))
        {
            return false;
        }

        var assignments = await db.RoleAssignments
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.Active)
            .ToListAsync(ct);

        var permissions = PermissionEvaluator.Resolve(assignments);
        return permissions.HasCapability(Capability.ManageMasterData);
    }
}

public sealed record ReasonCategoryDto(Guid Id, string Name);
