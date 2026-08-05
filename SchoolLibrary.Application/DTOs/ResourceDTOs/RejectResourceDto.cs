using System.ComponentModel.DataAnnotations;

namespace SchoolLibrary.Application.DTOs.ResourceDTOs
{
    public class RejectResourceDto
    {
        [Required(ErrorMessage = "Причината за отхвърляне е задължителна.")]
        [StringLength(
        1000,
        MinimumLength = 5,
        ErrorMessage = "Причината трябва да бъде между 5 и 1000 символа.")]
        public string Reason { get; set; } = string.Empty;
    }
}
