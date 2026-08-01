using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using SchoolLibrary.Application.Common.Exceptions;
using SchoolLibrary.Application.DTOs.FileDtos;
using SchoolLibrary.Application.Enums;
using SchoolLibrary.Application.Interfaces;

namespace SchoolLibrary.Infrastructure.Storage
{
    public sealed class R2FileStorageService : IFileStorageService
    {
        private static readonly HashSet<string> AllowedResourceExtensions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".txt", ".zip"
            };

        private static readonly HashSet<string> AllowedCoverExtensions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg",  ".jpeg", ".png", ".webp"
            };

        private static readonly HashSet<string> AllowedCoverContentTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "image/jpeg", "image/png", "image/webp"
            };

        private readonly IAmazonS3 s3Client;
        private readonly R2StorageOptions options;

        public R2FileStorageService(
            IAmazonS3 s3Client,
            IOptions<R2StorageOptions> options)
        {
            this.s3Client = s3Client;
            this.options = options.Value;
        }

        public Task<PresignedUploadDto> CreateUploadUrlAsync(
            CreateUploadUrlDto model,
            CancellationToken cancellationToken = default)
        {
            ValidateUpload(model);

            var extension = Path
                .GetExtension(model.OriginalFileName)
                .ToLowerInvariant();

            var folder = model.Kind switch
            {
                StoredFileKind.Resource => "resources",
                StoredFileKind.Cover => "covers",
                _ => throw new ValidationException("Невалиден вид на файла.")
            };

            var now = DateTime.UtcNow;

            var storageKey =
                $"{folder}/{now:yyyy/MM}/{Guid.NewGuid():N}{extension}";

            var expiresAtUtc = now.AddMinutes(
                options.UploadUrlExpirationMinutes);

            var request = new GetPreSignedUrlRequest
            {
                BucketName = options.BucketName,
                Key = storageKey,
                Verb = HttpVerb.PUT,
                ContentType = model.ContentType,
                Expires = expiresAtUtc
            };

            var uploadUrl = s3Client.GetPreSignedURL(request);

            return Task.FromResult(new PresignedUploadDto
            {
                UploadUrl = uploadUrl,
                StorageKey = storageKey,
                ContentType = model.ContentType,
                ExpiresAtUtc = expiresAtUtc
            });
        }

        public Task<PresignedDownloadDto> CreateDownloadUrlAsync(
            string storageKey,
            string? downloadFileName = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(storageKey))
            {
                throw new ValidationException("Липсва ключ на файла.");
            }

            var expiresAtUtc = DateTime.UtcNow.AddMinutes(
                options.DownloadUrlExpirationMinutes);

            var request = new GetPreSignedUrlRequest
            {
                BucketName = options.BucketName,
                Key = storageKey,
                Verb = HttpVerb.GET,
                Expires = expiresAtUtc
            };

            if (!string.IsNullOrWhiteSpace(downloadFileName))
            {
                var safeName = downloadFileName.Replace("\"", string.Empty);

                request.ResponseHeaderOverrides =
                    new ResponseHeaderOverrides
                    {
                        ContentDisposition = $"attachment; filename=\"{safeName}\""
                    };
            }

            var downloadUrl = s3Client.GetPreSignedURL(request);

            return Task.FromResult(new PresignedDownloadDto
            {
                DownloadUrl = downloadUrl,
                ExpiresAtUtc = expiresAtUtc
            });
        }

        public async Task<bool> ObjectExistsAsync(
            string storageKey,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await s3Client.GetObjectMetadataAsync(
                    new GetObjectMetadataRequest
                    {
                        BucketName = options.BucketName,
                        Key = storageKey
                    },
                    cancellationToken);

                return true;
            }
            catch (AmazonS3Exception exception)
                when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        public async Task DeleteAsync(
            string storageKey,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(storageKey))
            {
                return;
            }

            await s3Client.DeleteObjectAsync(
                new DeleteObjectRequest
                {
                    BucketName = options.BucketName,
                    Key = storageKey
                },
                cancellationToken);
        }

        private void ValidateUpload(CreateUploadUrlDto model)
        {
            var extension = Path.GetExtension(model.OriginalFileName);

            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new ValidationException("Файлът няма валидно разширение.");
            }

            switch (model.Kind)
            {
                case StoredFileKind.Resource:
                    if (!AllowedResourceExtensions.Contains(extension))
                    {
                        throw new ValidationException("Този формат на учебен ресурс не е разрешен.");
                    }

                    if (model.FileSize >
                        options.MaxResourceFileSizeBytes)
                    {
                        throw new ValidationException("Учебният файл надвишава максималния размер.");
                    }

                    break;

                case StoredFileKind.Cover:
                    if (!AllowedCoverExtensions.Contains(extension) ||
                        !AllowedCoverContentTypes.Contains(
                            model.ContentType))
                    {
                        throw new ValidationException("Корицата трябва да бъде JPG, PNG или WebP.");
                    }

                    if (model.FileSize >
                        options.MaxCoverFileSizeBytes)
                    {
                        throw new ValidationException("Изображението надвишава максималния размер.");
                    }

                    break;

                default:
                    throw new ValidationException("Невалиден вид на файла.");
            }
        }
    }
}
