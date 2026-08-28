using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Simando.Domain.Security;
using Simando.Infrastructure.Persistence;

namespace Simando.Api.Controllers;

public sealed record IndustryTypeDto(Guid Id, string Name, string? ContohProduk);

public sealed record AreaDto(Guid Id, string Name, string Code, Guid RegionId, string RegionName);

public sealed record SalesUserDto(Guid Id, string FullName, string Username, string Email);

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
}
