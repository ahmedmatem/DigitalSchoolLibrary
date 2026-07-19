using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolLibrary.Domain.Entities
{
    public class SchoolClass
    {
        public Guid Id { get; set; }

        public int GradeLevelId { get; set; }

        public GradeLevel GradeLevel { get; set; } = null!;

        public string Section { get; set; } = string.Empty;

        public ICollection<ResourceSchoolClass> ResourceSchoolClasses { get; set; }
        = new HashSet<ResourceSchoolClass>();
    }
}
