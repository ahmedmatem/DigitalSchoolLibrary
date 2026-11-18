using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Application.Common.Exceptions;
using SchoolLibrary.Application.Common.Models;
using SchoolLibrary.Application.DTOs.AdminUserDtos;
using SchoolLibrary.Application.Interfaces;
using SchoolLibrary.Domain.Constants;
using SchoolLibrary.Domain.Entities;
using SchoolLibrary.Domain.Enums;
using SchoolLibrary.Infrastructure.Data;
using SchoolLibrary.Infrastructure.Identity;
namespace SchoolLibrary.Infrastructure.Services;
public class AdminUserService:IAdminUserService
{
    private readonly ApplicationDbContext db;
    private readonly UserManager<ApplicationUser> users;
    public AdminUserService(ApplicationDbContext db,UserManager<ApplicationUser> users){this.db=db;this.users=users;}

    public async Task<PagedResult<AdminUserListItemDto>> GetUsersAsync(AdminUserQueryDto model,CancellationToken ct=default)
    {
        var q=db.Users.AsNoTracking();
        if(!string.IsNullOrWhiteSpace(model.Search)){var s=model.Search.Trim();q=q.Where(x=>x.FirstName.Contains(s)||x.FatherName.Contains(s)||x.LastName.Contains(s)||(x.Email!=null&&x.Email.Contains(s)));}
        if(model.IsActive.HasValue)q=q.Where(x=>x.IsActive==model.IsActive);
        if(model.GradeLevelId.HasValue)q=q.Where(x=>x.GradeLevelId==model.GradeLevelId);
        if(model.SchoolClassId.HasValue)q=q.Where(x=>x.SchoolClassId==model.SchoolClassId);
        if(!string.IsNullOrWhiteSpace(model.Role)){var role=model.Role.Trim();q=q.Where(x=>db.UserRoles.Any(ur=>ur.UserId==x.Id&&db.Roles.Any(r=>r.Id==ur.RoleId&&r.Name==role)));}
        q=model.SortBy.ToLowerInvariant() switch
        {
            "email"=>model.SortDescending?q.OrderByDescending(x=>x.Email):q.OrderBy(x=>x.Email),
            "status"=>model.SortDescending?q.OrderByDescending(x=>x.IsActive):q.OrderBy(x=>x.IsActive),
            _=>model.SortDescending?q.OrderByDescending(x=>x.FirstName).ThenByDescending(x=>x.LastName):q.OrderBy(x=>x.FirstName).ThenBy(x=>x.LastName)
        };
        var total=await q.CountAsync(ct);
        var page=await q.Skip((model.Page-1)*model.PageSize).Take(model.PageSize).ToListAsync(ct);
        var result=new List<AdminUserListItemDto>();
        foreach(var user in page)result.Add(await ListItem(user,ct));
        return new(){Items=result,Page=model.Page,PageSize=model.PageSize,TotalCount=total};
    }

    public async Task<AdminUserDetailsDto> GetUserAsync(Guid id,CancellationToken ct=default)
    {
        var user=await Required(id);var x=await ListItem(user,ct);
        return new()
        {
            Id=x.Id,FullName=x.FullName,Email=x.Email,Role=x.Role,IsActive=x.IsActive,
            GradeLevelId=x.GradeLevelId,GradeNumber=x.GradeNumber,SchoolClassId=x.SchoolClassId,SchoolClassName=x.SchoolClassName,
            FirstName=user.FirstName,FatherName=user.FatherName,LastName=user.LastName,
            DeactivatedAtUtc=user.DeactivatedAtUtc,DeactivationReason=user.DeactivationReason,
            PromotedToTeacherAtUtc=user.PromotedToTeacherAtUtc,MustChangePassword=user.MustChangePassword,
            CreatedResourcesCount=await db.Resources.CountAsync(r=>r.SubmittedByUserId==id,ct),
            SavedResourcesCount=await db.SavedResources.CountAsync(r=>r.UserId==id,ct),
            AuditLog=await db.AdminAuditLogs.AsNoTracking().Where(a=>a.TargetUserId==id).OrderByDescending(a=>a.CreatedAtUtc).Take(20)
                .Select(a=>new AdminAuditLogDto{Id=a.Id,Action=a.Action.ToString(),Details=a.Details,CreatedAtUtc=a.CreatedAtUtc,AdminUserId=a.AdminUserId}).ToListAsync(ct)
        };
    }

    public async Task<AdminUserDetailsDto> UpdateUserAsync(Guid adminId,Guid id,UpdateAdminUserDto model,CancellationToken ct=default)
    {
        var user=await Required(id);var roles=await users.GetRolesAsync(user);
        if(roles.Contains(RoleConstants.Admin)&&adminId!=id)throw new ValidationException("Друг администраторски профил не може да бъде редактиран.");
        var email=model.Email.Trim();var owner=await users.FindByEmailAsync(email);
        if(owner!=null&&owner.Id!=id)throw new ValidationException("Вече съществува потребител с този имейл адрес.");
        if(roles.Contains(RoleConstants.Student))await ValidateClass(model.GradeLevelId,model.SchoolClassId,ct);
        else if(model.GradeLevelId.HasValue||model.SchoolClassId.HasValue)throw new ValidationException("Клас и паралелка могат да се задават само на ученик.");
        user.FirstName=model.FirstName.Trim();user.FatherName=model.FatherName.Trim();user.LastName=model.LastName.Trim();
        user.GradeLevelId=roles.Contains(RoleConstants.Student)?model.GradeLevelId:null;
        user.SchoolClassId=roles.Contains(RoleConstants.Student)?model.SchoolClassId:null;
        if(!string.Equals(user.Email,email,StringComparison.OrdinalIgnoreCase))
        {
            Check(await users.SetEmailAsync(user,email));Check(await users.SetUserNameAsync(user,email));
        }
        Check(await users.UpdateAsync(user));Audit(adminId,id,AdminAuditAction.UpdateUserProfile,"Променени основни данни.");
        await db.SaveChangesAsync(ct);return await GetUserAsync(id,ct);
    }

