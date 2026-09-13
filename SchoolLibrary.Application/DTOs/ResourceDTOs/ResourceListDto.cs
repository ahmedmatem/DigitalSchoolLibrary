using SchoolLibrary.Domain.Enums;

namespace SchoolLibrary.Application.DTOs.ResourceDTOs
{
    public class ResourceListDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Author { get; set; }

        public ResourceCollectionType CollectionType { get; set; }

        public ResourceType Type { get; set; }

        public string? SubjectName { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string? CoverStorageKey { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}
