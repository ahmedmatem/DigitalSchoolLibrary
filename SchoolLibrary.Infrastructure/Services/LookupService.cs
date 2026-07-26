using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Application.DTOs.LookupDTOs;
using SchoolLibrary.Application.Interfaces;
using SchoolLibrary.Infrastructure.Data;

namespace SchoolLibrary.Infrastructure.Services
{
    public class LookupService : ILookupService
    {
        private readonly ApplicationDbContext dbContext;

        public LookupService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IReadOnlyCollection<SubjectLookupDto>>
            GetSubjectsAsync(
                CancellationToken cancellationToken = default)
        {
            return await dbContext.Subjects
                .AsNoTracking()
                .OrderBy(subject => subject.Name)
                .Select(subject => new SubjectLookupDto
                {
                    Id = subject.Id,
                    Name = subject.Name
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<CategoryLookupDto>>
            GetCategoriesAsync(
                CancellationToken cancellationToken = default)
        {
            return await dbContext.Categories
                .AsNoTracking()
                .OrderBy(category => category.Name)
                .Select(category => new CategoryLookupDto
                {
                    Id = category.Id,
                    Name = category.Name
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<GradeLevelLookupDto>>
            GetGradeLevelsAsync(
                CancellationToken cancellationToken = default)
        {
            return await dbContext.GradeLevels
                .AsNoTracking()
                .OrderBy(gradeLevel => gradeLevel.Number)
                .Select(gradeLevel => new GradeLevelLookupDto
                {
                    Id = gradeLevel.Id,
                    Number = gradeLevel.Number,
                    DisplayName = gradeLevel.Number + ". клас"
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<SchoolClassLookupDto>>
            GetSchoolClassesAsync(
                int? gradeLevelId = null,
                CancellationToken cancellationToken = default)
        {
            var query = dbContext.SchoolClasses
                .AsNoTracking()
                .AsQueryable();

            if (gradeLevelId.HasValue)
            {
                query = query.Where(schoolClass =>
                    schoolClass.GradeLevelId == gradeLevelId.Value);
            }

            return await query
                .OrderBy(schoolClass => schoolClass.GradeLevel.Number)
                .ThenBy(schoolClass => schoolClass.Section)
                .Select(schoolClass => new SchoolClassLookupDto
                {
                    Id = schoolClass.Id,
                    GradeLevelId = schoolClass.GradeLevelId,
                    GradeNumber = schoolClass.GradeLevel.Number,
                    Section = schoolClass.Section,
                    DisplayName =
                        schoolClass.GradeLevel.Number +
                        schoolClass.Section
                })
                .ToListAsync(cancellationToken);
        }
    }
}
