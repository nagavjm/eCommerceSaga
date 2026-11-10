using BuildingBlocks.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Notification.Application.Common.Interfaces;
using Notification.Application.DTOs;

namespace Notification.Application.Queries;

public sealed record GetNotificationsByRecipientQuery(string RecipientEmail) : IRequest<Result<IReadOnlyList<NotificationDto>>>;

public class GetNotificationsByRecipientQueryHandler : IRequestHandler<GetNotificationsByRecipientQuery, Result<IReadOnlyList<NotificationDto>>>
{
    private readonly INotificationDbContext _dbContext;

    public GetNotificationsByRecipientQueryHandler(INotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyList<NotificationDto>>> Handle(GetNotificationsByRecipientQuery request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.RecipientEmail.Trim().ToLowerInvariant();

        var notifications = await _dbContext.Notifications
            .Where(n => n.Recipient == normalizedEmail)
            .OrderByDescending(n => n.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        IReadOnlyList<NotificationDto> dtos = notifications.Select(n => new NotificationDto(
            n.Id,
            n.Recipient,
            n.Subject,
            n.Body,
            n.Type.ToString(),
            n.Channel.ToString(),
            n.Status.ToString(),
            n.FailureReason,
            n.CreatedAtUtc,
            n.SentAtUtc)).ToList();

        return Result<IReadOnlyList<NotificationDto>>.Success(dtos);
    }
}

public sealed record GetNotificationByIdQuery(Guid Id) : IRequest<Result<NotificationDto>>;

public class GetNotificationByIdQueryHandler : IRequestHandler<GetNotificationByIdQuery, Result<NotificationDto>>
{
    private readonly INotificationDbContext _dbContext;

    public GetNotificationByIdQueryHandler(INotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<NotificationDto>> Handle(GetNotificationByIdQuery request, CancellationToken cancellationToken)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken);

        if (notification is null)
        {
            return Error.NotFound("Notification.NotFound", $"Notification with ID '{request.Id}' was not found.");
        }

        return new NotificationDto(
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
    }
}
