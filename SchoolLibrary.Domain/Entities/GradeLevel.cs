using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolLibrary.Domain.Entities
{
    public class GradeLevel
    {
        public int Id { get; set; }

        public int Number { get; set; }

        public ICollection<SchoolClass> SchoolClasses { get; set; }
            = new HashSet<SchoolClass>();

        public ICollection<ResourceGradeLevel> ResourceGradeLevels { get; set; }
            = new HashSet<ResourceGradeLevel>();
    }
}
