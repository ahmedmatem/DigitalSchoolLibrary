using SchoolLibrary.Application.Common.Models;
using SchoolLibrary.Application.DTOs.FileDtos;
using SchoolLibrary.Application.DTOs.ResourceDTOs;

namespace SchoolLibrary.Application.Interfaces
{
    public interface IResourceService
    {
        // =========================================================
        // PUBLIC CATALOG
        // =========================================================

        Task<PagedResult<PublicResourceListDto>> GetPublicCatalogAsync(
            ResourceQueryDto query,
            CancellationToken cancellationToken = default);

        Task<PublicResourceDetailsDto?> GetPublicDetailsAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<PresignedDownloadDto?> CreatePublicCoverUrlAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        // =========================================================
        // PERSONALIZED CATALOG
        // =========================================================

        Task<PagedResult<PersonalResourceListDto>> GetForCurrentUserAsync(
            ResourceQueryDto query,
            CancellationToken cancellationToken = default);

        // =========================================================
        // PROTECTED RESOURCE ACCESS
        // =========================================================

        Task<ResourceOpenDto?> GetOpenUrlAsync(
            Guid id,
            CancellationToken cancellationToken);

        // =========================================================
        // RESOURCE MODERATION
        // =========================================================

        Task<PagedResult<ModerationResourceDto>> GetPendingAsync(
            ResourceQueryDto query,
            CancellationToken cancellationToken = default);

        Task<PagedResult<ModerationResourceDto>> GetMineAsync(
            ResourceQueryDto query,
            CancellationToken cancellationToken = default);

        Task<bool> ApproveAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<bool> RejectAsync(
            Guid id,
            RejectResourceDto model,
            CancellationToken cancellationToken = default);

        Task<bool> ResubmitAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        // =========================================================
        // RESOURCE MANAGEMENT
        // =========================================================

        Task<PagedResult<ResourceListDto>> GetAllAsync(
            ResourceQueryDto query,
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

        Task<PresignedDownloadDto?> CreateModerationDownloadUrlAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<PresignedDownloadDto?> CreateModerationCoverUrlAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        // =========================================================

        Task<MyResourcesSummaryDto> GetMineSummaryAsync(CancellationToken cancellationToken = default);
    }
}
