using SchoolLibrary.Application.Common.Models;
using SchoolLibrary.Application.DTOs.AdminUserDtos;
namespace SchoolLibrary.Application.Interfaces;
public interface IAdminUserService
{
    Task<PagedResult<AdminUserListItemDto>> GetUsersAsync(AdminUserQueryDto query,CancellationToken cancellationToken=default);
    Task<AdminUserDetailsDto> GetUserAsync(Guid userId,CancellationToken cancellationToken=default);
    Task<AdminUserDetailsDto> UpdateUserAsync(Guid adminUserId,Guid userId,UpdateAdminUserDto model,CancellationToken cancellationToken=default);
    Task PromoteToTeacherAsync(Guid adminUserId,Guid userId,CancellationToken cancellationToken=default);
    Task DeactivateAsync(Guid adminUserId,Guid userId,DeactivateUserDto model,CancellationToken cancellationToken=default);
    Task ReactivateAsync(Guid adminUserId,Guid userId,CancellationToken cancellationToken=default);
    Task ResetPasswordAsync(Guid adminUserId,Guid userId,AdminResetPasswordDto model,CancellationToken cancellationToken=default);
}
