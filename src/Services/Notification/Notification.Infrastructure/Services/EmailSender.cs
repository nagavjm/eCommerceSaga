using Microsoft.Extensions.Logging;
using Notification.Application.Common.Interfaces;

namespace Notification.Infrastructure.Services;

public class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(ILogger<EmailSender> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendEmailAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        // Fail-safe simulation for testing failure handling
        if (recipient.Contains("fail-delivery", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Email delivery failed for recipient: {Recipient}", recipient);
            return Task.FromResult(false);
        }

        _logger.LogInformation("Email sent successfully to {Recipient} with subject '{Subject}'", recipient, subject);
        return Task.FromResult(true);
    }
}
