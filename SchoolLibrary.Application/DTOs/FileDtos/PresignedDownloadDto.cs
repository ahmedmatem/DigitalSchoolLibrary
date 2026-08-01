namespace SchoolLibrary.Application.DTOs.FileDtos
{
    public class PresignedDownloadDto
    {
        public string DownloadUrl { get; set; } = string.Empty;

        public DateTime ExpiresAtUtc { get; set; }
    }
}
