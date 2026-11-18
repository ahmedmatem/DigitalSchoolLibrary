using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolLibrary.Application.Common.Models;
using SchoolLibrary.Application.DTOs.AdminUserDtos;
using SchoolLibrary.Application.Interfaces;
using SchoolLibrary.Domain.Constants;
using System.Security.Claims;
namespace SchoolLibrary.Api.Controllers;

[Route("api/admin/users"), ApiController, Authorize(Roles = RoleConstants.Admin)]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService service;

    public AdminUsersController(IAdminUserService service) => this.service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminUserListItemDto>>>
        GetUsers([FromQuery] AdminUserQueryDto q, CancellationToken ct) => Ok(await service.GetUsersAsync(q, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminUserDetailsDto>> GetUser(Guid id, CancellationToken ct)
        => Ok(await service.GetUserAsync(id, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminUserDetailsDto>>
        Update(Guid id, UpdateAdminUserDto m, CancellationToken ct)
        => Ok(await service.UpdateUserAsync(CurrentId(), id, m, ct));

    [HttpPost("{id:guid}/promote-to-teacher")]
    public async Task<IActionResult> Promote(Guid id, CancellationToken ct)
    {
        await service.PromoteToTeacherAsync(CurrentId(), id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, DeactivateUserDto m, CancellationToken ct)
    {
        await service.DeactivateAsync(CurrentId(), id, m, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
    {
        await service.ReactivateAsync(CurrentId(), id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> Reset(Guid id, AdminResetPasswordDto m, CancellationToken ct)
    {
        await service.ResetPasswordAsync(CurrentId(), id, m, ct);
        return NoContent();
    }

    private Guid CurrentId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : throw new UnauthorizedAccessException();
}
