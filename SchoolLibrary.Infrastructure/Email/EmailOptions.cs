namespace SchoolLibrary.Infrastructure.Email;
public class EmailOptions
{
    public const string SectionName="Email";
    public string ApiToken { get; set; }=string.Empty;
    public string FromName { get; set; }="Училищна библиотека";
    public string FromAddress { get; set; }="onboarding@resend.dev";
    public string FrontendBaseUrl { get; set; }="http://localhost:4200";
    public bool UseTestRecipient { get; set; }=true;
    public string TestRecipient { get; set; }="delivered+password-reset@resend.dev";
}
