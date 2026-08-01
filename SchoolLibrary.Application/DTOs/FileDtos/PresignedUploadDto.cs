namespace SchoolLibrary.Application.DTOs.FileDtos
{
    public class PresignedUploadDto
    {
        public string UploadUrl { get; set; } = string.Empty;

        public string StorageKey { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public DateTime ExpiresAtUtc { get; set; }
    }
}
