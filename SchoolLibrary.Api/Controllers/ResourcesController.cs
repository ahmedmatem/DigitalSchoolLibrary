using Microsoft.AspNetCore.Mvc;
using SchoolLibrary.Application.DTOs.ResourceDTOs;
using SchoolLibrary.Application.Interfaces;

namespace SchoolLibrary.Api.Controllers
{
    [Route("api/resources")]
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        private readonly IResourceService resourceService;

        public ResourcesController(IResourceService resourceService)
        {
            this.resourceService = resourceService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyCollection<ResourceListDto>>> GetAll(
            [FromQuery] ResourceQueryDto queryModel,
            CancellationToken cancellationToken)
        {
            var resources = await resourceService.GetAllAsync(
                queryModel,
                cancellationToken);

            return Ok(resources);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ResourceDetailsDto>> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var resource = await resourceService
                .GetByIdAsync(id, cancellationToken);

            if (resource is null)
            {
                return NotFound();
            }

            return Ok(resource);
        }

        [HttpPost]
        public async Task<ActionResult> Create(
            CreateResourceDto model,
            CancellationToken cancellationToken)
        {
            try
            {
                var id = await resourceService
                    .CreateAsync(model, cancellationToken);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id },
                    new { id });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new
                {
                    error = exception.Message
                });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult> Update(
            Guid id,
            UpdateResourceDto model,
            CancellationToken cancellationToken)
        {
            try
            {
                var updated = await resourceService
                    .UpdateAsync(id, model, cancellationToken);

                if (!updated)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new
                {
                    error = exception.Message
                });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken)
        {
            var deleted = await resourceService
                .DeleteAsync(id, cancellationToken);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
