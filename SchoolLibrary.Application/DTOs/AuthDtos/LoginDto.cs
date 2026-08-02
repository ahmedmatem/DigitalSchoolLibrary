using System.ComponentModel.DataAnnotations;

namespace SchoolLibrary.Application.DTOs.AuthDtos
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Имейлът е задължителен.")]
        [EmailAddress(ErrorMessage = "Въведеният имейл адрес е невалиден.")]
        [StringLength(
        256,
        ErrorMessage = "Имейлът не може да бъде по-дълъг от 256 символа.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Паролата е задължителна.")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
