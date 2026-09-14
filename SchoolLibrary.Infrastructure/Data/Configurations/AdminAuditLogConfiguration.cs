using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolLibrary.Domain.Entities;
using SchoolLibrary.Infrastructure.Identity;
namespace SchoolLibrary.Infrastructure.Data.Configurations;
public class AdminAuditLogConfiguration:IEntityTypeConfiguration<AdminAuditLog>
{
    public void Configure(EntityTypeBuilder<AdminAuditLog> builder)
    {
        builder.HasKey(x=>x.Id);
        builder.Property(x=>x.Details).HasMaxLength(1000);
        builder.HasIndex(x=>new{x.TargetUserId,x.CreatedAtUtc});
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x=>x.AdminUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x=>x.TargetUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