    public async Task PromoteToTeacherAsync(Guid adminId,Guid id,CancellationToken ct=default)
    {
        var user=await Required(id);var roles=await users.GetRolesAsync(user);
        if(!roles.Contains(RoleConstants.Student)||roles.Contains(RoleConstants.Teacher)||roles.Contains(RoleConstants.Admin))
            throw new ValidationException("Само потребител с роля Ученик може да бъде повишен до Учител.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        Check(await users.RemoveFromRoleAsync(user,RoleConstants.Student));Check(await users.AddToRoleAsync(user,RoleConstants.Teacher));
        user.GradeLevelId=null;user.SchoolClassId=null;user.PromotedToTeacherAtUtc=DateTimeOffset.UtcNow;user.PromotedToTeacherByUserId=adminId;
        Check(await users.UpdateAsync(user));Audit(adminId,id,AdminAuditAction.PromoteToTeacher,"Student → Teacher.");
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
    }

    public async Task DeactivateAsync(Guid adminId,Guid id,DeactivateUserDto model,CancellationToken ct=default)
    {
        if(adminId==id)throw new ValidationException("Не можете да деактивирате собствения си профил.");
        var user=await Required(id);
        if(await users.IsInRoleAsync(user,RoleConstants.Admin))throw new ValidationException("Администратор не може да бъде деактивиран през приложението.");
        if(!user.IsActive)throw new ValidationException("Потребителят вече е деактивиран.");
        user.IsActive=false;user.DeactivatedAtUtc=DateTimeOffset.UtcNow;user.DeactivatedByUserId=adminId;
        user.DeactivationReason=string.IsNullOrWhiteSpace(model.Reason)?null:model.Reason.Trim();
        Check(await users.UpdateAsync(user));Check(await users.UpdateSecurityStampAsync(user));
        Audit(adminId,id,AdminAuditAction.DeactivateUser,user.DeactivationReason);await db.SaveChangesAsync(ct);
    }

    public async Task ReactivateAsync(Guid adminId,Guid id,CancellationToken ct=default)
    {
        var user=await Required(id);if(user.IsActive)throw new ValidationException("Потребителят вече е активен.");
        user.IsActive=true;user.DeactivatedAtUtc=null;user.DeactivatedByUserId=null;user.DeactivationReason=null;
        Check(await users.UpdateAsync(user));Audit(adminId,id,AdminAuditAction.ReactivateUser,null);await db.SaveChangesAsync(ct);
    }

    public async Task ResetPasswordAsync(Guid adminId,Guid id,AdminResetPasswordDto model,CancellationToken ct=default)
    {
        var user=await Required(id);
        if(await users.IsInRoleAsync(user,RoleConstants.Admin)&&adminId!=id)throw new ValidationException("Паролата на друг администратор не може да бъде нулирана.");
        var token=await users.GeneratePasswordResetTokenAsync(user);Check(await users.ResetPasswordAsync(user,token,model.TemporaryPassword));
        user.MustChangePassword=true;Check(await users.UpdateAsync(user));Check(await users.UpdateSecurityStampAsync(user));
        Audit(adminId,id,AdminAuditAction.ResetPassword,"Зададена временна парола.");await db.SaveChangesAsync(ct);
    }

    private async Task<ApplicationUser> Required(Guid id)=>await users.FindByIdAsync(id.ToString())??throw new NotFoundException("Потребителят не е намерен.");
    private async Task<AdminUserListItemDto> ListItem(ApplicationUser u,CancellationToken ct)
    {
        var roles=await users.GetRolesAsync(u);
        var grade=u.GradeLevelId.HasValue?await db.GradeLevels.Where(x=>x.Id==u.GradeLevelId).Select(x=>(int?)x.Number).FirstOrDefaultAsync(ct):null;
        var cls=u.SchoolClassId.HasValue?await db.SchoolClasses.Where(x=>x.Id==u.SchoolClassId).Select(x=>x.GradeLevel.Number+x.Section).FirstOrDefaultAsync(ct):null;
        return new(){Id=u.Id,FullName=$"{u.FirstName} {u.FatherName} {u.LastName}",Email=u.Email??"",Role=roles.FirstOrDefault()??"",IsActive=u.IsActive,GradeLevelId=u.GradeLevelId,GradeNumber=grade,SchoolClassId=u.SchoolClassId,SchoolClassName=cls};
    }
    private async Task ValidateClass(int? grade,Guid? cls,CancellationToken ct)
    {
        if(!grade.HasValue&&!cls.HasValue)return;
        if(!grade.HasValue||!cls.HasValue)throw new ValidationException("Трябва да бъдат избрани както клас, така и паралелка.");
        if(!await db.SchoolClasses.AnyAsync(x=>x.Id==cls&&x.GradeLevelId==grade,ct))throw new ValidationException("Избраната паралелка не принадлежи към избрания клас.");
    }
    private void Audit(Guid admin,Guid target,AdminAuditAction action,string? details)=>db.AdminAuditLogs.Add(new AdminAuditLog{Id=Guid.NewGuid(),AdminUserId=admin,TargetUserId=target,Action=action,Details=details,CreatedAtUtc=DateTime.UtcNow});
    private static void Check(IdentityResult r){if(!r.Succeeded)throw new ValidationException("Операцията не беше успешна.",r.Errors.GroupBy(x=>x.Code).ToDictionary(x=>x.Key,x=>x.Select(e=>e.Description).ToArray()));}
}
