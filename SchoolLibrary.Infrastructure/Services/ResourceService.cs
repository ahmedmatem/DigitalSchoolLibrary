using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Application.DTOs.ResourceDTOs;
using SchoolLibrary.Application.Interfaces;
using SchoolLibrary.Domain.Entities;
using SchoolLibrary.Domain.Enums;
using SchoolLibrary.Infrastructure.Data;

namespace SchoolLibrary.Infrastructure.Services
{
    public class ResourceService : IResourceService
    {
        private readonly ApplicationDbContext dbContext;

        public ResourceService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IReadOnlyCollection<ResourceListDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await dbContext.Resources
                .AsNoTracking()
                .OrderByDescending(resource => resource.CreatedAtUtc)
                .Select(resource => new ResourceListDto
                {
                    Id = resource.Id,
                    Title = resource.Title,
                    Author = resource.Author,
                    Type = resource.Type,
                    SubjectName = resource.Subject.Name,
                    CategoryName = resource.Category.Name,
                    CoverImagePath = resource.CoverImagePath,
                    CreatedAtUtc = resource.CreatedAtUtc
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<ResourceDetailsDto?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await dbContext.Resources
                .AsNoTracking()
                .Where(resource => resource.Id == id)
                .Select(resource => new ResourceDetailsDto
                {
                    Id = resource.Id,
                    Title = resource.Title,
                    Description = resource.Description,
                    Author = resource.Author,
                    Type = resource.Type,
                    FilePath = resource.FilePath,
                    ExternalUrl = resource.ExternalUrl,
                    CoverImagePath = resource.CoverImagePath,
                    SubjectId = resource.SubjectId,
                    SubjectName = resource.Subject.Name,
                    CategoryId = resource.CategoryId,
                    CategoryName = resource.Category.Name,
                    AudienceType = resource.AudienceType,
                    GradeLevelIds = resource.ResourceGradeLevels
                        .Select(item => item.GradeLevelId)
                        .ToArray(),
                    SchoolClassIds = resource.ResourceSchoolClasses
                        .Select(item => item.SchoolClassId)
                        .ToArray(),
                    CreatedAtUtc = resource.CreatedAtUtc,
                    UpdatedAtUtc = resource.UpdatedAtUtc
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Guid> CreateAsync(
            CreateResourceDto model,
            CancellationToken cancellationToken = default)
        {
            await ValidateReferencesAsync(
                model.SubjectId,
                model.CategoryId,
                model.AudienceType,
                model.GradeLevelIds,
                model.SchoolClassIds,
                cancellationToken);

            var resource = new Resource
            {
                Id = Guid.NewGuid(),
                Title = model.Title.Trim(),
                Description = model.Description.Trim(),
                Author = model.Author?.Trim(),
                Type = model.Type,
                FilePath = model.FilePath?.Trim(),
                ExternalUrl = model.ExternalUrl?.Trim(),
                CoverImagePath = model.CoverImagePath?.Trim(),
                SubjectId = model.SubjectId,
                CategoryId = model.CategoryId,
                AudienceType = model.AudienceType,
                CreatedAtUtc = DateTime.UtcNow
            };

            AddAudienceRelations(
                resource,
                model.AudienceType,
                model.GradeLevelIds,
                model.SchoolClassIds);

            dbContext.Resources.Add(resource);

            await dbContext.SaveChangesAsync(cancellationToken);

            return resource.Id;
        }

        public async Task<bool> UpdateAsync(
            Guid id,
            UpdateResourceDto model,
            CancellationToken cancellationToken = default)
        {
            var resource = await dbContext.Resources
                .Include(item => item.ResourceGradeLevels)
                .Include(item => item.ResourceSchoolClasses)
                .FirstOrDefaultAsync(
                    item => item.Id == id,
                    cancellationToken);

            if (resource is null)
            {
                return false;
            }

            await ValidateReferencesAsync(
                model.SubjectId,
                model.CategoryId,
                model.AudienceType,
                model.GradeLevelIds,
                model.SchoolClassIds,
                cancellationToken);

            resource.Title = model.Title.Trim();
            resource.Description = model.Description.Trim();
            resource.Author = model.Author?.Trim();
            resource.Type = model.Type;
            resource.FilePath = model.FilePath?.Trim();
            resource.ExternalUrl = model.ExternalUrl?.Trim();
            resource.CoverImagePath = model.CoverImagePath?.Trim();
            resource.SubjectId = model.SubjectId;
            resource.CategoryId = model.CategoryId;
            resource.AudienceType = model.AudienceType;
            resource.UpdatedAtUtc = DateTime.UtcNow;

            resource.ResourceGradeLevels.Clear();
            resource.ResourceSchoolClasses.Clear();

            AddAudienceRelations(
                resource,
                model.AudienceType,
                model.GradeLevelIds,
                model.SchoolClassIds);

            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var resource = await dbContext.Resources
                .FirstOrDefaultAsync(
                    item => item.Id == id,
                    cancellationToken);

            if (resource is null)
            {
                return false;
            }

            dbContext.Resources.Remove(resource);

            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        private async Task ValidateReferencesAsync(
            Guid subjectId,
            Guid categoryId,
            ResourceAudienceType audienceType,
            ICollection<int> gradeLevelIds,
            ICollection<Guid> schoolClassIds,
            CancellationToken cancellationToken)
        {
            var subjectExists = await dbContext.Subjects
                .AnyAsync(
                    subject => subject.Id == subjectId,
                    cancellationToken);

            if (!subjectExists)
            {
                throw new ArgumentException("The selected subject does not exist.");
            }

            var categoryExists = await dbContext.Categories
                .AnyAsync(
                    category => category.Id == categoryId,
                    cancellationToken);

            if (!categoryExists)
            {
                throw new ArgumentException("The selected category does not exist.");
            }

            switch (audienceType)
            {
                case ResourceAudienceType.AllStudents:
                    if (gradeLevelIds.Count > 0 || schoolClassIds.Count > 0)
                    {
                        throw new ArgumentException(
                            "Audience collections must be empty for AllStudents.");
                    }

                    break;

                case ResourceAudienceType.GradeLevels:
                    if (gradeLevelIds.Count == 0)
                    {
                        throw new ArgumentException(
                            "At least one grade level must be selected.");
                    }

                    if (schoolClassIds.Count > 0)
                    {
                        throw new ArgumentException(
                            "School classes cannot be selected for GradeLevels.");
                    }

                    var existingGradeCount = await dbContext.GradeLevels
                        .CountAsync(
                            gradeLevel =>
                                gradeLevelIds.Contains(gradeLevel.Id),
                            cancellationToken);

                    if (existingGradeCount != gradeLevelIds.Distinct().Count())
                    {
                        throw new ArgumentException(
                            "One or more grade levels do not exist.");
                    }

                    break;

                case ResourceAudienceType.SchoolClasses:
                    if (schoolClassIds.Count == 0)
                    {
                        throw new ArgumentException(
                            "At least one school class must be selected.");
                    }

                    if (gradeLevelIds.Count > 0)
                    {
                        throw new ArgumentException(
                            "Grade levels cannot be selected for SchoolClasses.");
                    }

                    var existingClassCount = await dbContext.SchoolClasses
                        .CountAsync(
                            schoolClass =>
                                schoolClassIds.Contains(schoolClass.Id),
                            cancellationToken);

                    if (existingClassCount != schoolClassIds.Distinct().Count())
                    {
                        throw new ArgumentException(
                            "One or more school classes do not exist.");
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(audienceType),
                        "Unsupported audience type.");
            }
        }

        private static void AddAudienceRelations(
            Resource resource,
            ResourceAudienceType audienceType,
            ICollection<int> gradeLevelIds,
            ICollection<Guid> schoolClassIds)
        {
            if (audienceType == ResourceAudienceType.GradeLevels)
            {
                foreach (var gradeLevelId in gradeLevelIds.Distinct())
                {
                    resource.ResourceGradeLevels.Add(
                        new ResourceGradeLevel
                        {
                            GradeLevelId = gradeLevelId
                        });
                }
            }

            if (audienceType == ResourceAudienceType.SchoolClasses)
            {
                foreach (var schoolClassId in schoolClassIds.Distinct())
                {
                    resource.ResourceSchoolClasses.Add(
                        new ResourceSchoolClass
                        {
                            SchoolClassId = schoolClassId
                        });
                }
            }
        }
    }
}
