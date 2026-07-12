using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolLibrary.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

using static SchoolLibrary.Domain.Constants;

namespace SchoolLibrary.Infrastructure.Data.Configurations
{
    public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
    {
        public void Configure(EntityTypeBuilder<Resource> builder)
        {
            builder.HasKey(r => r.Id);

            builder
                .Property(r => r.Title)
                .IsRequired()
                .HasMaxLength(TitleMaxLength);

            builder
                .Property(r => r.Description)
                .IsRequired()
                .HasMaxLength(DescriptionMaxLength);

            builder
                .Property(r => r.Author)
                .HasMaxLength(AuthorMaxLength);

            builder
                .Property(r => r.FilePath)
                .HasMaxLength(FilePathMaxLength);

            builder
                .Property(r => r.ExternalUrl)
                .HasMaxLength(2000);

            builder
                .Property(r => r.CoverImagePath)
                .HasMaxLength(1000);

            builder
                .Property(r => r.Type)
                .IsRequired();

            builder
                .Property(r => r.CreatedAtUtc)
                .IsRequired();

            builder
                .HasOne(r => r.Subject)
                .WithMany(s => s.Resources)
                .HasForeignKey(r => r.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(r => r.Category)
                .WithMany(c => c.Resources)
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
