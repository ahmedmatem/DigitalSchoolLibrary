using Microsoft.AspNetCore.Mvc;
using SchoolLibrary.Application.DTOs.FileDtos;
using SchoolLibrary.Application.Interfaces;

namespace SchoolLibrary.Api.Controllers
{
    [Route("api/files")]
    [ApiController]
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
