using Amazon.Auth.AccessControlPolicy;
using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Application.Common.Interfaces;
using SchoolLibrary.Application.Common.Models;
using SchoolLibrary.Application.DTOs.ResourceDTOs;
using SchoolLibrary.Application.Interfaces;
using SchoolLibrary.Domain.Constants;
using SchoolLibrary.Domain.Entities;
using SchoolLibrary.Domain.Enums;
using SchoolLibrary.Infrastructure.Data;

namespace SchoolLibrary.Infrastructure.Services
{
    public class SavedResourceService : ISavedResourceService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly ICurrentUserService currentUserService;

        public SavedResourceService(
            ApplicationDbContext dbContext,
            ICurrentUserService currentUserService)
        {
            this.dbContext = dbContext;
            this.currentUserService = currentUserService;
        }

        // =========================================================
        // GET MY SAVED RESOURCES
        // =========================================================

        public async Task<PagedResult<SavedResourceDto>> GetMineAsync(
            ResourceQueryDto queryModel,
            CancellationToken cancellationToken = default)
        {
            var userId = GetRequiredUserId();

            NormalizePagination(queryModel);

            var query = dbContext.SavedResources
                .AsNoTracking()
                .Where(savedResource =>
                    savedResource.UserId == userId &&
                    !savedResource.Resource.IsArchived &&
                    savedResource.Resource.ModerationStatus == ResourceModerationStatus.Approved);

            /*
             * Ако потребителят е ученик, не връщаме ресурс,
             * до който вече няма достъп.
             *
             * Например ученикът може да е преместен в друг клас.
             */
            if (currentUserService.IsInRole(RoleConstants.Student))
            {
                var studentData = await GetStudentDataAsync(
                    userId,
                    cancellationToken);

                if (studentData is null)
                {
                    return EmptyResult(queryModel);
                }

                var gradeLevelId = studentData.GradeLevelId;
                var schoolClassId = studentData.SchoolClassId;

                query = query.Where(savedResource =>
                    savedResource.Resource.AudienceType ==
                        ResourceAudienceType.AllStudents

                    || (
                        savedResource.Resource.AudienceType ==
                            ResourceAudienceType.GradeLevels
                        && gradeLevelId.HasValue
                        && savedResource.Resource
                            .ResourceGradeLevels.Any(relation =>
                                relation.GradeLevelId ==
                                gradeLevelId.Value)
                    )

                    || (
                        savedResource.Resource.AudienceType ==
                            ResourceAudienceType.SchoolClasses
                        && schoolClassId.HasValue
                        && savedResource.Resource
                            .ResourceSchoolClasses.Any(relation =>
                                relation.SchoolClassId ==
                                schoolClassId.Value)
                    ));
            }

            query = ApplyFilters(query, queryModel);

            var totalCount = await query.CountAsync(
                cancellationToken);

            var items = await query
                .OrderByDescending(savedResource =>
                    savedResource.SavedAtUtc)
                .Skip((queryModel.Page - 1) * queryModel.PageSize)
                .Take(queryModel.PageSize)
                .Select(savedResource => new SavedResourceDto
                {
                    Id = savedResource.Resource.Id,
                    Title = savedResource.Resource.Title,
                    Author = savedResource.Resource.Author,
                    Type = savedResource.Resource.Type,

                    SubjectName =
                        savedResource.Resource.Subject.Name,

                    CategoryName =
                        savedResource.Resource.Category.Name,

                    AudienceType =
                        savedResource.Resource.AudienceType,

                    HasCover =
                        savedResource.Resource.CoverStorageKey != null &&
                        savedResource.Resource.CoverStorageKey !=
                            string.Empty,

                    CreatedAtUtc =
                        savedResource.Resource.CreatedAtUtc,

                    SavedAtUtc = savedResource.SavedAtUtc
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<SavedResourceDto>
            {
                Items = items,
                Page = queryModel.Page,
                PageSize = queryModel.PageSize,
                TotalCount = totalCount
            };
        }

        // =========================================================
        // SAVE RESOURCE
        // =========================================================

        public async Task<bool> SaveAsync(
            Guid resourceId,
            CancellationToken cancellationToken = default)
        {
            var userId = GetRequiredUserId();

            var hasAccess = await CanCurrentUserAccessResourceAsync(
                resourceId,
                cancellationToken);

            if (!hasAccess)
            {
                return false;
            }

            var alreadySaved = await dbContext.SavedResources
                .AnyAsync(
                    savedResource =>
                        savedResource.UserId == userId &&
                        savedResource.ResourceId == resourceId,
                    cancellationToken);

            /*
             * POST е idempotent:
             * ако вече е запазен, считаме операцията за успешна.
             */
            if (alreadySaved)
            {
                return true;
            }

            var savedResource = new SavedResource
            {
                UserId = userId,
                ResourceId = resourceId,
                SavedAtUtc = DateTime.UtcNow
            };

            dbContext.SavedResources.Add(savedResource);

            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        // =========================================================
        // REMOVE RESOURCE
        // =========================================================

        public async Task<bool> RemoveAsync(
            Guid resourceId,
            CancellationToken cancellationToken = default)
        {
            var userId = GetRequiredUserId();

            var savedResource = await dbContext.SavedResources
                .FirstOrDefaultAsync(
                    item =>
                        item.UserId == userId &&
                        item.ResourceId == resourceId,
                    cancellationToken);

            if (savedResource is null)
            {
                return false;
            }

            dbContext.SavedResources.Remove(savedResource);

            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        // =========================================================
        // ACCESS CHECK
        // =========================================================

        private async Task<bool> CanCurrentUserAccessResourceAsync(
            Guid resourceId,
            CancellationToken cancellationToken)
        {
            var query = dbContext.Resources
                .AsNoTracking()
                .Where(resource =>
                    resource.Id == resourceId &&
                    !resource.IsArchived &&
                    resource.ModerationStatus == ResourceModerationStatus.Approved);

            /*
             * Teacher и Admin могат да запазват всички
             * активни ресурси.
             */
            if (currentUserService.IsInRole(RoleConstants.Teacher) ||
                currentUserService.IsInRole(RoleConstants.Admin))
            {
                return await query.AnyAsync(cancellationToken);
            }

            if (!currentUserService.IsInRole(RoleConstants.Student))
            {
                return false;
            }

            var userId = GetRequiredUserId();

            var studentData = await GetStudentDataAsync(
                userId,
                cancellationToken);

            if (studentData is null)
            {
                return false;
            }

            var gradeLevelId = studentData.GradeLevelId;
            var schoolClassId = studentData.SchoolClassId;

            return await query.AnyAsync(
                resource =>
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
                    ),
                cancellationToken);
        }

        // =========================================================
        // STUDENT DATA
        // =========================================================

        private async Task<StudentData?> GetStudentDataAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            return await dbContext.Users
                .AsNoTracking()
                .Where(user => user.Id == userId)
                .Select(user => new StudentData
                {
                    GradeLevelId = user.GradeLevelId,
                    SchoolClassId = user.SchoolClassId
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        // =========================================================
        // FILTERS
        // =========================================================

        private static IQueryable<SavedResource> ApplyFilters(
            IQueryable<SavedResource> query,
            ResourceQueryDto queryModel)
        {
            if (!string.IsNullOrWhiteSpace(queryModel.Search))
            {
                var searchTerm = queryModel.Search.Trim();

                query = query.Where(savedResource =>
                    savedResource.Resource.Title.Contains(searchTerm) ||
                    savedResource.Resource.Description.Contains(
                        searchTerm) ||
                    (
                        savedResource.Resource.Author != null &&
                        savedResource.Resource.Author.Contains(
                            searchTerm)
                    ) ||
                    savedResource.Resource.Subject.Name.Contains(
                        searchTerm) ||
                    savedResource.Resource.Category.Name.Contains(
                        searchTerm));
            }

            if (queryModel.SubjectId.HasValue)
            {
                query = query.Where(savedResource =>
                    savedResource.Resource.SubjectId ==
                    queryModel.SubjectId.Value);
            }

            if (queryModel.CategoryId.HasValue)
            {
                query = query.Where(savedResource =>
                    savedResource.Resource.CategoryId ==
                    queryModel.CategoryId.Value);
            }

            if (queryModel.Type.HasValue)
            {
                query = query.Where(savedResource =>
                    savedResource.Resource.Type ==
                    queryModel.Type.Value);
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

        private static PagedResult<SavedResourceDto> EmptyResult(
            ResourceQueryDto queryModel)
        {
            return new PagedResult<SavedResourceDto>
            {
                Items = Array.Empty<SavedResourceDto>(),
                Page = queryModel.Page,
                PageSize = queryModel.PageSize,
                TotalCount = 0
            };
        }

        private Guid GetRequiredUserId()
        {
            if (!currentUserService.IsAuthenticated ||
                !currentUserService.UserId.HasValue)
            {
                throw new UnauthorizedAccessException("Потребителят не е автентикиран.");
            }

            return currentUserService.UserId.Value;
        }

        private sealed class StudentData
        {
            public int? GradeLevelId { get; init; }

            public Guid? SchoolClassId { get; init; }
        }
    }
}
