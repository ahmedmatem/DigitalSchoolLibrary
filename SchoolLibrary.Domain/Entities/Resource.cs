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

        public string? FilePath { get; set; }

        public string? ExternalUrl { get; set; }

        public string? CoverImagePath { get; set; }

        public Guid SubjectId { get; set; }

        public Subject Subject { get; set; } = null!;

        public Guid CategoryId { get; set; }

        public Category Category { get; set; } = null!;

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? UpdateAtUtc { get; set; }

        public bool IsArchived { get; set; }

        public ICollection<ResourceGrade> ResourceGrades { get; set; }
            = new HashSet<ResourceGrade>();
    }
}
