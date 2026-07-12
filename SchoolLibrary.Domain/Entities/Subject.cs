using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;

namespace SchoolLibrary.Domain.Entities
{
    public class Subject
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ICollection<Resource> Resources { get; set; }
            = new HashSet<Resource>();
    }
}
