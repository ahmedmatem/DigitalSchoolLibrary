using SchoolLibrary.Domain.Enums;

namespace SchoolLibrary.Application.DTOs.ResourceDTOs
{
    public class ResourceDetailsDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? Author { get; set; }

        public ResourceType Type { get; set; }

        public string? FilePath { get; set; }

        public string? ExternalUrl { get; set; }

        public string? CoverImagePath { get; set; }

        public Guid SubjectId { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        public Guid CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public ResourceAudienceType AudienceType { get; set; }

        public IReadOnlyCollection<int> GradeLevelIds { get; set; }
            = Array.Empty<int>();

        public IReadOnlyCollection<Guid> SchoolClassIds { get; set; }
            = Array.Empty<Guid>();

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }
    }
}
