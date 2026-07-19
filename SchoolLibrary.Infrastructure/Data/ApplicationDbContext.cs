using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolLibrary.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Resource> Resources => Set<Resource>();

        public DbSet<Subject> Subjects => Set<Subject>();

        public DbSet<Category> Categories => Set<Category>();

        public DbSet<GradeLevel> GradeLevels => Set<GradeLevel>();

        public DbSet<SchoolClass> SchoolClasses => Set<SchoolClass>();

        public DbSet<ResourceGradeLevel> ResourceGradeLevels => Set<ResourceGradeLevel>();

        public DbSet<ResourceSchoolClass> ResourceSchoolClasses => Set<ResourceSchoolClass>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ApplicationDbContext).Assembly);
        }
    }
}
