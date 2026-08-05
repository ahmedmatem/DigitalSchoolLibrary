using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolLibrary.Application.DTOs.FileDtos;
using SchoolLibrary.Application.DTOs.ResourceDTOs;
using SchoolLibrary.Application.Interfaces;
using SchoolLibrary.Domain.Constants;

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

        [Authorize(Roles = RoleConstants.Teacher + "," + RoleConstants.Admin)]
        [HttpGet("mine")]
        public async Task<IActionResult> GetMine(
            [FromQuery] ResourceQueryDto query,
            CancellationToken cancellationToken)
        {
            var result = await resourceService.GetMineAsync(
                query,
                cancellationToken);

            return Ok(result);
        }

        [Authorize(Roles = RoleConstants.Admin)]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending(
            [FromQuery] ResourceQueryDto query,
            CancellationToken cancellationToken)
        {
            var result = await resourceService.GetPendingAsync(
                query,
                cancellationToken);

            return Ok(result);
        }

        [Authorize(Roles = RoleConstants.Admin)]
        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> Approve(
            Guid id,
            CancellationToken cancellationToken)
        {
            var approved = await resourceService.ApproveAsync(
                id,
                cancellationToken);

            if (!approved)
            {
                return NotFound();
            }

            return NoContent();
        }

        [Authorize(Roles = RoleConstants.Admin)]
        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(
            Guid id,
            [FromBody] RejectResourceDto model,
            CancellationToken cancellationToken)
        {
            var rejected = await resourceService.RejectAsync(
                id,
                model,
                cancellationToken);

            if (!rejected)
            {
                return NotFound();
            }

            return NoContent();
        }

        [Authorize(Roles = RoleConstants.Teacher + "," + RoleConstants.Admin)]
        [HttpPost("{id:guid}/resubmit")]
        public async Task<IActionResult> Resubmit(
            Guid id,
            CancellationToken cancellationToken)
        {
            var resubmitted = await resourceService.ResubmitAsync(
                id,
                cancellationToken);

            if (!resubmitted)
            {
                return NotFound();
            }

            return NoContent();
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

        [Authorize]
        [HttpGet("for-me")]
        public async Task<IActionResult> GetForMe(
            [FromQuery] ResourceQueryDto query,
            CancellationToken cancellationToken)
        {
            var result = await resourceService
                .GetForCurrentUserAsync(
                    query,
                    cancellationToken);

            return Ok(result);
        }
    }
}
