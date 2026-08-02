using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolLibrary.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolLibrary.Infrastructure.Data.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(user => user.FirstName)
            .HasMaxLength(100)
            .IsRequired();

            builder.Property(user => user.FatherName)
            .HasMaxLength(100)
            .IsRequired();

            builder.Property(user => user.LastName)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasOne<SchoolLibrary.Domain.Entities.GradeLevel>()
                .WithMany()
                .HasForeignKey(user => user.GradeLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<SchoolLibrary.Domain.Entities.SchoolClass>()
                .WithMany()
                .HasForeignKey(user => user.SchoolClassId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
