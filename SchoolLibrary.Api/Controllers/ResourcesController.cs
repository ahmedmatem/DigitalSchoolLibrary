using Microsoft.AspNetCore.Mvc;
using SchoolLibrary.Application.Common.Models;
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
        public async Task<ActionResult<PageResult<ResourceListDto>>> GetAll(
            [FromQuery] ResourceQueryDto queryModel,
            CancellationToken cancellationToken)
        {
            var result = await resourceService.GetAllAsync(
                queryModel,
                cancellationToken);

            return Ok(result);
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
            var id = await resourceService.CreateAsync(model, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult> Update(
            Guid id,
            UpdateResourceDto model,
            CancellationToken cancellationToken)
        {
            var updated = await resourceService.UpdateAsync(id, model, cancellationToken);

            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPatch("{id:guid}/archive")]
        public async Task<ActionResult> Archive(
            Guid id,
            CancellationToken cancellationToken)
        {
            var archived = await resourceService.ArchiveAsync(id, cancellationToken);

            if (!archived)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPatch("{id:guid}/restore")]
        public async Task<ActionResult> Restore(
            Guid id,
            CancellationToken cancellationToken)
        {
            var restored = await resourceService.RestoreAsync(id, cancellationToken);

            if (!restored)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
