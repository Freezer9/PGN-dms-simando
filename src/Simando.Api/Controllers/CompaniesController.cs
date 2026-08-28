using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Simando.Application.Common;
using Simando.Application.Directory;
using Simando.Application.RecordHub;
using Simando.Domain.Directory;
using Simando.Domain.Geography;
using Simando.Domain.Security;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Persistence;

namespace Simando.Api.Controllers;

public sealed record UpdateLocationRequest(double Latitude, double Longitude);

public sealed record UpdateCompanyRequest(
    string NamaPerusahaan,
    string? Website,
    Guid VillageId,
    string Alamat,
    double Latitude,
    double Longitude,
    Guid IndustryTypeId,
    string? Email,
    string? KodePos,
    string? Telp,
    string? Npwp);

public sealed record CompanyMapPinDto(
    Guid Id,
    string Nomor,
    string NamaPerusahaan,
    double Latitude,
    double Longitude,
    byte CurrentStage,
    RecordStatus Status,
    string IndustryTypeName,
    string LocationLabel,
    PosisiPelanggan? PosisiPelanggan,
    Kawasan? Kawasan,
    string? SalesUserName);

public sealed record CompanyRecordDto(
    Guid Id,
    string Nomor,
    string NamaPerusahaan,
    string? Website,
    string Alamat,
    Guid VillageId,
    string VillageName,
    Guid DistrictId,
    string DistrictName,
    Guid RegencyId,
    string RegencyName,
    Guid ProvinceId,
    string ProvinceName,
    string LocationLabel,
    Guid IndustryTypeId,
    string IndustryTypeName,
    string? Npwp,
    string? Email,
    string? KodePos,
    string? Telp,
    Guid AreaId,
    string AreaName,
    Guid RegionId,
    string RegionName,
    byte CurrentStage,
    RecordStatus Status,
    Guid CreatedBy,
    string SalesRepName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    double? Latitude,
    double? Longitude,
    string? HolderLabel,
    string? HolderName,
    DateTimeOffset StatusSince,
    Guid? CurrentStepId,
    WorkflowStepKind? CurrentStepKind,
    Guid? WorkflowInstanceId,
    bool CanSubmit,
    bool CanAct,
    bool CanChooseReviewers,
    IReadOnlyList<ContactDetail> Contacts);

