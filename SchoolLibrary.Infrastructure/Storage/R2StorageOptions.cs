namespace SchoolLibrary.Infrastructure.Storage
{
    public sealed class R2StorageOptions
    {
        public const string SectionName = "R2Storage";

        public string AccountId { get; set; } = string.Empty;

        public string BucketName { get; set; } = string.Empty;

        public string AccessKeyId { get; set; } = string.Empty;

        public string SecretAccessKey { get; set; } = string.Empty;

        public int UploadUrlExpirationMinutes { get; set; } = 10;

        public int DownloadUrlExpirationMinutes { get; set; } = 5;

        public long MaxResourceFileSizeBytes { get; set; } = 50 * 1024 * 1024;

        public long MaxCoverFileSizeBytes { get; set; }  = 5 * 1024 * 1024;
    }
}
