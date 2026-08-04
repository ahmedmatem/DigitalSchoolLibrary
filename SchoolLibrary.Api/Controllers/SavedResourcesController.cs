using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolLibrary.Application.DTOs.ResourceDTOs;
using SchoolLibrary.Application.Interfaces;

namespace SchoolLibrary.Api.Controllers
{
    [Route("api/saved-resources")]
    [ApiController]
    [Authorize]
    public class SavedResourcesController : ControllerBase
    {
        private readonly ISavedResourceService savedResourceService;

        public SavedResourcesController(ISavedResourceService savedResourceService)
        {
            this.savedResourceService = savedResourceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMine(
            [FromQuery] ResourceQueryDto query,
            CancellationToken cancellationToken)
        {
            var result = await savedResourceService.GetMineAsync(
                query,
                cancellationToken);

            return Ok(result);
        }

        [HttpPost("{resourceId:guid}")]
        public async Task<IActionResult> Save(
            Guid resourceId,
            CancellationToken cancellationToken)
        {
            var saved = await savedResourceService.SaveAsync(
                resourceId,
                cancellationToken);

            if (!saved)
            {
                /*
                 * Не уточняваме дали ресурсът не съществува
                 * или потребителят няма достъп до него.
                 */
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{resourceId:guid}")]
        public async Task<IActionResult> Remove(
            Guid resourceId,
            CancellationToken cancellationToken)
        {
            var removed = await savedResourceService.RemoveAsync(
                resourceId,
                cancellationToken);

            if (!removed)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
