using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolLibrary.Domain.Entities
{
    public class ResourceGradeLevel
    {
        public Guid ResourceId { get; set; }

        public Resource Resource { get; set; } = null!;

        public int GradeLevelId { get; set; }

        public GradeLevel GradeLevel { get; set; } = null!;
    }
}
