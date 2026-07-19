using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolLibrary.Domain.Entities
{
    public class ResourceSchoolClass
    {
        public Guid ResourceId { get; set; }

        public Resource Resource { get; set; } = null!;

        public Guid SchoolClassId { get; set; }

        public SchoolClass SchoolClass { get; set; } = null!;
    }
}
