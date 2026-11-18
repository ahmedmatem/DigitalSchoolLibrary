using System.ComponentModel.DataAnnotations;
namespace SchoolLibrary.Application.DTOs.AuthDtos;
public class ResetPasswordDto
{
    [Required,EmailAddress,StringLength(256)]
    public string Email { get; set; }=string.Empty;
    [Required]
    public string Token { get; set; }=string.Empty;
    [Required,MinLength(6)]
    public string NewPassword { get; set; }=string.Empty;
    [Required,Compare(nameof(NewPassword),ErrorMessage="Паролите не съвпадат.")]
    public string ConfirmNewPassword { get; set; }=string.Empty;
}
