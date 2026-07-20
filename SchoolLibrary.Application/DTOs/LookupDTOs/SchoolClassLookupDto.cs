namespace SchoolLibrary.Application.DTOs.LookupDTOs
{
    public class SchoolClassLookupDto
    {
        public Guid Id { get; set; }

        public int GradeLevelId { get; set; }

        public int GradeNumber { get; set; }

        public string Section { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;
    }
}
