using System.ComponentModel.DataAnnotations;
namespace SchoolLibrary.Application.DTOs.AuthDtos;
public class ForgotPasswordDto
{
    [Required(ErrorMessage="Имейлът е задължителен.")]
    [EmailAddress(ErrorMessage="Въведеният имейл адрес е невалиден.")]
    [StringLength(256)]
    public string Email { get; set; }=string.Empty;
}
