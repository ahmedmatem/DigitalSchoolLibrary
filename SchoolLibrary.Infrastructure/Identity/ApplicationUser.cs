using Microsoft.AspNetCore.Identity;

namespace SchoolLibrary.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;

        public string FatherName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public int? GradeLevelId { get; set; }

        public Guid? SchoolClassId { get; set; }
    }
}
