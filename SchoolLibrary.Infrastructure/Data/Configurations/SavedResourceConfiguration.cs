using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolLibrary.Domain.Entities;
using SchoolLibrary.Infrastructure.Identity;

namespace SchoolLibrary.Infrastructure.Data.Configurations
{
    public class SavedResourceConfiguration : IEntityTypeConfiguration<SavedResource>
    {
        public void Configure(EntityTypeBuilder<SavedResource> builder)
        {
            builder.ToTable("SavedResources");

            builder.HasKey(savedResource => new
            {
                savedResource.UserId,
                savedResource.ResourceId
            });

            builder.Property(savedResource => savedResource.SavedAtUtc)
                .IsRequired();

            builder.HasOne(savedResource => savedResource.Resource)
                .WithMany()
                .HasForeignKey(savedResource => savedResource.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(savedResource => savedResource.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(savedResource => savedResource.UserId);

            builder.HasIndex(savedResource => savedResource.SavedAtUtc);
        }
    }
}
