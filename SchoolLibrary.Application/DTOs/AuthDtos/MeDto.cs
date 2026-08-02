namespace SchoolLibrary.Application.DTOs.AuthDtos
{
    public class MeDto
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string FatherName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public IReadOnlyCollection<string> Roles { get; set; } = [];

        public int? GradeLevelId { get; set; }

        public int? GradeNumber { get; set; }

        public Guid? SchoolClassId { get; set; }

        public string? SchoolClassName { get; set; }
    }
}
