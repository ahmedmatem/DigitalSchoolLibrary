using SchoolLibrary.Application.Common.Models;
using SchoolLibrary.Application.DTOs.ResourceDTOs;

namespace SchoolLibrary.Application.Interfaces
{
    public interface IResourceService
    {
        Task<PageResult<ResourceListDto>> GetAllAsync(
            ResourceQueryDto queryModel,
            CancellationToken cancellationToken = default);

        Task<ResourceDetailsDto?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<Guid> CreateAsync(
            CreateResourceDto model,
            CancellationToken cancellationToken = default);

        Task<bool> UpdateAsync(
            Guid id,
            UpdateResourceDto model,
            CancellationToken cancellationToken = default);

        Task<bool> ArchiveAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<bool> RestoreAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}
