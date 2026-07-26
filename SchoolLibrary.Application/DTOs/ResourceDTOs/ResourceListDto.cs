using SchoolLibrary.Domain.Enums;

namespace SchoolLibrary.Application.DTOs.ResourceDTOs
{
    public class ResourceListDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Author { get; set; }

        public ResourceType Type { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string? CoverImagePath { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}
