using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolLibrary.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolLibrary.Infrastructure.Data.Configurations
{
    public class ResourceGradeLevelConfiguration : IEntityTypeConfiguration<ResourceGradeLevel>
    {
        public void Configure(EntityTypeBuilder<ResourceGradeLevel> builder)
        {
            builder.HasKey(resourceGradeLevel => new
            {
                resourceGradeLevel.ResourceId,
                resourceGradeLevel.GradeLevelId
            });

            builder
                .HasOne(resourceGradeLevel => resourceGradeLevel.Resource)
                .WithMany(resource => resource.ResourceGradeLevels)
                .HasForeignKey(resourceGradeLevel =>
                    resourceGradeLevel.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(resourceGradeLevel => resourceGradeLevel.GradeLevel)
                .WithMany(gradeLevel => gradeLevel.ResourceGradeLevels)
                .HasForeignKey(resourceGradeLevel =>
                    resourceGradeLevel.GradeLevelId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
