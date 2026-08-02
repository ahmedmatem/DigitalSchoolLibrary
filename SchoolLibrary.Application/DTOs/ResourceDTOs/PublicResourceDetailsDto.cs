using SchoolLibrary.Domain.Enums;

namespace SchoolLibrary.Application.DTOs.ResourceDTOs
{
    public class PublicResourceDetailsDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? Author { get; set; }

        public ResourceType Type { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public ResourceAudienceType AudienceType { get; set; }

        public bool HasCover { get; set; }

        public bool RequiresAuthentication { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; }
    }
}
