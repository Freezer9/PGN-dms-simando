using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pgn.Dms.Api.Services;
using Pgn.Dms.Shared;

namespace Pgn.Dms.Api.Controllers;

/// <summary>Stage 2–6 satellite data. Reads are open to any authenticated user; writes
/// follow the Docs edit-permission matrix (SalesArea owns DRAFT, Regional Admin owns stage 7).</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StagesController(StageService stages) : ControllerBase
{
    private const string Editors = "SalesArea,RegionSales";

    private string UserName => User.FindFirst(ClaimTypes.Name)?.Value ?? "";

    // ── Plotting ────────────────────────────────────────────────────────────

    [HttpGet("{id:int}/plotting")]
    public async Task<ActionResult<PlottingDto>> GetPlotting(int id)
        => await stages.GetPlottingAsync(id) is { } dto ? Ok(dto) : NotFound();

    [HttpPut("{id:int}/plotting")]
    [Authorize(Roles = Editors)]
    public async Task<ActionResult<PlottingDto>> SavePlotting(int id, [FromBody] SavePlottingRequest req)
        => Ok(await stages.SavePlottingAsync(id, req, UserName));

    // ── Contacts ────────────────────────────────────────────────────────────

    [HttpGet("{id:int}/contacts")]
    public async Task<List<CompanyContactDto>> GetContacts(int id)
        => await stages.GetContactsAsync(id);

    [HttpPost("{id:int}/contacts")]
    [Authorize(Roles = Editors)]
    public async Task<ActionResult<CompanyContactDto>> SaveContact(int id, [FromBody] SaveContactRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Nama) || string.IsNullOrWhiteSpace(req.Jabatan))
            return BadRequest("Nama dan jabatan wajib diisi.");

        return Ok(await stages.SaveContactAsync(id, req, UserName));
    }

    [HttpDelete("{id:int}/contacts/{contactId:int}")]
    [Authorize(Roles = Editors)]
    public async Task<IActionResult> DeleteContact(int id, int contactId)
        => await stages.DeleteContactAsync(id, contactId, UserName) ? Ok() : NotFound();

    // ── Survey ──────────────────────────────────────────────────────────────

    [HttpGet("{id:int}/survey")]
    public async Task<ActionResult<SurveyDto>> GetSurvey(int id)
        => await stages.GetSurveyAsync(id) is { } dto ? Ok(dto) : NotFound();

    [HttpPut("{id:int}/survey")]
    [Authorize(Roles = Editors)]
    public async Task<ActionResult<SurveyDto>> SaveSurvey(int id, [FromBody] SurveyDto req)
        => Ok(await stages.SaveSurveyAsync(id, req, UserName));

    // ── A1 ──────────────────────────────────────────────────────────────────

    [HttpGet("{id:int}/a1")]
    public async Task<ActionResult<A1RegistrationDto>> GetA1(int id)
        => await stages.GetA1Async(id) is { } dto ? Ok(dto) : NotFound();

    [HttpPut("{id:int}/a1")]
    [Authorize(Roles = Editors)]
    public async Task<ActionResult<A1RegistrationDto>> SaveA1(int id, [FromBody] A1RegistrationDto req)
        => Ok(await stages.SaveA1Async(id, req, UserName));

    // ── NOL request ─────────────────────────────────────────────────────────

    [HttpGet("{id:int}/nol-request")]
    public async Task<ActionResult<NolRequestDto>> GetNolRequest(int id)
        => await stages.GetNolRequestAsync(id) is { } dto ? Ok(dto) : NotFound();

    [HttpPut("{id:int}/nol-request")]
    [Authorize(Roles = Editors)]
    public async Task<ActionResult<NolRequestDto>> SaveNolRequest(int id, [FromBody] NolRequestDto req)
        => Ok(await stages.SaveNolRequestAsync(id, req, UserName));
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MasterDataController(StageService stages) : ControllerBase
{
    [HttpGet]
    public async Task<List<MasterDataEntryDto>> GetAll([FromQuery] MasterCategory? category)
        => await stages.GetMasterDataAsync(category);

    [HttpPost]
    [Authorize(Roles = "RegionSales")]
    public async Task<ActionResult<MasterDataEntryDto>> Create([FromBody] SaveMasterDataRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Nama wajib diisi.");
        return Ok(await stages.SaveMasterDataAsync(null, req));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "RegionSales")]
    public async Task<ActionResult<MasterDataEntryDto>> Update(int id, [FromBody] SaveMasterDataRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Nama wajib diisi.");
        return Ok(await stages.SaveMasterDataAsync(id, req));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "RegionSales")]
    public async Task<IActionResult> Delete(int id)
        => await stages.DeleteMasterDataAsync(id) ? Ok() : NotFound();
}

