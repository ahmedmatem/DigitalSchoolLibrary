using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolLibrary.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolLibrary.Infrastructure.Data.Configurations
{
    public class GradeLevelConfiguration : IEntityTypeConfiguration<GradeLevel>
    {
        public void Configure(EntityTypeBuilder<GradeLevel> builder)
        {
            builder.HasKey(gradeLevel => gradeLevel.Id);

            builder
                .Property(gradeLevel => gradeLevel.Number)
                .IsRequired();

            builder
                .HasIndex(gradeLevel => gradeLevel.Number)
                .IsUnique();

            builder.ToTable(tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_GradeLevels_Number",
                    "[Number] BETWEEN 5 AND 12");
            });
        }
    }
}
