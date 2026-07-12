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

        public DbSet<Grade> Grades => Set<Grade>();

        public DbSet<ResourceGrade> ResourceGrades => Set<ResourceGrade>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ApplicationDbContext).Assembly);
        }
    }
}
