using SchoolLibrary.Application.Common.Models;
using SchoolLibrary.Application.DTOs.ResourceDTOs;

namespace SchoolLibrary.Application.Interfaces
{
    public interface ISavedResourceService
    {
        Task<PagedResult<SavedResourceDto>> GetMineAsync(
            ResourceQueryDto query,
            CancellationToken cancellationToken = default);

        Task<bool> SaveAsync(
            Guid resourceId,
            CancellationToken cancellationToken = default);

        Task<bool> RemoveAsync(
            Guid resourceId,
            CancellationToken cancellationToken = default);
    }
}
