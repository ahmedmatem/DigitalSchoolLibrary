using SchoolLibrary.Application.DTOs.AuthDtos;

namespace SchoolLibrary.Application.Interfaces
{
    public interface IAuthService
    {
        Task<MeDto> RegisterAsync(
            RegisterDto model,
            CancellationToken cancellationToken = default);

        Task<MeDto?> LoginAsync(
            LoginDto model,
            CancellationToken cancellationToken = default);

        Task LogoutAsync();

        Task ForgotPasswordAsync(
            ForgotPasswordDto model,
            CancellationToken cancellationToken = default);

        Task ResetPasswordAsync(
            ResetPasswordDto model,
            CancellationToken cancellationToken = default);

        Task ChangePasswordAsync(
            Guid userId,
            ChangePasswordDto model,
            CancellationToken cancellationToken = default);

        Task<MeDto?> GetMeAsync(
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
