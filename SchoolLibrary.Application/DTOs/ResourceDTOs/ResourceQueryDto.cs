using SchoolLibrary.Domain.Enums;

namespace SchoolLibrary.Application.DTOs.ResourceDTOs
{
    public class ResourceQueryDto
    {
        private const int DefaultPageSize = 12;
        private const int MaxPageSize = 50;

        private int page = 1;
        private int pageSize = DefaultPageSize;

        public string? Search { get; set; }

        public Guid? SubjectId { get; set; }

        public Guid? CategoryId { get; set; }

        public int? GradeLevelId { get; set; }

        public Guid? SchoolClassId { get; set; }

        public ResourceType? Type { get; set; }

        public ResourceAudienceType? AudienceType { get; set; }

        public int Page
        {
            get => page;
            set => page = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => pageSize;
            set
            {
                if (value < 1)
                {
                    pageSize = DefaultPageSize;
                }
                else
                {
                    pageSize = Math.Min(value, MaxPageSize);
                }
            }
        }
    }
}
