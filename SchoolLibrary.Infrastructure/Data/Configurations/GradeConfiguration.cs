using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolLibrary.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

using static SchoolLibrary.Domain.Constants.GradeConstants;

namespace SchoolLibrary.Infrastructure.Data.Configurations
{
    public class GradeConfiguration : IEntityTypeConfiguration<Grade>
    {
        public void Configure(EntityTypeBuilder<Grade> builder)
        {
            builder.HasKey(grade => grade.Id);

            builder
                .Property(grade => grade.Name)
                .IsRequired()
                .HasMaxLength(NameMaxLength);

            builder
                .HasIndex(grade => grade.Name)
                .IsUnique();
        }
    }
}
