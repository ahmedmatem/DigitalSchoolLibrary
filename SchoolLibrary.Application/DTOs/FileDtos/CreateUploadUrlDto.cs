using SchoolLibrary.Application.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchoolLibrary.Application.DTOs.FileDtos
{
    public class CreateUploadUrlDto
    {
        [Required]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        public string ContentType { get; set; } = string.Empty;

        [Range(1, long.MaxValue)]
        public long FileSize { get; set; }

        [EnumDataType(typeof(StoredFileKind))]
        public StoredFileKind Kind { get; set; }
    }
}
