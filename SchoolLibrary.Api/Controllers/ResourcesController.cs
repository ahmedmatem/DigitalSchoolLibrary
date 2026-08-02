using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolLibrary.Application.Common.Models;
using SchoolLibrary.Application.DTOs.FileDtos;
using SchoolLibrary.Application.DTOs.ResourceDTOs;
using SchoolLibrary.Application.Interfaces;
using SchoolLibrary.Domain.Constants;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] ResourceQueryDto query,
            CancellationToken cancellationToken)
        {
            var result = await resourceService
                .GetPublicCatalogAsync(
                    query,
                    cancellationToken);

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await resourceService
                .GetPublicDetailsAsync(
                    id,
                    cancellationToken);

            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("{id:guid}/cover")]
        public async Task<IActionResult> GetCover(Guid id, CancellationToken cancellationToken)
        {
            var result = await resourceService
                .CreatePublicCoverUrlAsync(
                    id,
                    cancellationToken);

            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [Authorize(Roles = RoleConstants.Teacher + "," + RoleConstants.Admin)]
        [HttpPost]
        public async Task<ActionResult> Create(
            CreateResourceDto model,
            CancellationToken cancellationToken)
        {
            var id = await resourceService.CreateAsync(model, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [Authorize(Roles = RoleConstants.Teacher + "," + RoleConstants.Admin)]
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

        [Authorize(Roles = RoleConstants.Teacher + "," + RoleConstants.Admin)]
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

        [Authorize(Roles = RoleConstants.Teacher + "," + RoleConstants.Admin)]
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

        /// <summary>
        /// Generates a presigned URL for downloading a resource file.
        /// Това връща краткотраен GET URL и самият файл се изтегля директно от R2.
        /// Cloudflare препоръчва точно този поток за private client-side downloads.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("{id:guid}/download")]
        public async Task<ActionResult<PresignedDownloadDto>> Download(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await resourceService
                .CreateDownloadUrlAsync(
                    id,
                    cancellationToken);

            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}
