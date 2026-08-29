using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simando.Application.Security;
using Simando.Domain.Security;

namespace Simando.Api.Controllers;

public sealed record CreateUserRequest(
    string FullName,
    string Username,
    string? Email,
    Role Role,
    Guid? AreaId = null,
    Guid? RegionId = null
);

public sealed record CreateUserResponse(
    Guid UserId,
    string Username,
    string TemporaryPassword
);

public sealed record AddRoleAssignmentRequest(
    Role Role,
    Guid? AreaId = null,
    Guid? RegionId = null
);

public sealed record ResetPasswordResponse(
    string TemporaryPassword
);

public sealed record SetUserStatusRequest(
    bool Active
);

public sealed record UserListItemDto(
    Guid Id,
    string FullName,
    string Username,
    string? Email,
    bool Active,
    DateTimeOffset? LastLoginAt,
    IReadOnlyList<RoleAssignmentDto> Roles,
    IReadOnlyList<Guid> AssignmentIds
);

public sealed record RoleAssignmentDto(
    Role Role,
    string ScopeLabel
);

[ApiController]
[Route("api/admin/users")]
[Authorize]
public sealed class UsersAdminController(
    IUserService userService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<UserListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsers(CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();

        if (!currentUser.Permissions.HasCapability(Capability.AssignRoles) && currentUser.Scope != AccessScope.All)
        {
            return Forbid();
        }

        var users = await userService.GetUsersAsync(currentUser.Permissions, ct);
        var dtos = users.Select(u => new UserListItemDto(
            u.Id,
            u.FullName,
            u.UserName,
            u.Email,
            u.Active,
            u.LastLoginAt,
            u.Roles.Select(r => new RoleAssignmentDto(r.Role, r.ScopeLabel)).ToList(),
            u.RoleAssignmentIds
        )).ToList();

        return Ok(dtos);
    }

    [HttpPost]
    [ProducesResponseType<CreateUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();

        if (!currentUser.Permissions.HasCapability(Capability.AssignRoles))
        {
            return Forbid();
        }

        var result = await userService.CreateUserAsync(
            request.FullName,
            request.Username,
            request.Email,
            request.Role,
            request.AreaId,
            request.RegionId,
            currentUser.UserId,
            currentUser.Permissions,
            ct);

        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(new CreateUserResponse(result.UserId, request.Username, result.TemporaryPassword!));
    }

    [HttpPost("{id:guid}/roles")]
    [ProducesResponseType<RoleAssignmentResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddRole(Guid id, [FromBody] AddRoleAssignmentRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();

        if (!currentUser.Permissions.HasCapability(Capability.AssignRoles))
        {
            return Forbid();
        }

        var result = await userService.AddRoleAssignmentAsync(
            id,
            request.Role,
            request.AreaId,
            request.RegionId,
            currentUser.UserId,
            currentUser.Permissions,
            ct);

        if (!result.Succeeded)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}/roles/{assignmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeactivateRole(Guid id, Guid assignmentId, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();

        if (!currentUser.Permissions.HasCapability(Capability.AssignRoles))
        {
            return Forbid();
        }

        await userService.DeactivateRoleAssignmentAsync(assignmentId, currentUser.UserId, id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reset-password")]
    [ProducesResponseType<ResetPasswordResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResetPassword(Guid id, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();

        if (!currentUser.Permissions.HasCapability(Capability.AssignRoles))
        {
            return Forbid();
        }

        var newTempPassword = await userService.ResetPasswordAsync(id, ct);
        return Ok(new ResetPasswordResponse(newTempPassword));
    }

    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SetStatus(Guid id, [FromBody] SetUserStatusRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();

        if (!currentUser.Permissions.HasCapability(Capability.AssignRoles))
        {
            return Forbid();
        }

        await userService.SetUserActiveAsync(id, request.Active, ct);
        return NoContent();
    }
}
