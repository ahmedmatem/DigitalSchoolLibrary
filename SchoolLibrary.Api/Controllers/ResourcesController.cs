using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public ResourcesController(
            IResourceService resourceService)
        {
            this.resourceService = resourceService;
        }

        // =========================================================
        // PUBLIC CATALOG
        // =========================================================

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetPublicCatalog(
            [FromQuery] ResourceQueryDto query,
            CancellationToken cancellationToken)
        {
            var result = await resourceService
                .GetPublicCatalogAsync(
                    query,
                    cancellationToken);

            return Ok(result);
        }

        // =========================================================
        // PERSONALIZED CATALOG
        // =========================================================

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

        // =========================================================
        // TEACHER / ADMIN - OWN SUBMISSIONS
        // =========================================================

        [Authorize(Roles = RoleConstants.Teacher + "," + RoleConstants.Admin)]
        [HttpGet("mine")]
        public async Task<IActionResult> GetMine(
            [FromQuery] ResourceQueryDto query,
            CancellationToken cancellationToken)
        {
            var result = await resourceService
                .GetMineAsync(
                    query,
                    cancellationToken);

            return Ok(result);
        }

        [Authorize(Roles = RoleConstants.Teacher + "," + RoleConstants.Admin)]
        [HttpGet("mine/summary")]
        public async Task<IActionResult> GetMineSummary(CancellationToken cancellationToken)
        {
            var result = await resourceService.GetMineSummaryAsync(cancellationToken);

            return Ok(result);
        }

        // =========================================================
        // ADMIN - PENDING RESOURCES
        // =========================================================

        [Authorize(Roles = RoleConstants.Admin)]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending(
            [FromQuery] ResourceQueryDto query,
            CancellationToken cancellationToken)
        {
            var result = await resourceService
                .GetPendingAsync(
                    query,
                    cancellationToken);

            return Ok(result);
        }

        // =========================================================
        // PUBLIC DETAILS
        // =========================================================

        [AllowAnonymous]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPublicDetails(
            Guid id,
            CancellationToken cancellationToken)
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

        // =========================================================
        // PUBLIC COVER
        // =========================================================

        [AllowAnonymous]
        [HttpGet("{id:guid}/cover")]
        public async Task<IActionResult> GetCover(
            Guid id,
            CancellationToken cancellationToken)
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

        // =========================================================
        // PROTECTED DOWNLOAD
        // =========================================================

        [Authorize]
        [HttpGet("{id:guid}/open")]
        public async Task<IActionResult> Open(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await resourceService.GetOpenUrlAsync(
                id,
                cancellationToken);

            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // =========================================================
        // ADMIN - MODERATION DOWNLOAD
        // =========================================================

        [Authorize(Roles = RoleConstants.Admin)]
        [HttpGet("{id:guid}/moderation-download")]
        public async Task<IActionResult> ModerationDownload(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await resourceService
                .CreateModerationDownloadUrlAsync(
                    id,
                    cancellationToken);

            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // =========================================================
        // MANAGEMENT DETAILS
        // =========================================================

        [Authorize(Roles = RoleConstants.Teacher + "," + RoleConstants.Admin)]
        [HttpGet("{id:guid}/manage")]
        public async Task<IActionResult> GetForManagement(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await resourceService
                .GetByIdAsync( id, cancellationToken);

            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [Authorize(Roles = RoleConstants.Teacher + "," + RoleConstants.Admin)]
        [HttpGet("{id:guid}/manage-open")]
        public async Task<IActionResult> OpenForManagement(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await resourceService
                .GetManagementOpenUrlAsync(
                    id,
                    cancellationToken);

            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [Authorize(Roles = RoleConstants.Teacher + "," + RoleConstants.Admin)]
        [HttpGet("{id:guid}/manage-cover")]
        public async Task<IActionResult> CoverForManagement(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await resourceService
                .CreateManagementCoverUrlAsync(
                    id,
                    cancellationToken);

            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [Authorize(Roles = RoleConstants.Admin)]
        [HttpGet("{id:guid}/moderation-cover")]
        public async Task<IActionResult> ModerationCover(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await resourceService
                .CreateModerationCoverUrlAsync(
                    id,
                    cancellationToken);

            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // =========================================================
        // CREATE
        // =========================================================

        [Authorize(Roles = RoleConstants.Teacher + "," + RoleConstants.Admin)]
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateResourceDto model,
            CancellationToken cancellationToken)
        {
            var id = await resourceService.CreateAsync(
                model,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetForManagement),
                new { id },
                new { id });
        }

        // =========================================================
        // UPDATE
        // =========================================================

        [Authorize(Roles = RoleConstants.Teacher + "," + RoleConstants.Admin)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateResourceDto model,
            CancellationToken cancellationToken)
        {
            var updated = await resourceService.UpdateAsync(
                id,
                model,
                cancellationToken);

            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }

        // =========================================================
        // ARCHIVE
        // =========================================================

        [Authorize(Roles = RoleConstants.Teacher + "," + RoleConstants.Admin)]
        [HttpPatch("{id:guid}/archive")]
        public async Task<IActionResult> Archive(
            Guid id,
            CancellationToken cancellationToken)
        {
            var archived = await resourceService.ArchiveAsync(
                id,
                cancellationToken);

            if (!archived)
            {
                return NotFound();
            }

            return NoContent();
        }

        // =========================================================
        // RESTORE - ADMIN ONLY
        // =========================================================

        [Authorize(Roles = RoleConstants.Admin)]
        [HttpPatch("{id:guid}/restore")]
        public async Task<IActionResult> Restore(
            Guid id,
            CancellationToken cancellationToken)
        {
            var restored = await resourceService.RestoreAsync(
                id,
                cancellationToken);

            if (!restored)
            {
                return NotFound();
            }

            return NoContent();
        }

        // =========================================================
        // MODERATION - APPROVE
        // =========================================================

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

        // =========================================================
        // MODERATION - REJECT
        // =========================================================

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

        // =========================================================
        // MODERATION - RESUBMIT
        // =========================================================

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
    }
}
