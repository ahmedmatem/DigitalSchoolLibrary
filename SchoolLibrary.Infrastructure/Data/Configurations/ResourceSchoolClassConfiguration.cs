using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolLibrary.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolLibrary.Infrastructure.Data.Configurations
{
    public class ResourceSchoolClassConfiguration : IEntityTypeConfiguration<ResourceSchoolClass>
    {
        public void Configure(EntityTypeBuilder<ResourceSchoolClass> builder)
        {
            builder.HasKey(resourceSchoolClass => new
            {
                resourceSchoolClass.ResourceId,
                resourceSchoolClass.SchoolClassId
            });

            builder
                .HasOne(resourceSchoolClass =>
                    resourceSchoolClass.Resource)
                .WithMany(resource =>
                    resource.ResourceSchoolClasses)
                .HasForeignKey(resourceSchoolClass =>
                    resourceSchoolClass.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(resourceSchoolClass =>
                    resourceSchoolClass.SchoolClass)
                .WithMany(schoolClass =>
                    schoolClass.ResourceSchoolClasses)
                .HasForeignKey(resourceSchoolClass =>
                    resourceSchoolClass.SchoolClassId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
