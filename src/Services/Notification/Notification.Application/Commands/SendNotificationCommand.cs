using BuildingBlocks.Common.Results;
using FluentValidation;
using MediatR;
using Notification.Application.Common.Interfaces;
using Notification.Application.DTOs;
using Notification.Domain.Entities;
using Notification.Domain.Enums;

namespace Notification.Application.Commands;

public sealed record SendNotificationCommand(
    string Recipient,
    string Subject,
    string Body,
    NotificationType Type,
    NotificationChannel Channel,
    Guid CorrelationId) : IRequest<Result<NotificationDto>>;

public class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationCommandValidator()
    {
        RuleFor(x => x.Recipient).NotEmpty().EmailAddress().WithMessage("Valid recipient email is required.");
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200).WithMessage("Subject is required.");
        RuleFor(x => x.Body).NotEmpty().WithMessage("Body is required.");
    }
}

public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, Result<NotificationDto>>
{
    private readonly INotificationDbContext _dbContext;
    private readonly IEmailSender _emailSender;

    public SendNotificationCommandHandler(
        INotificationDbContext dbContext,
        IEmailSender emailSender)
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
    }

    public async Task<Result<NotificationDto>> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        var correlationId = request.CorrelationId == Guid.Empty ? Guid.NewGuid() : request.CorrelationId;

        var notification = NotificationLog.Create(
            request.Recipient,
            request.Subject,
            request.Body,
            request.Type,
            request.Channel,
            correlationId);

        _dbContext.Notifications.Add(notification);

        // Dispatch email
        var sendSuccess = await _emailSender.SendEmailAsync(
            request.Recipient,
            request.Subject,
            request.Body,
            cancellationToken);

        if (sendSuccess)
        {
            notification.MarkAsSent();
        }
        else
        {
            notification.MarkAsFailed("Failed to deliver message via notification channel provider.");
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = new NotificationDto(
            notification.Id,
            notification.Recipient,
            notification.Subject,
            notification.Body,
            notification.Type.ToString(),
            notification.Channel.ToString(),
            notification.Status.ToString(),
            notification.FailureReason,
            notification.CreatedAtUtc,
            notification.SentAtUtc);

        return dto;
    }
}
