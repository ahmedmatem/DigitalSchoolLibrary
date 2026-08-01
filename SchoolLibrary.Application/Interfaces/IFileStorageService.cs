using SchoolLibrary.Application.DTOs.FileDtos;

namespace SchoolLibrary.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<PresignedUploadDto> CreateUploadUrlAsync(
            CreateUploadUrlDto model,
            CancellationToken cancellationToken = default);

        Task<PresignedDownloadDto> CreateDownloadUrlAsync(
            string storageKey,
            string? downloadFileName = null,
            CancellationToken cancellationToken = default);

        Task<bool> ObjectExistsAsync(
            string storageKey,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            string storageKey,
            CancellationToken cancellationToken = default);
    }
}
