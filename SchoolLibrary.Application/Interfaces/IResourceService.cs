using SchoolLibrary.Application.DTOs.ResourceDTOs;

namespace SchoolLibrary.Application.Interfaces
{
    public interface IResourceService
    {
        Task<IReadOnlyCollection<ResourceListDto>> GetAllAsync(
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

        Task<bool> DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}
