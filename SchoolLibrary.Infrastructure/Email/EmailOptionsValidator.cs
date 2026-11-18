using System.Net.Mail;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace SchoolLibrary.Infrastructure.Email;

public sealed class EmailOptionsValidator(IHostEnvironment environment) : IValidateOptions<EmailOptions>
{
    public ValidateOptionsResult Validate(string? name, EmailOptions options)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(options.ApiToken))
            errors.Add("Email:ApiToken (or RESEND_APITOKEN) is required.");
        if (!MailAddress.TryCreate(options.FromAddress, out var from) || from.Address != options.FromAddress)
            errors.Add("Email:FromAddress must be an email address.");
        if (!Uri.TryCreate(options.FrontendBaseUrl, UriKind.Absolute, out var uri) ||
            !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment) ||
            (uri.Scheme != Uri.UriSchemeHttps && !(environment.IsDevelopment() && uri.Scheme == Uri.UriSchemeHttp)) ||
            (!environment.IsDevelopment() && (uri.IsLoopback || uri.Host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase))))
            errors.Add("Email:FrontendBaseUrl must be an absolute frontend URL without credentials, query or fragment; non-development URLs must use HTTPS and cannot be localhost.");
        if (options.UseTestRecipient && !environment.IsDevelopment())
            errors.Add("Email:UseTestRecipient is allowed only in Development.");
        if (options.UseTestRecipient && (!MailAddress.TryCreate(options.TestRecipient, out var to) || to.Address != options.TestRecipient))
            errors.Add("Email:TestRecipient must be an email address when test delivery is enabled.");
        if (!environment.IsDevelopment() && options.FromAddress.EndsWith("@resend.dev", StringComparison.OrdinalIgnoreCase))
            errors.Add("Email:FromAddress must use your verified sending domain outside Development.");
        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}