[ApiController]
[Route("api/companies")]
[Authorize]
public sealed class CompaniesController(
    ICompanyService companyService,
    ICompanyDetailService companyDetailService,
    IDbContextFactory<SimandoDbContext> dbContextFactory) : ControllerBase
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
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actorContext = await ResolveActorContextAsync(db, ct);
        if (actorContext is null)
        {
            return Unauthorized();
        }

        if (!actorContext.Value.Permissions.HasCapability(Capability.CreateCompany))
        {
            return Forbid();
        }

        // If actor is Area-scoped, ensure company belongs to their Area
        if (actorContext.Value.Permissions.Scope == AccessScope.Area && actorContext.Value.Permissions.AreaId is { } actorAreaId)
        {
            request = request with { AreaId = actorAreaId };
        }

        var result = await companyService.CreateAsync(request, actorContext.Value.UserId, actorContext.Value.Permissions, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<CompanyRecordDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actorContext = await ResolveActorContextAsync(db, ct);
        if (actorContext is null)
        {
            return Unauthorized();
        }

        var detail = await companyDetailService.GetDetailAsync(
            id,
            actorContext.Value.UserId,
            actorContext.Value.Permissions,
            actorContext.Value.Roles,
            ct);

        if (detail is null)
        {
            return NotFound();
        }

        var company = await db.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        if (company is null)
        {
            return NotFound();
        }

        var village = await db.Villages.AsNoTracking().FirstOrDefaultAsync(v => v.Id == company.VillageId, ct);
        var district = village is not null ? await db.Districts.AsNoTracking().FirstOrDefaultAsync(d => d.Id == village.DistrictId, ct) : null;
        var regency = district is not null ? await db.Regencies.AsNoTracking().FirstOrDefaultAsync(r => r.Id == district.RegencyId, ct) : null;
        var province = regency is not null ? await db.Provinces.AsNoTracking().FirstOrDefaultAsync(p => p.Id == regency.ProvinceId, ct) : null;

        var contacts = await companyService.GetContactsAsync(id, ct);

        var record = new CompanyRecordDto(
            company.Id,
            company.Nomor,
            company.NamaPerusahaan,
            company.Website,
            company.Alamat,
            company.VillageId,
            village?.Name ?? "Desa/Kelurahan",
            district?.Id ?? Guid.Empty,
            district?.Name ?? "Kecamatan",
            regency?.Id ?? Guid.Empty,
            regency?.Name ?? "Kota/Kabupaten",
            province?.Id ?? Guid.Empty,
            province?.Name ?? "Provinsi",
            detail.LocationLabel,
            company.IndustryTypeId,
            detail.IndustryTypeName,
            company.Npwp,
            company.Email,
            company.KodePos,
            company.Telp,
            company.AreaId,
            detail.AreaName,
            detail.RegionId,
            detail.RegionName,
            company.CurrentStage,
            company.Status,
            company.CreatedBy,
            detail.SalesRepName,
            company.CreatedAt,
            company.UpdatedAt,
            company.Location?.Y,
            company.Location?.X,
            detail.HolderLabel,
            detail.HolderName,
            detail.StatusSince,
            detail.CurrentStepId,
            detail.CurrentStepKind,
            detail.WorkflowInstanceId,
            detail.CanSubmit,
            detail.CanAct,
            detail.CanChooseReviewers,
            contacts);

        return Ok(record);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCompanyRequest request, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actorContext = await ResolveActorContextAsync(db, ct);
        if (actorContext is null)
        {
            return Unauthorized();
        }

        var company = await db.Companies.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (company is null)
        {
            return NotFound();
        }

        if (company.Status != RecordStatus.Draft)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Edit Rejected",
                Detail = "Hanya berkas berstatus Draft yang dapat diperbarui informasinya."
            });
        }

        if (!actorContext.Value.Permissions.HasCapability(Capability.EditStages1To3))
        {
            return Forbid();
        }

        var village = await db.Villages.AsNoTracking().FirstOrDefaultAsync(v => v.Id == request.VillageId, ct);
        if (village is null)
        {
            return BadRequest(new ProblemDetails { Detail = "Data Kelurahan/Desa tidak valid." });
        }
        var district = await db.Districts.AsNoTracking().FirstOrDefaultAsync(d => d.Id == village.DistrictId, ct);
        var regency = district is not null ? await db.Regencies.AsNoTracking().FirstOrDefaultAsync(r => r.Id == district.RegencyId, ct) : null;
        var province = regency is not null ? await db.Provinces.AsNoTracking().FirstOrDefaultAsync(p => p.Id == regency.ProvinceId, ct) : null;

        company.NamaPerusahaan = request.NamaPerusahaan;
        company.Website = request.Website;
        company.VillageId = request.VillageId;
        company.Alamat = request.Alamat;
        company.Location = new Point(request.Longitude, request.Latitude) { SRID = 4326 };
        company.IndustryTypeId = request.IndustryTypeId;
        company.Email = request.Email;
        company.KodePos = request.KodePos;
        company.Telp = request.Telp;
        company.Npwp = request.Npwp;
        company.UpdatedAt = DateTimeOffset.UtcNow;

        if (province is not null && regency is not null)
        {
            company.Nomor = $"{company.NomorSeq:0000000}-{province.BpsCode}-{regency.BpsCode}";
        }

        await db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actorContext = await ResolveActorContextAsync(db, ct);
        if (actorContext is null)
        {
            return Unauthorized();
        }

        var result = await companyService.SoftDeleteAsync(id, actorContext.Value.UserId, ct);
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

    [HttpGet("{id:guid}/contacts")]
    [ProducesResponseType<IReadOnlyList<ContactDetail>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContacts(Guid id, CancellationToken ct)
    {
        var result = await companyService.GetContactsAsync(id, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/contacts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddContact(Guid id, [FromBody] SaveContactRequest request, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actorContext = await ResolveActorContextAsync(db, ct);
        if (actorContext is null)
        {
            return Unauthorized();
        }

        var result = await companyService.AddContactAsync(id, request, actorContext.Value.UserId, actorContext.Value.Permissions, ct);
        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails { Detail = result.Error });
        }

        return Ok();
    }

    [HttpPut("{id:guid}/contacts/{contactId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateContact(Guid id, Guid contactId, [FromBody] SaveContactRequest request, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actorContext = await ResolveActorContextAsync(db, ct);
        if (actorContext is null)
        {
            return Unauthorized();
        }

        var result = await companyService.UpdateContactAsync(id, contactId, request, actorContext.Value.UserId, actorContext.Value.Permissions, ct);
        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails { Detail = result.Error });
        }

        return Ok();
    }

    [HttpDelete("{id:guid}/contacts/{contactId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteContact(Guid id, Guid contactId, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actorContext = await ResolveActorContextAsync(db, ct);
        if (actorContext is null)
        {
            return Unauthorized();
        }

        var result = await companyService.DeleteContactAsync(id, contactId, actorContext.Value.UserId, actorContext.Value.Permissions, ct);
        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails { Detail = result.Error });
        }

        return Ok();
    }

    [HttpGet("{id:guid}/plotting")]
    [ProducesResponseType<PlottingDetail>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlotting(Guid id, CancellationToken ct)
    {
        var result = await companyService.GetPlottingAsync(id, ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}/plotting")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SavePlotting(Guid id, [FromBody] SavePlottingRequest request, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actorContext = await ResolveActorContextAsync(db, ct);
        if (actorContext is null)
        {
            return Unauthorized();
        }

        var result = await companyService.SavePlottingAsync(id, request, actorContext.Value.UserId, actorContext.Value.Permissions, ct);
        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails { Detail = result.Error });
        }

        return Ok();
    }

    [HttpPost("{id:guid}/promote-to-prospek")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PromoteToProspek(Guid id, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actorContext = await ResolveActorContextAsync(db, ct);
        if (actorContext is null)
        {
            return Unauthorized();
        }

        var result = await companyService.PromoteToProspekAsync(id, actorContext.Value.UserId, actorContext.Value.Permissions, ct);
        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails { Detail = result.Error });
        }

        return Ok();
    }

    [HttpPut("{id:guid}/location")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateLocation(Guid id, [FromBody] UpdateLocationRequest request, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var actorContext = await ResolveActorContextAsync(db, ct);
        if (actorContext is null)
        {
            return Unauthorized();
        }

        var result = await companyService.UpdateLocationAsync(id, request.Latitude, request.Longitude, actorContext.Value.UserId, actorContext.Value.Permissions, ct);
        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails { Detail = result.Error });
        }

        return Ok();
    }

    [HttpGet("{id:guid}/timeline")]
    [ProducesResponseType<IReadOnlyList<TimelineEntry>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTimeline(Guid id, CancellationToken ct)
    {
        var result = await companyDetailService.GetTimelineAsync(id, ct);
        return Ok(result);
    }

    [HttpGet("map-pins")]
    [ProducesResponseType<IReadOnlyList<CompanyMapPinDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMapPins(CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var companies = await db.Companies
            .AsNoTracking()
            .Where(c => c.Location != null)
            .ToListAsync(ct);

        var villageIds = companies.Select(c => c.VillageId).ToHashSet();
        var villages = await db.Villages.AsNoTracking().Where(v => villageIds.Contains(v.Id)).ToDictionaryAsync(v => v.Id, ct);
        var districtIds = villages.Values.Select(v => v.DistrictId).ToHashSet();
        var districts = await db.Districts.AsNoTracking().Where(d => districtIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id, ct);
        var regencyIds = districts.Values.Select(d => d.RegencyId).ToHashSet();
        var regencies = await db.Regencies.AsNoTracking().Where(r => regencyIds.Contains(r.Id)).ToDictionaryAsync(r => r.Id, ct);

        var industryTypeIds = companies.Select(c => c.IndustryTypeId).ToHashSet();
        var industryTypeNames = await db.IndustryTypes.AsNoTracking()
            .Where(t => industryTypeIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Name, ct);

        var companyIds = companies.Select(c => c.Id).ToHashSet();
        var plottings = await db.Plottings.AsNoTracking()
            .Where(p => companyIds.Contains(p.CompanyId))
            .ToDictionaryAsync(p => p.CompanyId, ct);

        var salesUserIds = plottings.Values.Select(p => p.SalesUserId).ToHashSet();
        var salesUserNames = await db.Users.AsNoTracking()
            .Where(u => salesUserIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, ct);

        var pins = new List<CompanyMapPinDto>();
        foreach (var c in companies)
        {
            if (c.Location is null) continue;

            var village = villages.GetValueOrDefault(c.VillageId);
            var district = village is not null ? districts.GetValueOrDefault(village.DistrictId) : null;
            var regency = district is not null ? regencies.GetValueOrDefault(district.RegencyId) : null;
            var locationLabel = regency is not null
                ? $"{(regency.Type == RegencyType.Kota ? "Kota" : "Kabupaten")} {regency.Name}"
                : "Lokasi";

            var plotting = plottings.GetValueOrDefault(c.Id);

            pins.Add(new CompanyMapPinDto(
                c.Id,
                c.Nomor,
                c.NamaPerusahaan,
                c.Location.Y,
                c.Location.X,
                c.CurrentStage,
                c.Status,
                industryTypeNames.GetValueOrDefault(c.IndustryTypeId, "Industri"),
                locationLabel,
                plotting?.PosisiPelanggan,
                plotting?.Kawasan,
                plotting is null ? null : salesUserNames.GetValueOrDefault(plotting.SalesUserId)
            ));
        }

        return Ok(pins);
    }

    private async Task<(Guid UserId, EffectivePermissions Permissions, IReadOnlySet<Role> Roles)?> ResolveActorContextAsync(
        SimandoDbContext db,
        CancellationToken ct)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (idClaim is null || !Guid.TryParse(idClaim, out var userId))
        {
            return null;
        }

        var assignments = await db.RoleAssignments
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.Active)
            .ToListAsync(ct);

        var permissions = PermissionEvaluator.Resolve(assignments);
        var roles = assignments.Select(a => a.Role).ToHashSet();

        return (userId, permissions, roles);
    }
}
