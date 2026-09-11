using SchoolLibrary.Domain.Enums;

namespace SchoolLibrary.Application.DTOs.ResourceDTOs
{
    public class ModerationResourceDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? Author { get; set; }

        public ResourceType Type { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public ResourceAudienceType AudienceType { get; set; }

        public ResourceModerationStatus ModerationStatus { get; set; }

        public bool HasFile { get; set; }

        public bool HasCover { get; set; }

        public string? ExternalUrl { get; set; }

        public Guid SubmittedByUserId { get; set; }

        public string SubmittedByName { get; set; } = string.Empty;

        public DateTime SubmittedAtUtc { get; set; }

        public Guid? ReviewedByUserId { get; set; }

        public DateTime? ReviewedAtUtc { get; set; }

        public string? RejectionReason { get; set; }
    }
}
