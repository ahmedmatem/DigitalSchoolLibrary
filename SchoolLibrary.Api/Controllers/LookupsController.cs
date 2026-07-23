using Microsoft.AspNetCore.Mvc;
using SchoolLibrary.Application.DTOs.LookupDTOs;
using SchoolLibrary.Application.Interfaces;

namespace SchoolLibrary.Api.Controllers
{
    [ApiController]
    [Route("api/lookups")]
    public class LookupsController : ControllerBase
    {
        private readonly ILookupService lookupService;

        public LookupsController(ILookupService lookupService)
        {
            this.lookupService = lookupService;
        }

        [HttpGet("subjects")]
        [ProducesResponseType(
            typeof(IReadOnlyCollection<SubjectLookupDto>),
            StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyCollection<SubjectLookupDto>>>
            GetSubjects(CancellationToken cancellationToken)
        {
            var subjects = await lookupService
                .GetSubjectsAsync(cancellationToken);

            return Ok(subjects);
        }

        [HttpGet("categories")]
        [ProducesResponseType(
            typeof(IReadOnlyCollection<CategoryLookupDto>),
            StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyCollection<CategoryLookupDto>>>
            GetCategories(CancellationToken cancellationToken)
        {
            var categories = await lookupService
                .GetCategoriesAsync(cancellationToken);

            return Ok(categories);
        }

        [HttpGet("grade-levels")]
        [ProducesResponseType(
            typeof(IReadOnlyCollection<GradeLevelLookupDto>),
            StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyCollection<GradeLevelLookupDto>>>
            GetGradeLevels(CancellationToken cancellationToken)
        {
            var gradeLevels = await lookupService
                .GetGradeLevelsAsync(cancellationToken);

            return Ok(gradeLevels);
        }

        [HttpGet("school-classes")]
        [ProducesResponseType(
            typeof(IReadOnlyCollection<SchoolClassLookupDto>),
            StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyCollection<SchoolClassLookupDto>>>
            GetSchoolClasses(
                [FromQuery] int? gradeLevelId,
                CancellationToken cancellationToken)
        {
            var schoolClasses = await lookupService
                .GetSchoolClassesAsync(
                    gradeLevelId,
                    cancellationToken);

            return Ok(schoolClasses);
        }
    }
}
