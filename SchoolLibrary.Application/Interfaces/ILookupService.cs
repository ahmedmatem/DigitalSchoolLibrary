using SchoolLibrary.Application.DTOs.LookupDTOs;

namespace SchoolLibrary.Application.Interfaces
{
    public interface ILookupService
    {
        Task<IReadOnlyCollection<SubjectLookupDto>>
            GetSubjectsAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<CategoryLookupDto>>
            GetCategoriesAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<GradeLevelLookupDto>>
            GetGradeLevelsAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<SchoolClassLookupDto>>
            GetSchoolClassesAsync(
                int? gradeLevelId = null,
                CancellationToken cancellationToken = default);
    }
}
