using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simando.Api.Security;
using Simando.Application.Directory;
using Simando.Application.Security;
using Simando.Domain.Security;

namespace Simando.Api.Controllers;

[ApiController]
[Route("api/companies")]
[Authorize]
public sealed class CompanyContactsController(
    ICompanyService companyService,
    IBreakGlassService breakGlassService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("{id:guid}/contacts")]
    [ProducesResponseType<IReadOnlyList<ContactDetail>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetContacts(Guid id, CancellationToken ct)
    {
        var hasViewCapability = currentUser.HasCapability(Capability.ViewCompanyRecords);
        var hasActiveBreakGlass = !hasViewCapability && await breakGlassService.HasActiveAccessAsync(currentUser.UserId, id, ct);

        if (!hasViewCapability && !hasActiveBreakGlass)
        {
            return Forbid();
        }

        var contacts = await companyService.GetContactsAsync(id, hasActiveBreakGlass, ct);
        return Ok(contacts);
    }

    [HttpPost("{id:guid}/contacts")]
    [RequireCapability(Capability.EditStages1To3)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddContact(Guid id, [FromBody] SaveContactRequest request, CancellationToken ct)
    {
        var result = await companyService.AddContactAsync(
            id,
            request,
            currentUser.UserId,
            currentUser.Permissions,
            ct);

        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails { Detail = result.Error });
        }

        return Ok();
    }

    [HttpPut("{id:guid}/contacts/{contactId:guid}")]
    [RequireCapability(Capability.EditStages1To3)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateContact(Guid id, Guid contactId, [FromBody] SaveContactRequest request, CancellationToken ct)
    {
        var result = await companyService.UpdateContactAsync(
            id,
            contactId,
            request,
            currentUser.UserId,
            currentUser.Permissions,
            ct);

        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails { Detail = result.Error });
        }

        return Ok();
    }

    [HttpDelete("{id:guid}/contacts/{contactId:guid}")]
    [RequireCapability(Capability.EditStages1To3)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteContact(Guid id, Guid contactId, CancellationToken ct)
    {
        var result = await companyService.DeleteContactAsync(
            id,
            contactId,
            currentUser.UserId,
            currentUser.Permissions,
            ct);

        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails { Detail = result.Error });
        }

        return Ok();
    }
}
