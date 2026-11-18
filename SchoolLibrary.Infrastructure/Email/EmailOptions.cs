namespace SchoolLibrary.Infrastructure.Email;
public class EmailOptions
{
    public const string SectionName="Email";
    public string ApiToken { get; set; }=string.Empty;
    public string FromName { get; set; }="Училищна библиотека";
    public string FromAddress { get; set; }=string.Empty;
    public string FrontendBaseUrl { get; set; }=string.Empty;
    public bool UseTestRecipient { get; set; }=false;
    public string TestRecipient { get; set; }=string.Empty;
}
