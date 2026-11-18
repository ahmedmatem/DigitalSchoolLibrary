using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SchoolLibrary.Application.Common.Interfaces;
namespace SchoolLibrary.Infrastructure.Email;
public class ResendEmailSender:IEmailSender
{
    private readonly HttpClient client;
    private readonly EmailOptions options;
    public ResendEmailSender(HttpClient client,IOptions<EmailOptions> options)
    {
        this.client=client;this.options=options.Value;
    }
    public async Task SendPasswordResetAsync(string recipientEmail,string resetUrl,CancellationToken cancellationToken=default)
    {
        if(string.IsNullOrWhiteSpace(options.ApiToken))
            throw new InvalidOperationException("Resend API token is not configured.");
        using var request=new HttpRequestMessage(HttpMethod.Post,"emails");
        request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",options.ApiToken);
        request.Content=JsonContent.Create(new
        {
            from=$"{options.FromName} <{options.FromAddress}>",
            to=new[]{options.UseTestRecipient?options.TestRecipient:recipientEmail},
            subject="Възстановяване на парола",
            html=$"<p>Получихме заявка за нова парола.</p><p><a href=\"{System.Net.WebUtility.HtmlEncode(resetUrl)}\">Задаване на нова парола</a></p><p>Ако не сте изпратили заявката, игнорирайте това писмо.</p>",
            text=$"Получихме заявка за нова парола. Отворете: {resetUrl}\nАко не сте изпратили заявката, игнорирайте това писмо."
        });
        using var response=await client.SendAsync(request,cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
