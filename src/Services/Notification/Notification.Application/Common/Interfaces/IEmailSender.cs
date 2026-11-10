namespace Notification.Application.Common.Interfaces;

public interface IEmailSender
{
    Task<bool> SendEmailAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken);
}
