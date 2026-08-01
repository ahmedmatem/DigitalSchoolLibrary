using SchoolLibrary.Domain.Constants;
using SchoolLibrary.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SchoolLibrary.Application.DTOs.ResourceDTOs
{
    public abstract class ResourceWriteDto
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

        [StringLength(
            ResourceConstants.AuthorMaxLength,
            ErrorMessage = "Името на автора може да съдържа до {1} символа.")]
        public string? Author { get; set; }

        [EnumDataType(
            typeof(ResourceType),
            ErrorMessage = "Невалиден тип на ресурса.")]
        public ResourceType Type { get; set; }

        [StringLength(500)]
        public string? FileStorageKey { get; set; }

        [StringLength(255)]
        public string? OriginalFileName { get; set; }

        [StringLength(150)]
        public string? FileContentType { get; set; }

        [Range(1, long.MaxValue)]
        public long? FileSize { get; set; }

        [StringLength(500)]
        public string? CoverStorageKey { get; set; }

        [StringLength(
            ResourceConstants.ExternalUrlMaxLength,
            ErrorMessage = "Адресът може да съдържа до {1} символа.")]
        [Url(ErrorMessage = "Въведеният външен адрес не е валиден URL.")]
        public string? ExternalUrl { get; set; }

        [Required(ErrorMessage = "Трябва да бъде избран предмет.")]
        public Guid SubjectId { get; set; }

        [Required(ErrorMessage = "Трябва да бъде избрана категория.")]
        public Guid CategoryId { get; set; }

        [EnumDataType(
            typeof(ResourceAudienceType),
            ErrorMessage = "Невалиден тип аудитория.")]
        public ResourceAudienceType AudienceType { get; set; }

        public ICollection<int> GradeLevelIds { get; set; }
            = new HashSet<int>();

        public ICollection<Guid> SchoolClassIds { get; set; }
            = new HashSet<Guid>();
    }
}
