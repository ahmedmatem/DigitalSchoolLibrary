using SchoolLibrary.Domain.Constants;
using SchoolLibrary.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchoolLibrary.Application.DTOs.ResourceDTOs
{
    public class UpdateResourceDto
    {
        [Required(ErrorMessage = "Заглавието е задължително.")]
        [StringLength(
        ResourceConstants.TitleMaxLength,
        MinimumLength = ResourceConstants.TitleMinLength,
        ErrorMessage =
            "Заглавието трябва да бъде между {2} и {1} символа.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Описанието е задължително.")]
        [StringLength(
            ResourceConstants.DescriptionMaxLength,
            MinimumLength = ResourceConstants.DescriptionMinLength,
            ErrorMessage =
                "Описанието трябва да бъде между {2} и {1} символа.")]
        public string Description { get; set; } = string.Empty;

        [StringLength(ResourceConstants.AuthorMaxLength)]
        public string? Author { get; set; }

        [EnumDataType(typeof(ResourceType))]
        public ResourceType Type { get; set; }

        [StringLength(ResourceConstants.FilePathMaxLength)]
        public string? FilePath { get; set; }

        [StringLength(ResourceConstants.ExternalUrlMaxLength)]
        [Url(ErrorMessage = "Въведеният външен адрес не е валиден URL.")]
        public string? ExternalUrl { get; set; }

        [StringLength(ResourceConstants.CoverImagePathMaxLength)]
        public string? CoverImagePath { get; set; }

        public Guid SubjectId { get; set; }

        public Guid CategoryId { get; set; }

        [EnumDataType(typeof(ResourceAudienceType))]
        public ResourceAudienceType AudienceType { get; set; }

        public ICollection<int> GradeLevelIds { get; set; }
            = new HashSet<int>();

        public ICollection<Guid> SchoolClassIds { get; set; }
            = new HashSet<Guid>();
    }
}
