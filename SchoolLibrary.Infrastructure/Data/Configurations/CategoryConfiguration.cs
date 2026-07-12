using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolLibrary.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

using static SchoolLibrary.Domain.Constants.CategoryConstants;

namespace SchoolLibrary.Infrastructure.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(category => category.Id);

            builder
                .Property(category => category.Name)
                .IsRequired()
                .HasMaxLength(NameMaxLength);

            builder
                .HasIndex(category => category.Name)
                .IsUnique();
        }
    }
}
