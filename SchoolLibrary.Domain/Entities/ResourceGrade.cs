using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolLibrary.Domain.Entities
{
    public class ResourceGrade
    {
        public Guid ResourceId { get; set; }

        public Resource Resource { get; set; } = null!;

        public int GradeId { get; set; }

        public Grade Grade { get; set; } = null!;
    }
}
