using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolLibrary.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

using static SchoolLibrary.Domain.Constants.SubjectConstants;

namespace SchoolLibrary.Infrastructure.Data.Configurations
{
    public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
    {
        public void Configure(EntityTypeBuilder<Subject> builder)
        {
            builder.HasKey(subject => subject.Id);

            builder
                .Property(subject => subject.Name)
                .IsRequired()
                .HasMaxLength(NameMaxLength);

            builder
                .HasIndex(subject => subject.Name)
                .IsUnique();
        }
    }
}
