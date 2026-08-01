using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Application.Common.Exceptions;
using SchoolLibrary.Application.Common.Models;
using SchoolLibrary.Application.DTOs.FileDtos;
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
        private readonly IFileStorageService fileStorageService;

        public ResourceService(
            ApplicationDbContext dbContext,
            IFileStorageService fileStorageService)
        {
            this.dbContext = dbContext;
            this.fileStorageService = fileStorageService;
        }

        // =========================================================
        // GET ALL: търсене, филтриране и странициране
        // =========================================================

        public async Task<PagedResult<ResourceListDto>> GetAllAsync(
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
                    resource.ResourceGradeLevels.Any(resourceGradeLevel =>
                        resourceGradeLevel.GradeLevelId == gradeLevelId));
            }

            if (queryModel.SchoolClassId.HasValue)
            {
                var schoolClassId = queryModel.SchoolClassId.Value;

                query = query.Where(resource =>
                    resource.ResourceSchoolClasses.Any(resourceSchoolClass =>
                        resourceSchoolClass.SchoolClassId == schoolClassId));
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

                    CoverStorageKey = resource.CoverStorageKey,

                    CreatedAtUtc = resource.CreatedAtUtc
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<ResourceListDto>
            {
                Items = items,
                Page = queryModel.Page,
                PageSize = queryModel.PageSize,
                TotalCount = totalCount
            };
        }

        // =========================================================
        // GET BY ID
        // =========================================================

        public async Task<ResourceDetailsDto?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await dbContext.Resources
                .AsNoTracking()
                .Where(resource =>
                    resource.Id == id &&
                    !resource.IsArchived)
                .Select(resource => new ResourceDetailsDto
                {
                    Id = resource.Id,
                    Title = resource.Title,
                    Description = resource.Description,
                    Author = resource.Author,
                    Type = resource.Type,

                    FileStorageKey = resource.FileStorageKey,
                    OriginalFileName = resource.OriginalFileName,
                    FileContentType = resource.FileContentType,
                    FileSize = resource.FileSize,
                    CoverStorageKey = resource.CoverStorageKey,

                    ExternalUrl = resource.ExternalUrl,

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

        // =========================================================
        // CREATE
        // =========================================================

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
                model.FileStorageKey,
                model.ExternalUrl);

            await ValidateReferencesAsync(
                model.SubjectId,
                model.CategoryId,
                model.AudienceType,
                gradeLevelIds,
                schoolClassIds,
                cancellationToken);

            await ValidateStoredFilesAsync(
                model.FileStorageKey,
                model.CoverStorageKey,
                cancellationToken);

            var resource = new Resource
            {
                Id = Guid.NewGuid(),

                Title = model.Title.Trim(),
                Description = model.Description.Trim(),
                Author = NormalizeOptionalText(model.Author),

                Type = model.Type,
                AudienceType = model.AudienceType,

                FileStorageKey =
                    NormalizeOptionalText(model.FileStorageKey),

                OriginalFileName =
                    NormalizeOptionalText(model.OriginalFileName),

                FileContentType =
                    NormalizeOptionalText(model.FileContentType),

                FileSize = model.FileSize,

                CoverStorageKey =
                    NormalizeOptionalText(model.CoverStorageKey),

                ExternalUrl =
                    NormalizeOptionalText(model.ExternalUrl),

                SubjectId = model.SubjectId,
                CategoryId = model.CategoryId,

                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = null,

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

        // =========================================================
        // UPDATE
        // =========================================================

        public async Task<bool> UpdateAsync(
            Guid id,
            UpdateResourceDto model,
            CancellationToken cancellationToken = default)
        {
            var resource = await dbContext.Resources
                .Include(resource => resource.ResourceGradeLevels)
                .Include(resource => resource.ResourceSchoolClasses)
                .FirstOrDefaultAsync(
                    resource => resource.Id == id,
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
                model.FileStorageKey,
                model.ExternalUrl);

            await ValidateReferencesAsync(
                model.SubjectId,
                model.CategoryId,
                model.AudienceType,
                gradeLevelIds,
                schoolClassIds,
                cancellationToken);

            /*
             * Проверяваме R2 само ако ключът е нов или е променен.
             * Това спестява излишна HEAD заявка при редакция само на заглавието.
             */
            if (!string.Equals(
                    resource.FileStorageKey,
                    model.FileStorageKey,
                    StringComparison.Ordinal))
            {
                await ValidateStorageObjectAsync(
                    model.FileStorageKey,
                    "Каченият учебен файл не беше намерен.",
                    cancellationToken);
            }

            if (!string.Equals(
                    resource.CoverStorageKey,
                    model.CoverStorageKey,
                    StringComparison.Ordinal))
            {
                await ValidateStorageObjectAsync(
                    model.CoverStorageKey,
                    "Каченото изображение за корица не беше намерено.",
                    cancellationToken);
            }

            resource.Title = model.Title.Trim();
            resource.Description = model.Description.Trim();
            resource.Author = NormalizeOptionalText(model.Author);

            resource.Type = model.Type;
            resource.AudienceType = model.AudienceType;

            resource.FileStorageKey =
                NormalizeOptionalText(model.FileStorageKey);

            resource.OriginalFileName =
                NormalizeOptionalText(model.OriginalFileName);

            resource.FileContentType =
                NormalizeOptionalText(model.FileContentType);

            resource.FileSize = model.FileSize;

            resource.CoverStorageKey =
                NormalizeOptionalText(model.CoverStorageKey);

            resource.ExternalUrl =
                NormalizeOptionalText(model.ExternalUrl);

            resource.SubjectId = model.SubjectId;
            resource.CategoryId = model.CategoryId;

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

        // =========================================================
        // ARCHIVE
        // =========================================================

        public async Task<bool> ArchiveAsync(
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

            if (resource.IsArchived)
            {
                return true;
            }

            resource.IsArchived = true;
            resource.ArchivedAtUtc = DateTime.UtcNow;
            resource.UpdatedAtUtc = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        // =========================================================
        // RESTORE
        // =========================================================

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

            if (!resource.IsArchived)
            {
                return true;
            }

            resource.IsArchived = false;
            resource.ArchivedAtUtc = null;
            resource.UpdatedAtUtc = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        // =========================================================
        // DOWNLOAD URL
        // =========================================================

        public async Task<PresignedDownloadDto?> CreateDownloadUrlAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var resource = await dbContext.Resources
                .AsNoTracking()
                .Where(resource =>
                    resource.Id == id &&
                    !resource.IsArchived)
                .Select(resource => new
                {
                    resource.FileStorageKey,
                    resource.OriginalFileName
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (resource is null ||
                string.IsNullOrWhiteSpace(resource.FileStorageKey))
            {
                return null;
            }

            var fileExists = await fileStorageService.ObjectExistsAsync(
                resource.FileStorageKey,
                cancellationToken);

            if (!fileExists)
            {
                return null;
            }

            return await fileStorageService.CreateDownloadUrlAsync(
                resource.FileStorageKey,
                resource.OriginalFileName,
                cancellationToken);
        }

        // =========================================================
        // ВАЛИДАЦИЯ НА R2 ОБЕКТИТЕ
        // =========================================================

        private async Task ValidateStoredFilesAsync(
            string? fileStorageKey,
            string? coverStorageKey,
            CancellationToken cancellationToken)
        {
            await ValidateStorageObjectAsync(
                fileStorageKey,
                "Каченият учебен файл не беше намерен.",
                cancellationToken);

            await ValidateStorageObjectAsync(
                coverStorageKey,
                "Каченото изображение за корица не беше намерено.",
                cancellationToken);
        }

        private async Task ValidateStorageObjectAsync(
            string? storageKey,
            string errorMessage,
            CancellationToken cancellationToken)
        {
            /*
             * null е позволено:
             * - ExternalLink няма учебен файл;
             * - корицата не е задължителна.
             */
            if (string.IsNullOrWhiteSpace(storageKey))
            {
                return;
            }

            var exists = await fileStorageService.ObjectExistsAsync(
                storageKey.Trim(),
                cancellationToken);

            if (!exists)
            {
                throw new ValidationException(errorMessage);
            }
        }

        // =========================================================
        // ВАЛИДАЦИЯ: файл или външен URL
        // =========================================================

        private static void ValidateResourceLocation(
            ResourceType type,
            string? fileStorageKey,
            string? externalUrl)
        {
            /*
             * Провери дали стойността в твоя enum се казва точно
             * ExternalLink. Ако е Link или ExternalUrl, промени името тук.
             */
            if (type == ResourceType.ExternalLink)
            {
                if (string.IsNullOrWhiteSpace(externalUrl))
                {
                    throw new ValidationException(
                        "За външен ресурс трябва да бъде зададен URL адрес.");
                }

                if (!string.IsNullOrWhiteSpace(fileStorageKey))
                {
                    throw new ValidationException(
                        "Външен ресурс не трябва да съдържа качен файл.");
                }

                return;
            }

            if (string.IsNullOrWhiteSpace(fileStorageKey))
            {
                throw new ValidationException(
                    "За този тип ресурс трябва да бъде качен файл.");
            }

            if (!string.IsNullOrWhiteSpace(externalUrl))
            {
                throw new ValidationException(
                    "Ресурсът не може едновременно да съдържа файл и външен URL адрес.");
            }
        }

        // =========================================================
        // ВАЛИДАЦИЯ: subject, category и audience
        // =========================================================

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

            var subjectExists = await dbContext.Subjects.AnyAsync(
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

            var categoryExists = await dbContext.Categories.AnyAsync(
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
                    throw new ValidationException(
                        "Избраният тип аудитория е невалиден.");
            }
        }

        // =========================================================
        // СЪЗДАВАНЕ НА ВРЪЗКИТЕ С АУДИТОРИЯТА
        // =========================================================

        private static void AddAudienceRelations(
            Resource resource,
            ResourceAudienceType audienceType,
            IReadOnlyCollection<int> gradeLevelIds,
            IReadOnlyCollection<Guid> schoolClassIds)
        {
            if (audienceType == ResourceAudienceType.GradeLevels)
            {
                foreach (var gradeLevelId in gradeLevelIds)
                {
                    resource.ResourceGradeLevels.Add(
                        new ResourceGradeLevel
                        {
                            ResourceId = resource.Id,
                            GradeLevelId = gradeLevelId
                        });
                }
            }

            if (audienceType == ResourceAudienceType.SchoolClasses)
            {
                foreach (var schoolClassId in schoolClassIds)
                {
                    resource.ResourceSchoolClasses.Add(
                        new ResourceSchoolClass
                        {
                            ResourceId = resource.Id,
                            SchoolClassId = schoolClassId
                        });
                }
            }
        }

        // =========================================================
        // НОРМАЛИЗИРАНЕ НА НЕЗАДЪЛЖИТЕЛНИ ТЕКСТОВЕ
        // =========================================================

        private static string? NormalizeOptionalText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
