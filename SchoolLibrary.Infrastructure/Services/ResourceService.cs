using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Application.Common.Exceptions;
using SchoolLibrary.Application.Common.Interfaces;
using SchoolLibrary.Application.Common.Models;
using SchoolLibrary.Application.DTOs.FileDtos;
using SchoolLibrary.Application.DTOs.ResourceDTOs;
using SchoolLibrary.Application.Interfaces;
using SchoolLibrary.Domain.Constants;
using SchoolLibrary.Domain.Entities;
using SchoolLibrary.Domain.Enums;
using SchoolLibrary.Infrastructure.Data;

namespace SchoolLibrary.Infrastructure.Services
{
    public class ResourceService : IResourceService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IFileStorageService fileStorageService;
        private readonly ICurrentUserService currentUserService;

        public ResourceService(
            ApplicationDbContext dbContext,
            IFileStorageService fileStorageService,
            ICurrentUserService currentUserService)
        {
            this.dbContext = dbContext;
            this.fileStorageService = fileStorageService;
            this.currentUserService = currentUserService;
        }

        // =========================================================
        // PUBLIC CATALOG
        // =========================================================

        public async Task<PagedResult<PublicResourceListDto>> GetPublicCatalogAsync(
            ResourceQueryDto queryModel,
            CancellationToken cancellationToken = default)
        {
            NormalizePagination(queryModel);

            // Get optional user Id in order to get information if the resource is saved or not for the user
            var currentUserId = currentUserService.IsAuthenticated
                ? currentUserService.UserId
                : null;

            var query = dbContext.Resources
                .AsNoTracking()
                .Where(resource =>
                    !resource.IsArchived &&
                    resource.IsPubliclyVisible &&
                    resource.ModerationStatus ==
                        ResourceModerationStatus.Approved);

            query = ApplyCommonFilters(query, queryModel);

            var totalCount = await query.CountAsync(cancellationToken);

            var sortedQuery = ApplySorting(query, queryModel.Sort);

            var items = await sortedQuery
                .Skip((queryModel.Page - 1) * queryModel.PageSize)
                .Take(queryModel.PageSize)
                .Select(resource => new PublicResourceListDto
                {
                    Id = resource.Id,
                    Title = resource.Title,
                    Author = resource.Author,
                    Type = resource.Type,

                    SubjectName = resource.Subject.Name,

                    CategoryName = resource.Category.Name,

                    HasCover =
                        resource.CoverStorageKey != null &&
                        resource.CoverStorageKey != string.Empty,

                    IsSaved = 
                        currentUserId.HasValue &&
                        dbContext.SavedResources.Any(savedResource =>
                            savedResource.UserId == currentUserId.Value &&
                            savedResource.ResourceId == resource.Id),

                    CreatedAtUtc = resource.CreatedAtUtc
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<PublicResourceListDto>
            {
                Items = items,
                Page = queryModel.Page,
                PageSize = queryModel.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PublicResourceDetailsDto?> GetPublicDetailsAsync(
                Guid id,
                CancellationToken cancellationToken = default)
        {
            var resource = await dbContext.Resources
                .AsNoTracking()
                .Where(r => r.Id == id
                    && !r.IsArchived
                    && r.IsPubliclyVisible
                    && r.ModerationStatus == ResourceModerationStatus.Approved)
                .Select(r => new PublicResourceDetailsDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    Author = r.Author,
                    Type = r.Type,
                    SubjectName = r.Subject.Name,
                    CategoryName = r.Category.Name,
                    AudienceType = r.AudienceType,
                    HasCover = r.CoverStorageKey != null,
                    RequiresAuthentication = true,
                    CreatedAtUtc = r.CreatedAtUtc,

                    IsSaved = false
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (resource is null)
            {
                return null;
            }

            var userId = currentUserService.UserId;

            if (userId is Guid userIdValue)
            {
                resource.IsSaved = await dbContext.SavedResources
                    .AsNoTracking()
                    .AnyAsync(
                        sr =>
                            sr.UserId == userIdValue &&
                            sr.ResourceId == id,
                        cancellationToken);
            }

            return resource;
        }

        public async Task<PresignedDownloadDto?>
            CreatePublicCoverUrlAsync(
                Guid id,
                CancellationToken cancellationToken = default)
        {
            var coverStorageKey = await dbContext.Resources
                .AsNoTracking()
                .Where(resource =>
                    resource.Id == id &&
                    !resource.IsArchived &&
                    resource.IsPubliclyVisible &&
                    resource.ModerationStatus ==
                        ResourceModerationStatus.Approved &&
                    resource.CoverStorageKey != null &&
                    resource.CoverStorageKey != string.Empty)
                .Select(resource => resource.CoverStorageKey)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(coverStorageKey))
            {
                return null;
            }

            var coverExists =
                await fileStorageService.ObjectExistsAsync(
                    coverStorageKey,
                    cancellationToken);

            if (!coverExists)
            {
                return null;
            }

            return await fileStorageService.CreateDownloadUrlAsync(
                coverStorageKey,
                fileName: null,
                cancellationToken);
        }

        // =========================================================
        // PERSONALIZED CATALOG
        // =========================================================

        public async Task<PagedResult<PersonalResourceListDto>>
            GetForCurrentUserAsync(
                ResourceQueryDto queryModel,
                CancellationToken cancellationToken = default)
        {
            NormalizePagination(queryModel);

            var currentUserId = GetRequiredCurrentUserId();

            var query = dbContext.Resources
                .AsNoTracking()
                .Where(resource =>
                    !resource.IsArchived &&
                    resource.ModerationStatus ==
                        ResourceModerationStatus.Approved);

            query = await ApplyCurrentUserAudienceFilterAsync(
                query,
                cancellationToken);

            query = ApplyCommonFilters(query, queryModel);

            var totalCount = await query.CountAsync(
                cancellationToken);

            var items = await query
                .OrderByDescending(resource => resource.CreatedAtUtc)
                .ThenBy(resource => resource.Title)
                .Skip((queryModel.Page - 1) * queryModel.PageSize)
                .Take(queryModel.PageSize)
                .Select(resource => new PersonalResourceListDto
                {
                    Id = resource.Id,
                    Title = resource.Title,
                    Author = resource.Author,
                    Type = resource.Type,

                    SubjectName = resource.Subject.Name,
                    CategoryName = resource.Category.Name,

                    AudienceType = resource.AudienceType,

                    HasCover =
                        resource.CoverStorageKey != null &&
                        resource.CoverStorageKey != string.Empty,

                    IsSaved = dbContext.SavedResources.Any(savedResource =>
                        savedResource.UserId == currentUserId &&
                        savedResource.ResourceId == resource.Id),

                    CreatedAtUtc = resource.CreatedAtUtc
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<PersonalResourceListDto>
            {
                Items = items,
                Page = queryModel.Page,
                PageSize = queryModel.PageSize,
                TotalCount = totalCount
            };
        }

        // =========================================================
        // MANAGEMENT LIST
        // =========================================================

        public async Task<PagedResult<ResourceListDto>> GetAllAsync(
            ResourceQueryDto queryModel,
            CancellationToken cancellationToken = default)
        {
            NormalizePagination(queryModel);

            var query = dbContext.Resources
                .AsNoTracking()
                .Where(resource => !resource.IsArchived);

            query = ApplyManagementFilters(query, queryModel);

            var totalCount = await query.CountAsync(
                cancellationToken);

            var items = await query
                .OrderByDescending(resource => resource.CreatedAtUtc)
                .ThenBy(resource => resource.Title)
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
        // MANAGEMENT DETAILS
        // =========================================================

        public async Task<ResourceDetailsDto?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = GetRequiredCurrentUserId();

            var query = dbContext.Resources
                .AsNoTracking()
                .Where(resource =>
                    resource.Id == id &&
                    !resource.IsArchived);

            /*
             * Admin може да вижда всеки ресурс.
             * Teacher може да вижда само собствените си ресурси.
             */
            if (!currentUserService.IsInRole(RoleConstants.Admin))
            {
                query = query.Where(resource =>
                    resource.SubmittedByUserId == currentUserId);
            }

            return await query
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
                    IsPubliclyVisible = resource.IsPubliclyVisible,

                    ModerationStatus = resource.ModerationStatus,

                    SubmittedByUserId = resource.SubmittedByUserId,

                    SubmittedAtUtc =  resource.SubmittedAtUtc,

                    ReviewedByUserId =
                        resource.ReviewedByUserId,

                    ReviewedAtUtc =
                        resource.ReviewedAtUtc,

                    RejectionReason =
                        resource.RejectionReason,

                    GradeLevelIds = resource.ResourceGradeLevels
                        .Select(relation => relation.GradeLevelId)
                        .ToArray(),

                    SchoolClassIds = resource.ResourceSchoolClasses
                        .Select(relation => relation.SchoolClassId)
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
            var currentUserId = GetRequiredCurrentUserId();

            var isAdmin =
                currentUserService.IsInRole(RoleConstants.Admin);

            var moderationStatus = isAdmin
                ? ResourceModerationStatus.Approved
                : ResourceModerationStatus.Pending;

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

            ValidateFileMetadata(
                model.FileStorageKey,
                model.OriginalFileName,
                model.FileContentType,
                model.FileSize);

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

            var now = DateTime.UtcNow;

            var resource = new Resource
            {
                Id = Guid.NewGuid(),

                Title = model.Title.Trim(),
                Description = model.Description.Trim(),
                Author = NormalizeOptionalText(model.Author),

                Type = model.Type,
                AudienceType = model.AudienceType,

                IsPubliclyVisible = model.IsPubliclyVisible,

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

                ModerationStatus = moderationStatus,

                SubmittedByUserId = currentUserId,
                SubmittedAtUtc = now,

                ReviewedByUserId = isAdmin
                    ? currentUserId
                    : null,

                ReviewedAtUtc = isAdmin
                    ? now
                    : null,

                RejectionReason = null,

                CreatedAtUtc = now,
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
                .Include(item => item.ResourceGradeLevels)
                .Include(item => item.ResourceSchoolClasses)
                .FirstOrDefaultAsync(
                    item =>
                        item.Id == id &&
                        !item.IsArchived,
                    cancellationToken);

            if (resource is null)
            {
                return false;
            }

            var currentUserId = GetRequiredCurrentUserId();

            var isAdmin =
                currentUserService.IsInRole(RoleConstants.Admin);

            var isTeacher =
                currentUserService.IsInRole(RoleConstants.Teacher);

            if (!isAdmin)
            {
                var isOwner =
                    resource.SubmittedByUserId == currentUserId;

                var canEditStatus =
                    resource.ModerationStatus ==
                        ResourceModerationStatus.Pending ||
                    resource.ModerationStatus ==
                        ResourceModerationStatus.Rejected;

                if (!isTeacher ||
                    !isOwner ||
                    !canEditStatus)
                {
                    return false;
                }
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

            ValidateFileMetadata(
                model.FileStorageKey,
                model.OriginalFileName,
                model.FileContentType,
                model.FileSize);

            await ValidateReferencesAsync(
                model.SubjectId,
                model.CategoryId,
                model.AudienceType,
                gradeLevelIds,
                schoolClassIds,
                cancellationToken);

            var normalizedFileStorageKey =
                NormalizeOptionalText(model.FileStorageKey);

            var normalizedCoverStorageKey =
                NormalizeOptionalText(model.CoverStorageKey);

            if (!string.Equals(
                    resource.FileStorageKey,
                    normalizedFileStorageKey,
                    StringComparison.Ordinal))
            {
                await ValidateStorageObjectAsync(
                    normalizedFileStorageKey,
                    "Каченият учебен файл не беше намерен.",
                    cancellationToken);
            }

            if (!string.Equals(
                    resource.CoverStorageKey,
                    normalizedCoverStorageKey,
                    StringComparison.Ordinal))
            {
                await ValidateStorageObjectAsync(
                    normalizedCoverStorageKey,
                    "Каченото изображение за корица не беше намерено.",
                    cancellationToken);
            }

            resource.Title = model.Title.Trim();
            resource.Description = model.Description.Trim();
            resource.Author = NormalizeOptionalText(model.Author);

            resource.Type = model.Type;
            resource.AudienceType = model.AudienceType;

            resource.IsPubliclyVisible =
                model.IsPubliclyVisible;

            resource.FileStorageKey =
                normalizedFileStorageKey;

            resource.OriginalFileName =
                NormalizeOptionalText(model.OriginalFileName);

            resource.FileContentType =
                NormalizeOptionalText(model.FileContentType);

            resource.FileSize = model.FileSize;

            resource.CoverStorageKey =
                normalizedCoverStorageKey;

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
        // MODERATION - PENDING RESOURCES
        // =========================================================

        public async Task<PagedResult<ModerationResourceDto>>
            GetPendingAsync(
                ResourceQueryDto queryModel,
                CancellationToken cancellationToken = default)
        {
            NormalizePagination(queryModel);

            var query = dbContext.Resources
                .AsNoTracking()
                .Where(resource =>
                    !resource.IsArchived &&
                    resource.ModerationStatus ==
                        ResourceModerationStatus.Pending);

            query = ApplyCommonFilters(query, queryModel);

            var totalCount = await query.CountAsync(
                cancellationToken);

            var items = await query
                .OrderBy(resource => resource.SubmittedAtUtc)
                .Skip((queryModel.Page - 1) * queryModel.PageSize)
                .Take(queryModel.PageSize)
                .Select(resource => new ModerationResourceDto
                {
                    Id = resource.Id,
                    Title = resource.Title,
                    Description = resource.Description,
                    Author = resource.Author,
                    Type = resource.Type,

                    SubjectName = resource.Subject.Name,
                    CategoryName = resource.Category.Name,

                    AudienceType = resource.AudienceType,
                    ModerationStatus = resource.ModerationStatus,

                    HasFile =
                        resource.FileStorageKey != null &&
                        resource.FileStorageKey != string.Empty,

                    HasCover =
                        resource.CoverStorageKey != null &&
                        resource.CoverStorageKey != string.Empty,

                    ExternalUrl = resource.ExternalUrl,

                    SubmittedByUserId =
                        resource.SubmittedByUserId,

                    SubmittedAtUtc =
                        resource.SubmittedAtUtc,

                    ReviewedByUserId =
                        resource.ReviewedByUserId,

                    ReviewedAtUtc =
                        resource.ReviewedAtUtc,

                    RejectionReason =
                        resource.RejectionReason
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<ModerationResourceDto>
            {
                Items = items,
                Page = queryModel.Page,
                PageSize = queryModel.PageSize,
                TotalCount = totalCount
            };
        }

        // =========================================================
        // MODERATION DOWNLOAD
        // =========================================================

        public async Task<PresignedDownloadDto?> CreateModerationDownloadUrlAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            /*
             * Допълнителна service-level защита.
             * Controller-ът също ще бъде ограничен само за Admin.
             */
            if (!currentUserService.IsInRole(RoleConstants.Admin))
            {
                return null;
            }

            var resource = await dbContext.Resources
                .AsNoTracking()
                .Where(resource =>
                    resource.Id == id &&
                    !resource.IsArchived &&
                    (
                        resource.ModerationStatus == ResourceModerationStatus.Pending ||
                        resource.ModerationStatus == ResourceModerationStatus.Rejected
                    ))
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

            var fileExists = 
                await fileStorageService.ObjectExistsAsync(
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

        public async Task<PresignedDownloadDto?> CreateModerationCoverUrlAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (!currentUserService.IsInRole(RoleConstants.Admin))
            {
                return null;
            }

            var coverStorageKey = await dbContext.Resources
                .AsNoTracking()
                .Where(resource =>
                    resource.Id == id &&
                    !resource.IsArchived &&
                    (
                        resource.ModerationStatus == ResourceModerationStatus.Pending ||
                        resource.ModerationStatus == ResourceModerationStatus.Rejected
                    ) &&
                    resource.CoverStorageKey != null &&
                    resource.CoverStorageKey != string.Empty)
                .Select(resource => resource.CoverStorageKey)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(coverStorageKey))
            {
                return null;
            }

            var exists =
                await fileStorageService.ObjectExistsAsync(
                    coverStorageKey,
                    cancellationToken);

            if (!exists)
            {
                return null;
            }

            return await fileStorageService.CreateDownloadUrlAsync(
                coverStorageKey,
                fileName: null,
                cancellationToken);
        }

        // =========================================================
        // MODERATION - MY SUBMITTED RESOURCES
        // =========================================================

        public async Task<PagedResult<ModerationResourceDto>>
            GetMineAsync(
                ResourceQueryDto queryModel,
                CancellationToken cancellationToken = default)
        {
            var currentUserId = GetRequiredCurrentUserId();

            NormalizePagination(queryModel);

            var query = dbContext.Resources
                .AsNoTracking()
                .Where(resource =>
                    !resource.IsArchived &&
                    resource.SubmittedByUserId == currentUserId);

            query = ApplyManagementFilters(query, queryModel);

            var totalCount = await query.CountAsync(
                cancellationToken);

            var items = await query
                .OrderByDescending(resource =>
                    resource.SubmittedAtUtc)
                .Skip((queryModel.Page - 1) * queryModel.PageSize)
                .Take(queryModel.PageSize)
                .Select(resource => new ModerationResourceDto
                {
                    Id = resource.Id,
                    Title = resource.Title,
                    Description = resource.Description,
                    Author = resource.Author,
                    Type = resource.Type,

                    SubjectName = resource.Subject.Name,
                    CategoryName = resource.Category.Name,

                    AudienceType = resource.AudienceType,
                    ModerationStatus = resource.ModerationStatus,

                    HasFile =
                        resource.FileStorageKey != null &&
                        resource.FileStorageKey != string.Empty,

                    HasCover =
                        resource.CoverStorageKey != null &&
                        resource.CoverStorageKey != string.Empty,

                    ExternalUrl = resource.ExternalUrl,

                    SubmittedByUserId =
                        resource.SubmittedByUserId,

                    SubmittedAtUtc =
                        resource.SubmittedAtUtc,

                    ReviewedByUserId =
                        resource.ReviewedByUserId,

                    ReviewedAtUtc =
                        resource.ReviewedAtUtc,

                    RejectionReason =
                        resource.RejectionReason
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<ModerationResourceDto>
            {
                Items = items,
                Page = queryModel.Page,
                PageSize = queryModel.PageSize,
                TotalCount = totalCount
            };
        }

        // =========================================================
        // MODERATION - APPROVE
        // =========================================================

        public async Task<bool> ApproveAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var reviewerId = GetRequiredCurrentUserId();

            var resource = await dbContext.Resources
                .FirstOrDefaultAsync(
                    resource =>
                        resource.Id == id &&
                        !resource.IsArchived,
                    cancellationToken);

            if (resource is null)
            {
                return false;
            }

            if (resource.ModerationStatus !=
                ResourceModerationStatus.Pending)
            {
                return false;
            }

            var now = DateTime.UtcNow;

            resource.ModerationStatus =
                ResourceModerationStatus.Approved;

            resource.ReviewedByUserId = reviewerId;
            resource.ReviewedAtUtc = now;
            resource.RejectionReason = null;
            resource.UpdatedAtUtc = now;

            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        // =========================================================
        // MODERATION - REJECT
        // =========================================================

        public async Task<bool> RejectAsync(
            Guid id,
            RejectResourceDto model,
            CancellationToken cancellationToken = default)
        {
            var reviewerId = GetRequiredCurrentUserId();

            var resource = await dbContext.Resources
                .FirstOrDefaultAsync(
                    resource =>
                        resource.Id == id &&
                        !resource.IsArchived,
                    cancellationToken);

            if (resource is null)
            {
                return false;
            }

            if (resource.ModerationStatus !=
                ResourceModerationStatus.Pending)
            {
                return false;
            }

            var now = DateTime.UtcNow;

            resource.ModerationStatus =
                ResourceModerationStatus.Rejected;

            resource.ReviewedByUserId = reviewerId;
            resource.ReviewedAtUtc = now;
            resource.RejectionReason = model.Reason.Trim();
            resource.UpdatedAtUtc = now;

            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        // =========================================================
        // MODERATION - RESUBMIT
        // =========================================================

        public async Task<bool> ResubmitAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = GetRequiredCurrentUserId();

            var resource = await dbContext.Resources
                .FirstOrDefaultAsync(
                    resource =>
                        resource.Id == id &&
                        !resource.IsArchived,
                    cancellationToken);

            if (resource is null)
            {
                return false;
            }

            var isAdmin =
                currentUserService.IsInRole(RoleConstants.Admin);

            var isOwner =
                resource.SubmittedByUserId == currentUserId;

            if (!isAdmin && !isOwner)
            {
                return false;
            }

            if (resource.ModerationStatus !=
                ResourceModerationStatus.Rejected)
            {
                return false;
            }

            var now = DateTime.UtcNow;

            resource.ModerationStatus =
                ResourceModerationStatus.Pending;

            resource.SubmittedAtUtc = now;

            resource.ReviewedByUserId = null;
            resource.ReviewedAtUtc = null;
            resource.RejectionReason = null;
            resource.UpdatedAtUtc = now;

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
                    item => item.Id == id,
                    cancellationToken);

            if (resource is null)
            {
                return false;
            }

            var currentUserId = GetRequiredCurrentUserId();

            var isAdmin =
                currentUserService.IsInRole(RoleConstants.Admin);

            if (!isAdmin && resource.SubmittedByUserId != currentUserId)
            {
                return false;
            }

            if (resource.IsArchived)
            {
                return true;
            }

            var now = DateTime.UtcNow;

            resource.IsArchived = true;
            resource.ArchivedAtUtc = now;
            resource.UpdatedAtUtc = now;

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
            /*
             * Controller endpoint-ът за Restore трябва да бъде
             * разрешен само за Admin.
             */

            var resource = await dbContext.Resources
                .FirstOrDefaultAsync(
                    item => item.Id == id,
                    cancellationToken);

            if (resource is null)
            {
                return false;
            }

            if (!currentUserService.IsInRole(RoleConstants.Admin))
            {
                return false;
            }

            if (!resource.IsArchived)
            {
                return true;
            }

            var now = DateTime.UtcNow;

            resource.IsArchived = false;
            resource.ArchivedAtUtc = null;
            resource.UpdatedAtUtc = now;

            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        // =========================================================
        // PROTECTED DOWNLOAD
        // =========================================================

        public async Task<ResourceOpenDto?> GetOpenUrlAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var query = dbContext.Resources
                .AsNoTracking()
                .Where(resource =>
                    resource.Id == id &&
                    !resource.IsArchived &&
                    resource.ModerationStatus == ResourceModerationStatus.Approved);

            query = await ApplyCurrentUserAudienceFilterAsync(
                query,
                cancellationToken);

            var resource = await query
                .Select(resource => new
                {
                    resource.Type,
                    resource.FileStorageKey,
                    resource.OriginalFileName,
                    resource.ExternalUrl
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (resource is null)
            {
                return null;
            }

            if (resource.Type == ResourceType.ExternalLink)
            {
                if (string.IsNullOrWhiteSpace(resource.ExternalUrl))
                {
                    return null;
                }

                return new ResourceOpenDto
                {
                    Url = resource.ExternalUrl
                };
            }

            if (string.IsNullOrWhiteSpace(resource.FileStorageKey))
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

            var presignedDownload = await fileStorageService.CreateDownloadUrlAsync(
                    resource.FileStorageKey,
                    resource.OriginalFileName,
                    cancellationToken);

            return new ResourceOpenDto
            {
                Url = presignedDownload.DownloadUrl
            };
        }

        // =========================================================
        // CURRENT USER AUDIENCE FILTER
        // =========================================================

        private async Task<IQueryable<Resource>>
            ApplyCurrentUserAudienceFilterAsync(
                IQueryable<Resource> query,
                CancellationToken cancellationToken)
        {
            if (!currentUserService.IsAuthenticated ||
                !currentUserService.UserId.HasValue)
            {
                return query.Where(resource => false);
            }

            if (currentUserService.IsInRole(RoleConstants.Teacher) ||
                currentUserService.IsInRole(RoleConstants.Admin))
            {
                return query;
            }

            if (!currentUserService.IsInRole(
                    RoleConstants.Student))
            {
                return query.Where(resource => false);
            }

            var userId = currentUserService.UserId.Value;

            var studentData = await dbContext.Users
                .AsNoTracking()
                .Where(user => user.Id == userId)
                .Select(user => new
                {
                    user.GradeLevelId,
                    user.SchoolClassId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (studentData is null)
            {
                return query.Where(resource => false);
            }

            var gradeLevelId =
                studentData.GradeLevelId;

            var schoolClassId =
                studentData.SchoolClassId;

            return query.Where(resource =>
                resource.AudienceType ==
                    ResourceAudienceType.AllStudents

                || (
                    resource.AudienceType ==
                        ResourceAudienceType.GradeLevels
                    && gradeLevelId.HasValue
                    && resource.ResourceGradeLevels.Any(
                        relation =>
                            relation.GradeLevelId ==
                            gradeLevelId.Value)
                )

                || (
                    resource.AudienceType ==
                        ResourceAudienceType.SchoolClasses
                    && schoolClassId.HasValue
                    && resource.ResourceSchoolClasses.Any(
                        relation =>
                            relation.SchoolClassId ==
                            schoolClassId.Value)
                ));
        }

        // =========================================================
        // FILTERS
        // =========================================================

        private static IQueryable<Resource>
            ApplyManagementFilters(
                IQueryable<Resource> query,
                ResourceQueryDto queryModel)
        {
            query = ApplyCommonFilters(query, queryModel);

            if (queryModel.AudienceType.HasValue)
            {
                query = query.Where(resource =>
                    resource.AudienceType ==
                    queryModel.AudienceType.Value);
            }

            if (queryModel.ModerationStatus.HasValue)
            {
                query = query.Where(resource =>
                    resource.ModerationStatus ==
                    queryModel.ModerationStatus.Value);
            }

            //if (queryModel.GradeLevelId.HasValue)
            //{
            //    var gradeLevelId =
            //        queryModel.GradeLevelId.Value;

            //    query = query.Where(resource =>
            //        resource.ResourceGradeLevels.Any(
            //            relation =>
            //                relation.GradeLevelId ==
            //                gradeLevelId));
            //}

            if (queryModel.SchoolClassId.HasValue)
            {
                var schoolClassId =
                    queryModel.SchoolClassId.Value;

                query = query.Where(resource =>
                    resource.ResourceSchoolClasses.Any(
                        relation =>
                            relation.SchoolClassId ==
                            schoolClassId));
            }

            return query;
        }

        private static IQueryable<Resource> ApplyCommonFilters(
                IQueryable<Resource> query,
                ResourceQueryDto queryModel)
        {
            if (!string.IsNullOrWhiteSpace(queryModel.Search))
            {
                var searchTerm = queryModel.Search.Trim();

                query = query.Where(resource =>
                    resource.Title.Contains(searchTerm) ||
                    resource.Description.Contains(searchTerm) ||
                    (
                        resource.Author != null &&
                        resource.Author.Contains(searchTerm)
                    ) ||
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

            if (queryModel.GradeLevelId.HasValue)
            {
                var gradeLevelId = queryModel.GradeLevelId.Value;

                query = query.Where(resource =>
                    resource.AudienceType == ResourceAudienceType.AllStudents ||
                    resource.ResourceGradeLevels.Any(
                        relation => relation.GradeLevelId == gradeLevelId) ||
                    resource.ResourceSchoolClasses.Any(
                        relation => relation.SchoolClass.GradeLevelId == gradeLevelId));
            }

            return query;
        }

        private static void NormalizePagination(
            ResourceQueryDto queryModel)
        {
            if (queryModel.Page < 1)
            {
                queryModel.Page = 1;
            }

            if (queryModel.PageSize < 1)
            {
                queryModel.PageSize = 12;
            }

            if (queryModel.PageSize > 100)
            {
                queryModel.PageSize = 100;
            }
        }

        // =========================================================
        // STORAGE VALIDATION
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
            if (string.IsNullOrWhiteSpace(storageKey))
            {
                return;
            }

            var exists =
                await fileStorageService.ObjectExistsAsync(
                    storageKey.Trim(),
                    cancellationToken);

            if (!exists)
            {
                throw new ValidationException(errorMessage);
            }
        }

        // =========================================================
        // RESOURCE LOCATION VALIDATION
        // =========================================================

        private static void ValidateResourceLocation(
            ResourceType type,
            string? fileStorageKey,
            string? externalUrl)
        {
            /*
             * Провери името на стойността в твоя ResourceType enum.
             * Ако при теб се казва Link, замени ExternalLink с Link.
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

        private static void ValidateFileMetadata(
            string? fileStorageKey,
            string? originalFileName,
            string? fileContentType,
            long? fileSize)
        {
            if (string.IsNullOrWhiteSpace(fileStorageKey))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(originalFileName))
            {
                throw new ValidationException(
                    "Липсва оригиналното име на качения файл.");
            }

            if (string.IsNullOrWhiteSpace(fileContentType))
            {
                throw new ValidationException(
                    "Липсва MIME типът на качения файл.");
            }

            if (!fileSize.HasValue ||
                fileSize.Value <= 0)
            {
                throw new ValidationException(
                    "Размерът на качения файл е невалиден.");
            }
        }

        // =========================================================
        // REFERENCES AND AUDIENCE VALIDATION
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

            var subjectExists =
                await dbContext.Subjects.AnyAsync(
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

            var categoryExists =
                await dbContext.Categories.AnyAsync(
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
                                gradeLevelIds.Contains(
                                    gradeLevel.Id),
                            cancellationToken);

                    if (existingGradeLevelCount !=
                        gradeLevelIds.Count)
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
                                schoolClassIds.Contains(
                                    schoolClass.Id),
                            cancellationToken);

                    if (existingSchoolClassCount !=
                        schoolClassIds.Count)
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
        // AUDIENCE RELATIONS
        // =========================================================

        private static void AddAudienceRelations(
            Resource resource,
            ResourceAudienceType audienceType,
            IReadOnlyCollection<int> gradeLevelIds,
            IReadOnlyCollection<Guid> schoolClassIds)
        {
            if (audienceType ==
                ResourceAudienceType.GradeLevels)
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

            if (audienceType ==
                ResourceAudienceType.SchoolClasses)
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
        // CURRENT USER
        // =========================================================

        private Guid GetRequiredCurrentUserId()
        {
            if (!currentUserService.IsAuthenticated ||
                !currentUserService.UserId.HasValue)
            {
                throw new UnauthorizedAccessException(
                    "Потребителят не е автентикиран.");
            }

            return currentUserService.UserId.Value;
        }

        private static string? NormalizeOptionalText(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static IOrderedQueryable<Resource> ApplySorting(
            IQueryable<Resource> query,
            ResourceSortOption sort)
        {
            return sort switch
            {
                ResourceSortOption.Oldest => query
                        .OrderBy(resource => resource.CreatedAtUtc)
                        .ThenBy(resource => resource.Title),

                ResourceSortOption.TitleAscending => query
                        .OrderBy(resource => resource.Title)
                        .ThenByDescending(resource => resource.CreatedAtUtc),

                ResourceSortOption.TitleDescending => query
                        .OrderByDescending(resource => resource.Title)
                        .ThenByDescending(resource => resource.CreatedAtUtc),

                _ => query
                        .OrderByDescending(resource => resource.CreatedAtUtc)
                        .ThenBy(resource => resource.Title)
            };
        }

        
    }
}
