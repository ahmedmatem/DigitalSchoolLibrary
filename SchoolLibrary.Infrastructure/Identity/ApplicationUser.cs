using Microsoft.AspNetCore.Identity;
namespace SchoolLibrary.Infrastructure.Identity;
public class ApplicationUser:IdentityUser<Guid>
{
    public string FirstName { get; set; }=string.Empty;
    public string FatherName { get; set; }=string.Empty;
    public string LastName { get; set; }=string.Empty;
    public int? GradeLevelId { get; set; }
    public Guid? SchoolClassId { get; set; }
    public bool IsActive { get; set; }=true;
    public bool MustChangePassword { get; set; }
    public DateTimeOffset? DeactivatedAtUtc { get; set; }
    public Guid? DeactivatedByUserId { get; set; }
    public string? DeactivationReason { get; set; }
    public DateTimeOffset? PromotedToTeacherAtUtc { get; set; }
    public Guid? PromotedToTeacherByUserId { get; set; }
}
