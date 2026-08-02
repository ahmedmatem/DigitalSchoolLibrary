using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolLibrary.Application.DTOs.FileDtos;
using SchoolLibrary.Application.Interfaces;
using SchoolLibrary.Domain.Constants;

namespace SchoolLibrary.Api.Controllers
{
    [Route("api/files")]
    [ApiController]
    [Authorize(Roles = RoleConstants.Teacher + "," + RoleConstants.Admin)]
    public class FilesController : ControllerBase
    {
        private readonly IFileStorageService fileStorageService;

        public FilesController(
            IFileStorageService fileStorageService)
        {
            this.fileStorageService = fileStorageService;
        }

        [HttpPost("upload-url")]
        public async Task<ActionResult<PresignedUploadDto>> CreateUploadUrl(
            CreateUploadUrlDto model,
            CancellationToken cancellationToken)
        {
            var result = await fileStorageService
                .CreateUploadUrlAsync(model, cancellationToken);

            return Ok(result);
        }
    }
}
