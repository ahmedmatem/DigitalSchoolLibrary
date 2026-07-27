using SchoolLibrary.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolLibrary.Domain.Entities
{
    public class Resource
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? Author { get; set; }

        public ResourceType Type { get; set; }

        public ResourceAudienceType AudienceType { get; set; }

        public string? FilePath { get; set; }

        public string? ExternalUrl { get; set; }

        public string? CoverImagePath { get; set; }

        public Guid SubjectId { get; set; }

        public Subject Subject { get; set; } = null!;

        public Guid CategoryId { get; set; }

        public Category Category { get; set; } = null!;

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }

        public bool IsArchived { get; set; }

        public DateTime? ArchivedAtUtc { get; set; }

        public ICollection<ResourceGradeLevel> ResourceGradeLevels { get; set; }
            = new HashSet<ResourceGradeLevel>();

        public ICollection<ResourceSchoolClass> ResourceSchoolClasses { get; set; }
            = new HashSet<ResourceSchoolClass>();
    }
}
