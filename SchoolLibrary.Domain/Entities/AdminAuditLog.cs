using SchoolLibrary.Domain.Enums;
namespace SchoolLibrary.Domain.Entities;
public class AdminAuditLog
{
    public Guid Id { get; set; }
    public Guid AdminUserId { get; set; }
    public Guid TargetUserId { get; set; }
    public AdminAuditAction Action { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
