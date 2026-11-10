using BuildingBlocks.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.Commands;
using Notification.Application.DTOs;
using Notification.Application.Queries;
using Notification.Domain.Enums;

namespace Notification.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendNotification(
        [FromBody] SendNotificationRequest request,
        [FromHeader(Name = "X-Correlation-Id")] string? headerCorrelationId,
        CancellationToken cancellationToken)
    {
        Guid correlationId = Guid.Empty;
        if (!string.IsNullOrWhiteSpace(headerCorrelationId))
        {
            Guid.TryParse(headerCorrelationId, out correlationId);
        }

        if (!Enum.TryParse<NotificationType>(request.NotificationType, true, out var notifType))
        {
            notifType = NotificationType.OrderConfirmation;
        }

        if (!Enum.TryParse<NotificationChannel>(request.Channel, true, out var notifChannel))
        {
            notifChannel = NotificationChannel.Email;
        }

        var command = new SendNotificationCommand(
            request.RecipientEmail,
            request.Subject,
            request.Body,
            notifType,
            notifChannel,
            correlationId);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error.Code, message = result.Error.Description });
        }

        return Ok(result.Value);
    }

    [HttpGet("recipient/{email}")]
    public async Task<IActionResult> GetNotificationsByRecipient(string email, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetNotificationsByRecipientQuery(email), cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error.Code, message = result.Error.Description });
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetNotificationById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetNotificationByIdQuery(id), cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error.Code, message = result.Error.Description });
        }

        return Ok(result.Value);
    }
}
