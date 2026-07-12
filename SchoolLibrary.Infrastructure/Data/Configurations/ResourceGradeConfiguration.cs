using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolLibrary.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolLibrary.Infrastructure.Data.Configurations
{
    public class ResourceGradeConfiguration : IEntityTypeConfiguration<ResourceGrade>
    {
        public void Configure(EntityTypeBuilder<ResourceGrade> builder)
        {
            builder.HasKey(resourceGrade => new
            {
                resourceGrade.ResourceId,
                resourceGrade.GradeId
            });

            builder
                .HasOne(resourceGrade => resourceGrade.Resource)
                .WithMany(resource => resource.ResourceGrades)
                .HasForeignKey(resourceGrade => resourceGrade.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(resourceGrade => resourceGrade.Grade)
                .WithMany(grade => grade.ResourceGrades)
                .HasForeignKey(resourceGrade => resourceGrade.GradeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
