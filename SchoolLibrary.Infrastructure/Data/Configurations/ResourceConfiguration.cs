using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolLibrary.Domain.Entities;
using SchoolLibrary.Domain.Enums;
using SchoolLibrary.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Text;

using static SchoolLibrary.Domain.Constants.ResourceConstants;

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

            builder.Property(resource => resource.IsPubliclyVisible)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(resource => resource.FileStorageKey)
                .HasMaxLength(500);

            builder.Property(resource => resource.OriginalFileName)
                .HasMaxLength(255);

            builder.Property(resource => resource.FileContentType)
                .HasMaxLength(150);

            builder.Property(resource => resource.CoverStorageKey)
                .HasMaxLength(500);

            builder
                .Property(r => r.ExternalUrl)
                .HasMaxLength(2000);

            builder
                .Property(r => r.Type)
                .IsRequired();

            builder
                .Property(r => r.CreatedAtUtc)
                .IsRequired();

            builder
                .Property(resource => resource.IsArchived)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasIndex(resource => resource.IsArchived);

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

            builder.Property(resource => resource.ModerationStatus)
                .IsRequired();

            builder.Property(resource => resource.SubmittedByUserId)
                .IsRequired();

            builder.Property(resource => resource.SubmittedAtUtc)
                .IsRequired();

            builder.Property(resource => resource.RejectionReason)
                .HasMaxLength(1000);

            builder.HasIndex(resource => resource.ModerationStatus);

            builder.HasIndex(resource => resource.SubmittedByUserId);

            builder.HasIndex(resource => resource.ReviewedByUserId);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(resource => resource.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(resource => resource.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
