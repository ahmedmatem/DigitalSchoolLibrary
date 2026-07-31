using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Application.Common.Exceptions;
using SchoolLibrary.Application.Common.Models;
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

        public async Task<PageResult<ResourceListDto>> GetAllAsync(
            ResourceQueryDto queryModel,
            CancellationToken cancellationToken = default)
        {
            var query = dbContext.Resources
                .AsNoTracking()
                .Where(resource => !resource.IsArchived)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(queryModel.Search))
            {
                var searchTerm = queryModel.Search.Trim();

                query = query.Where(resource =>
                    resource.Title.Contains(searchTerm) ||
                    resource.Description.Contains(searchTerm) ||
                    (resource.Author != null &&
                     resource.Author.Contains(searchTerm)) ||
                    resource.Subject.Name.Contains(searchTerm) ||
                    resource.Category.Name.Contains(searchTerm));
            }

            if (queryModel.SubjectId.HasValue)
            {
                query = query.Where(resource =>
                    resource.SubjectId == queryModel.SubjectId.Value);
            }

            if (queryModel.CategoryId.HasValue)
            {
                query = query.Where(resource =>
                    resource.CategoryId == queryModel.CategoryId.Value);
            }

            if (queryModel.Type.HasValue)
            {
                query = query.Where(resource =>
                    resource.Type == queryModel.Type.Value);
            }

            if (queryModel.AudienceType.HasValue)
            {
                query = query.Where(resource =>
                    resource.AudienceType == queryModel.AudienceType.Value);
            }

            if (queryModel.GradeLevelId.HasValue)
            {
                var gradeLevelId = queryModel.GradeLevelId.Value;

                query = query.Where(resource =>
                    resource.ResourceGradeLevels.Any(item =>
                        item.GradeLevelId == gradeLevelId));
            }

            if (queryModel.SchoolClassId.HasValue)
            {
                var schoolClassId = queryModel.SchoolClassId.Value;

                query = query.Where(resource =>
                    resource.ResourceSchoolClasses.Any(item =>
                        item.SchoolClassId == schoolClassId));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(resource => resource.CreatedAtUtc)
                .ThenBy(resource => resource.Id)
                .Skip((queryModel.Page - 1) * queryModel.PageSize)
                .Take(queryModel.PageSize)
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

            return new PageResult<ResourceListDto>
            {
                Items = items,
                Page = queryModel.Page,
                PageSize = queryModel.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<ResourceDetailsDto?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await dbContext.Resources
                .AsNoTracking()
                .Where(resource => resource.Id == id && !resource.IsArchived)
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
            var gradeLevelIds = model.GradeLevelIds
                .Distinct()
                .ToArray();

            var schoolClassIds = model.SchoolClassIds
                .Distinct()
                .ToArray();

            ValidateResourceLocation(
                model.Type,
                model.FilePath,
                model.ExternalUrl);

            await ValidateReferencesAsync(
                model.SubjectId,
                model.CategoryId,
                model.AudienceType,
                gradeLevelIds,
                schoolClassIds,
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
                CreatedAtUtc = DateTime.UtcNow,
                IsArchived = false,
                ArchivedAtUtc = null
            };

            AddAudienceRelations(
                resource,
                model.AudienceType,
                gradeLevelIds,
                schoolClassIds);

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

            var gradeLevelIds = model.GradeLevelIds
                .Distinct()
                .ToArray();

            var schoolClassIds = model.SchoolClassIds
                .Distinct()
                .ToArray();

            ValidateResourceLocation(
                model.Type,
                model.FilePath,
                model.ExternalUrl);

            await ValidateReferencesAsync(
                model.SubjectId,
                model.CategoryId,
                model.AudienceType,
                gradeLevelIds,
                schoolClassIds,
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
                gradeLevelIds,
                schoolClassIds);

            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> ArchiveAsync(
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

            if (resource.IsArchived)
            {
                return true;
            }

            // Mark the resource as archived instead of deleting it from the database
            resource.IsArchived = true;
            resource.ArchivedAtUtc = DateTime.UtcNow;
            resource.UpdatedAtUtc = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> RestoreAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var resource = await dbContext.Resources
                .FirstOrDefaultAsync(
                    resource => resource.Id == id,
                    cancellationToken);

            if (resource is null)
            {
                return false;
            }

            resource.IsArchived = false;
            resource.ArchivedAtUtc = null;
            resource.UpdatedAtUtc = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        private async Task ValidateReferencesAsync(
            Guid subjectId,
            Guid categoryId,
            ResourceAudienceType audienceType,
            IReadOnlyCollection<int> gradeLevelIds,
            IReadOnlyCollection<Guid> schoolClassIds,
            CancellationToken cancellationToken)
        {
            if (subjectId == Guid.Empty)
            {
                throw new ValidationException(
                    "Трябва да бъде избран валиден предмет.");
            }

            var subjectExists = await dbContext.Subjects
                .AnyAsync(
                    subject => subject.Id == subjectId,
                    cancellationToken);

            if (!subjectExists)
            {
                throw new ValidationException(
                    "Избраният предмет не съществува.");
            }

            if (categoryId == Guid.Empty)
            {
                throw new ValidationException(
                    "Трябва да бъде избрана валидна категория.");
            }

            var categoryExists = await dbContext.Categories
                .AnyAsync(
                    category => category.Id == categoryId,
                    cancellationToken);

            if (!categoryExists)
            {
                throw new ValidationException(
                    "Избраната категория не съществува.");
            }

            switch (audienceType)
            {
                case ResourceAudienceType.AllStudents:
                    if (gradeLevelIds.Count > 0 ||
                        schoolClassIds.Count > 0)
                    {
                        throw new ValidationException(
                            "При аудитория „Всички ученици“ не трябва да се избират класове или паралелки.");
                    }

                    break;

                case ResourceAudienceType.GradeLevels:
                    if (gradeLevelIds.Count == 0)
                    {
                        throw new ValidationException(
                            "Трябва да бъде избран поне един клас.");
                    }

                    if (schoolClassIds.Count > 0)
                    {
                        throw new ValidationException(
                            "При аудитория по класове не трябва да се избират паралелки.");
                    }

                    var existingGradeLevelCount =
                        await dbContext.GradeLevels.CountAsync(
                            gradeLevel =>
                                gradeLevelIds.Contains(gradeLevel.Id),
                            cancellationToken);

                    if (existingGradeLevelCount != gradeLevelIds.Count)
                    {
                        throw new ValidationException(
                            "Един или повече от избраните класове не съществуват.");
                    }

                    break;

                case ResourceAudienceType.SchoolClasses:
                    if (schoolClassIds.Count == 0)
                    {
                        throw new ValidationException(
                            "Трябва да бъде избрана поне една паралелка.");
                    }

                    if (gradeLevelIds.Count > 0)
                    {
                        throw new ValidationException(
                            "При аудитория по паралелки не трябва да се избират цели класове.");
                    }

                    var existingSchoolClassCount =
                        await dbContext.SchoolClasses.CountAsync(
                            schoolClass =>
                                schoolClassIds.Contains(schoolClass.Id),
                            cancellationToken);

                    if (existingSchoolClassCount != schoolClassIds.Count)
                    {
                        throw new ValidationException(
                            "Една или повече от избраните паралелки не съществуват.");
                    }

                    break;

                default:
                    throw new ValidationException("Избраният тип аудитория е невалиден.");
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

        private static void ValidateResourceLocation(
            ResourceType type,
            string? filePath,
            string? externalUrl)
        {
            if (type == ResourceType.ExternalLink)
            {
                if (string.IsNullOrWhiteSpace(externalUrl))
                {
                    throw new ValidationException(
                        "За ресурс от тип външна връзка трябва да бъде зададен URL.");
                }

                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    throw new ValidationException(
                        "Ресурс от тип външна връзка не трябва да съдържа файл.");
                }

                return;
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ValidationException(
                    "За този тип ресурс трябва да бъде зададен файл.");
            }

            if (!string.IsNullOrWhiteSpace(externalUrl))
            {
                throw new ValidationException(
                    "Ресурсът не може едновременно да съдържа файл и външна връзка.");
            }
        }
    }
}
