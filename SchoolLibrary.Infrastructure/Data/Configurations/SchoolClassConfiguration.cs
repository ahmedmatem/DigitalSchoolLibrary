using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolLibrary.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

using static SchoolLibrary.Domain.Constants.SchoolClassConstants;

namespace SchoolLibrary.Infrastructure.Data.Configurations
{
    public class SchoolClassConfiguration : IEntityTypeConfiguration<SchoolClass>
    {
        public void Configure(EntityTypeBuilder<SchoolClass> builder)
        {
            builder.HasKey(schoolClass => schoolClass.Id);

            builder
                .Property(schoolClass => schoolClass.Section)
                .IsRequired()
                .HasMaxLength(SectionMaxLength);

            builder
                .HasOne(schoolClass => schoolClass.GradeLevel)
                .WithMany(gradeLevel => gradeLevel.SchoolClasses)
                .HasForeignKey(schoolClass => schoolClass.GradeLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasIndex(schoolClass => new
                {
                    schoolClass.GradeLevelId,
                    schoolClass.Section
                })
                .IsUnique();
        }
    }
}
