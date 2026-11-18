using System.ComponentModel.DataAnnotations;
using SchoolLibrary.Application.DTOs.AuthDtos;
using Xunit;

namespace SchoolLibrary.Application.Tests.DTOs.AuthDtos;

public class ForgotPasswordDtoTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    [InlineData("student@")]
    public void Validate_InvalidEmail_ReturnsEmailError(string? email)
    {
        var model = new ForgotPasswordDto { Email = email! };
        var errors = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, new ValidationContext(model), errors, true);

        Assert.False(isValid);
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(model.Email)));
    }

    [Fact]
    public void Validate_EmailLongerThan256Characters_ReturnsEmailError()
    {
        var model = new ForgotPasswordDto { Email = new string('a', 245) + "@example.com" };
        var errors = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, new ValidationContext(model), errors, true);

        Assert.False(isValid);
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(model.Email)));
    }

    [Theory]
    [InlineData("student@example.com")]
    [InlineData("student+library@example.com")]
    public void Validate_ValidEmail_ReturnsNoErrors(string email)
    {
        var model = new ForgotPasswordDto { Email = email };
        var errors = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, new ValidationContext(model), errors, true);

        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_EmailExactly256Characters_ReturnsNoErrors()
    {
        var model = new ForgotPasswordDto { Email = new string('a', 244) + "@example.com" };
        var errors = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, new ValidationContext(model), errors, true);

        Assert.True(isValid);
        Assert.Empty(errors);
    }
}
