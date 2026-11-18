using System.ComponentModel.DataAnnotations;
using SchoolLibrary.Application.DTOs.AuthDtos;
using Xunit;

namespace SchoolLibrary.Application.Tests.DTOs.AuthDtos;

public class ChangePasswordDtoTests
{
    [Theory]
    [InlineData(nameof(ChangePasswordDto.CurrentPassword), null)]
    [InlineData(nameof(ChangePasswordDto.CurrentPassword), "")]
    [InlineData(nameof(ChangePasswordDto.CurrentPassword), "   ")]
    [InlineData(nameof(ChangePasswordDto.NewPassword), null)]
    [InlineData(nameof(ChangePasswordDto.NewPassword), "")]
    [InlineData(nameof(ChangePasswordDto.NewPassword), "   ")]
    [InlineData(nameof(ChangePasswordDto.ConfirmNewPassword), null)]
    [InlineData(nameof(ChangePasswordDto.ConfirmNewPassword), "")]
    [InlineData(nameof(ChangePasswordDto.ConfirmNewPassword), "   ")]
    [InlineData(nameof(ChangePasswordDto.NewPassword), "abc12")]
    [InlineData(nameof(ChangePasswordDto.ConfirmNewPassword), "different123")]
    public void Validate_InvalidRequest_ReturnsErrorForMember(string member, string? value)
    {
        var model = CreateValidRequest();
        switch (member)
        {
            case nameof(model.CurrentPassword): model.CurrentPassword = value!; break;
            case nameof(model.NewPassword): model.NewPassword = value!; break;
            case nameof(model.ConfirmNewPassword): model.ConfirmNewPassword = value!; break;
            default: throw new ArgumentOutOfRangeException(nameof(member));
        }
        var errors = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, new ValidationContext(model), errors, true);

        Assert.False(isValid);
        Assert.Contains(errors, error => error.MemberNames.Contains(member));
    }

    [Theory]
    [InlineData("abc123")]
    [InlineData("new-password123")]
    public void Validate_ValidRequest_ReturnsNoErrors(string password)
    {
        var model = CreateValidRequest();
        model.NewPassword = password;
        model.ConfirmNewPassword = password;
        var errors = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, new ValidationContext(model), errors, true);

        Assert.True(isValid);
        Assert.Empty(errors);
    }

    private static ChangePasswordDto CreateValidRequest()
    {
        return new ChangePasswordDto
        {
            CurrentPassword = "current123",
            NewPassword = "new-password123",
            ConfirmNewPassword = "new-password123"
        };
    }
}
