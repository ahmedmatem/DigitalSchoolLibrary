namespace SchoolLibrary.Application.Common.Interfaces;
public interface IEmailSender
{
    Task SendPasswordResetAsync(string recipientEmail,string resetUrl,CancellationToken cancellationToken=default);
}
