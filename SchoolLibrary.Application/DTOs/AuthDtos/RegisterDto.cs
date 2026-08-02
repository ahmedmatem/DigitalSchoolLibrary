using System.ComponentModel.DataAnnotations;

namespace SchoolLibrary.Application.DTOs.AuthDtos
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Името е задължително.")]
        [StringLength(
        100,
        MinimumLength = 2,
        ErrorMessage = "Името трябва да бъде между 2 и 100 символа.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Бащиното име е задължително.")]
        [StringLength(
            100,
            MinimumLength = 2,
            ErrorMessage =
                "Бащиното име трябва да бъде между 2 и 100 символа.")]
        public string FatherName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Фамилията е задължителна.")]
        [StringLength(
            100,
            MinimumLength = 2,
            ErrorMessage = "Фамилията трябва да бъде между 2 и 100 символа.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Имейлът е задължителен.")]
        [EmailAddress(ErrorMessage = "Въведеният имейл адрес е невалиден.")]
        [StringLength(
            256,
            ErrorMessage = "Имейлът не може да бъде по-дълъг от 256 символа.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Паролата е задължителна.")]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "Паролата трябва да бъде между 6 и 100 символа.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Потвърждението на паролата е задължително.")]
        [Compare(
            nameof(Password),
            ErrorMessage = "Паролите не съвпадат.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /*
         * Остават nullable, защото зависимостта между тях е бизнес правило:
         * - или и двете са зададени;
         * - или и двете липсват.
         *
         * Това се проверява в AuthService.
         */
        public int? GradeLevelId { get; set; }

        public Guid? SchoolClassId { get; set; }
    }
}
