using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolLibrary.Application.DTOs.AuthDtos;
using SchoolLibrary.Application.Interfaces;
using System.Security.Claims;

namespace SchoolLibrary.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<MeDto>> Register(
            RegisterDto model,
            CancellationToken cancellationToken)
        {
            var result = await authService.RegisterAsync(
                model,
                cancellationToken);

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<MeDto>> Login(
            LoginDto model,
            CancellationToken cancellationToken)
        {
            var result = await authService.LoginAsync(
                model,
                cancellationToken);

            if (result is null)
            {
                return Unauthorized(new
                {
                    message = "Невалиден имейл или парола."
                });
            }

            return Ok(result);
        }

        [AllowAnonymous]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("password-recovery")]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordDto model,
            CancellationToken cancellationToken)
        {
            await authService.ForgotPasswordAsync(model, cancellationToken);
            return Ok(new { message = "Ако съществува активен профил с този имейл, ще бъде изпратен линк за възстановяване." });
        }

        [AllowAnonymous]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("password-recovery")]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordDto model,
            CancellationToken cancellationToken)
        {
            await authService.ResetPasswordAsync(model, cancellationToken);
            return NoContent();
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await authService.LogoutAsync();

            return NoContent();
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordDto model,
            CancellationToken cancellationToken)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            await authService.ChangePasswordAsync(userId, model, cancellationToken);
            return NoContent();
        }

        [HttpGet("me")]
        public async Task<ActionResult<MeDto>> Me(
            CancellationToken cancellationToken)
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var result = await authService.GetMeAsync(
                userId,
                cancellationToken);

            if (result is null)
            {
                return Unauthorized();
            }

            return Ok(result);
        }
    }
}
