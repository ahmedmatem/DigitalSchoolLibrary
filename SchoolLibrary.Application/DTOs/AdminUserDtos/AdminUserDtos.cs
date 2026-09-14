using System.ComponentModel.DataAnnotations;
namespace SchoolLibrary.Application.DTOs.AdminUserDtos;
public class AdminUserQueryDto
{
    public string? Search { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public int? GradeLevelId { get; set; }
    public Guid? SchoolClassId { get; set; }
    public string SortBy { get; set; } = "name";
    public bool SortDescending { get; set; }
    [Range(1,int.MaxValue)] public int Page { get; set; }=1;
    [Range(1,100)] public int PageSize { get; set; }=20;
}
public class AdminUserListItemDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; }=string.Empty;
    public string Email { get; set; }=string.Empty;
    public string Role { get; set; }=string.Empty;
    public bool IsActive { get; set; }
    public int? GradeLevelId { get; set; }
    public int? GradeNumber { get; set; }
    public Guid? SchoolClassId { get; set; }
    public string? SchoolClassName { get; set; }
}
public class AdminUserDetailsDto:AdminUserListItemDto
{
    public string FirstName { get; set; }=string.Empty;
    public string FatherName { get; set; }=string.Empty;
    public string LastName { get; set; }=string.Empty;
    public DateTimeOffset? DeactivatedAtUtc { get; set; }
    public string? DeactivationReason { get; set; }
    public DateTimeOffset? PromotedToTeacherAtUtc { get; set; }
    public bool MustChangePassword { get; set; }
    public int CreatedResourcesCount { get; set; }
    public int SavedResourcesCount { get; set; }
    public IReadOnlyCollection<AdminAuditLogDto> AuditLog { get; set; }=[];
}
public class UpdateAdminUserDto
{
    [Required,StringLength(100)] public string FirstName { get; set; }=string.Empty;
    [Required,StringLength(100)] public string FatherName { get; set; }=string.Empty;
    [Required,StringLength(100)] public string LastName { get; set; }=string.Empty;
    [Required,EmailAddress,StringLength(256)] public string Email { get; set; }=string.Empty;
    public int? GradeLevelId { get; set; }
    public Guid? SchoolClassId { get; set; }
}
public class DeactivateUserDto { [StringLength(500)] public string? Reason { get; set; } }
public class AdminResetPasswordDto { [Required,MinLength(6)] public string TemporaryPassword { get; set; }=string.Empty; }
public class AdminAuditLogDto
{
    public Guid Id { get; set; }
    public string Action { get; set; }=string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid AdminUserId { get; set; }
}
