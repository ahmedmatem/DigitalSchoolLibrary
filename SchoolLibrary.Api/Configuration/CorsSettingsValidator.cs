using Microsoft.Extensions.Options;

namespace SchoolLibrary.Api.Configuration;

public sealed class CorsSettingsValidator(IHostEnvironment environment) : IValidateOptions<CorsSettings>
{
    public ValidateOptionsResult Validate(string? name, CorsSettings options)
    {
        if (options.AllowedOrigins is null || options.AllowedOrigins.Length == 0 || options.AllowedOrigins.Any(origin =>
            !Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
            !string.IsNullOrEmpty(uri.UserInfo) ||
            origin != uri.GetLeftPart(UriPartial.Authority) ||
            (uri.Scheme != Uri.UriSchemeHttps &&
                !(environment.IsDevelopment() && uri.Scheme == Uri.UriSchemeHttp)) ||
            (!environment.IsDevelopment() && (uri.IsLoopback || uri.Host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase)))))
        {
            return ValidateOptionsResult.Fail("Cors:AllowedOrigins must contain explicit origins without paths or trailing slashes; non-development origins must use HTTPS and cannot be localhost.");
        }
        return ValidateOptionsResult.Success;
    }
}
