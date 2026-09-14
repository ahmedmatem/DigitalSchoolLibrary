using System.ComponentModel.DataAnnotations;
namespace SchoolLibrary.Application.DTOs.AuthDtos;
public class ChangePasswordDto
{
    [Required(ErrorMessage="Настоящата парола е задължителна.")]
    public string CurrentPassword { get; set; }=string.Empty;
    [Required(ErrorMessage="Новата парола е задължителна."),MinLength(6,ErrorMessage="Новата парола трябва да съдържа поне 6 символа.")]
    public string NewPassword { get; set; }=string.Empty;
    [Required,Compare(nameof(NewPassword),ErrorMessage="Паролите не съвпадат.")]
    public string ConfirmNewPassword { get; set; }=string.Empty;
}
