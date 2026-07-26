using SchoolLibrary.Domain.Enums;

namespace SchoolLibrary.Application.DTOs.ResourceDTOs
{
    public class CreateResourceDto
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? Author { get; set; }

        public ResourceType Type { get; set; }

        public string? FilePath { get; set; }

        public string? ExternalUrl { get; set; }

        public string? CoverImagePath { get; set; }

        public Guid SubjectId { get; set; }

        public Guid CategoryId { get; set; }

        public ResourceAudienceType AudienceType { get; set; }

        public ICollection<int> GradeLevelIds { get; set; }
            = new HashSet<int>();

        public ICollection<Guid> SchoolClassIds { get; set; }
            = new HashSet<Guid>();
    }
}
