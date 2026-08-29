using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Simando.Domain.MasterData;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;

namespace Simando.Api.Controllers;

public sealed record IndustryTypeDto(Guid Id, string Name, string? ContohProduk);

public sealed record AreaDto(Guid Id, string Name, string Code, Guid RegionId, string RegionName);

public sealed record SalesUserDto(Guid Id, string FullName, string Username, string Email);

public sealed record FuelTypeDto(Guid Id, string Name);

public sealed record UnitOfMeasureDto(Guid Id, string Code, string Name, UnitDimension Dimension);

public sealed record CountryDto(Guid Id, string IsoCode, string Name);

public sealed record SegmentDto(Guid Id, string Name, int SortOrder);

public sealed record ReferenceDocumentDto(Guid Id, string Name, int Version, DateOnly EffectiveFrom, DateOnly? EffectiveTo);

public sealed record MrsSpecDto(Guid Id, string Name);

public sealed record MeterSizeDto(Guid Id, string GSize, decimal NominalFlow, decimal MaxFlow, decimal PressureRating);

public sealed record ReviewerOptionDto(Guid Id, string FullName, string Username, string Email, Role Role);

public sealed record ReasonCategoryDto(Guid Id, string Name);

[ApiController]
[Route("api/master")]
[Authorize]
public sealed class MasterDataController(IDbContextFactory<SimandoDbContext> dbContextFactory) : ControllerBase
{
    [HttpGet("industry-types")]
    [ProducesResponseType<IReadOnlyList<IndustryTypeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIndustryTypes(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var items = await db.IndustryTypes
            .AsNoTracking()
            .OrderBy(i => i.Name)
            .Select(i => new IndustryTypeDto(i.Id, i.Name, i.ContohProduk))
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("areas")]
    [ProducesResponseType<IReadOnlyList<AreaDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAreas(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var regions = await db.Regions.AsNoTracking().ToDictionaryAsync(r => r.Id, r => r.Name, ct);

        var areas = await db.Areas
            .AsNoTracking()
            .Where(a => a.Active)
            .OrderBy(a => a.Name)
            .ToListAsync(ct);

        var result = areas.Select(a => new AreaDto(
            a.Id,
            a.Name,
            a.Code,
            a.RegionId,
            regions.GetValueOrDefault(a.RegionId, "Region")
        )).ToList();

        return Ok(result);
    }

    [HttpGet("sales-users")]
    [ProducesResponseType<IReadOnlyList<SalesUserDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSalesUsers(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var salesUserIds = await db.RoleAssignments
            .AsNoTracking()
            .Where(a => a.Active && (a.Role == Role.SalesArea || a.Role == Role.AreaHead))
            .Select(a => a.UserId)
            .Distinct()
            .ToListAsync(ct);

        var users = await db.Users
            .AsNoTracking()
            .Where(u => u.Active && salesUserIds.Contains(u.Id))
            .OrderBy(u => u.FullName)
            .Select(u => new SalesUserDto(u.Id, u.FullName, u.UserName ?? string.Empty, u.Email ?? string.Empty))
            .ToListAsync(ct);

        return Ok(users);
    }

    [HttpGet("fuel-types")]
    [ProducesResponseType<IReadOnlyList<FuelTypeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFuelTypes(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var items = await db.FuelTypes
            .AsNoTracking()
            .OrderBy(f => f.Name)
            .Select(f => new FuelTypeDto(f.Id, f.Name))
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("units")]
    [ProducesResponseType<IReadOnlyList<UnitOfMeasureDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnits(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var items = await db.UnitsOfMeasure
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .Select(u => new UnitOfMeasureDto(u.Id, u.Code, u.Name, u.Dimension))
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("countries")]
    [ProducesResponseType<IReadOnlyList<CountryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCountries(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var items = await db.Countries
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CountryDto(c.Id, c.IsoCode, c.Name))
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("segments")]
    [ProducesResponseType<IReadOnlyList<SegmentDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSegments(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var items = await db.Segments
            .AsNoTracking()
            .OrderBy(s => s.SortOrder)
            .Select(s => new SegmentDto(s.Id, s.Name, s.SortOrder))
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("reference-documents")]
    [ProducesResponseType<IReadOnlyList<ReferenceDocumentDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReferenceDocuments(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var items = await db.ReferenceDocuments
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new ReferenceDocumentDto(r.Id, r.Name, r.Version, r.EffectiveFrom, r.EffectiveTo))
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("mrs-specs")]
    [ProducesResponseType<IReadOnlyList<MrsSpecDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMrsSpecs(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var items = await db.MrsSpecs
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .Select(m => new MrsSpecDto(m.Id, m.Name))
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("meter-sizes")]
    [ProducesResponseType<IReadOnlyList<MeterSizeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMeterSizes(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var items = await db.MeterSizes
            .AsNoTracking()
            .OrderBy(m => m.GSize)
            .Select(m => new MeterSizeDto(m.Id, m.GSize, m.NominalFlow, m.MaxFlow, m.PressureRating))
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("reviewers")]
    [ProducesResponseType<IReadOnlyList<ReviewerOptionDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviewers(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var reviewerRoles = new HashSet<Role> { Role.Reviewer, Role.RegionalAdmin, Role.DivisionHead, Role.AreaHead };
        var assignments = await db.RoleAssignments
            .AsNoTracking()
            .Where(a => a.Active && (a.Role == Role.Reviewer || a.Role == Role.RegionalAdmin || a.Role == Role.DivisionHead || a.Role == Role.AreaHead))
            .ToListAsync(ct);

        var userIds = assignments.Select(a => a.UserId).Distinct().ToHashSet();
        var users = await db.Users
            .AsNoTracking()
            .Where(u => u.Active && userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, ct);

        var result = assignments
            .Where(a => users.ContainsKey(a.UserId))
            .Select(a =>
            {
                var u = users[a.UserId];
                return new ReviewerOptionDto(u.Id, u.FullName, u.UserName ?? string.Empty, u.Email ?? string.Empty, a.Role);
            })
            .OrderBy(r => r.FullName)
            .ToList();

        return Ok(result);
    }

    [HttpGet("reason-categories")]
    [ProducesResponseType<IReadOnlyList<ReasonCategoryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReasonCategories(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var items = await db.ReasonCategories
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new ReasonCategoryDto(r.Id, r.Name))
            .ToListAsync(ct);

        return Ok(items);
    }
}
