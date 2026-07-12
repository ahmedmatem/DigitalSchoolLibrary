using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolLibrary.Domain.Entities
{
    public class Grade
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ICollection<ResourceGrade> ResourceGrades { get; set; }
            = new HashSet<ResourceGrade>();
    }
}
