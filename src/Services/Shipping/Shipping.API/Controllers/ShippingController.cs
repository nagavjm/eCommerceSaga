using System.Security.Claims;
using BuildingBlocks.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shipping.Application.Commands;
using Shipping.Application.DTOs;
using Shipping.Application.Queries;
using Shipping.Domain.Enums;

namespace Shipping.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShippingController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShippingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateShipment(
        [FromBody] CreateShipmentRequest request,
        [FromHeader(Name = "X-User-Id")] string? headerUserId,
        [FromHeader(Name = "X-Correlation-Id")] string? headerCorrelationId,
        CancellationToken cancellationToken)
    {
        var customerId = request.CustomerId;
        if (customerId == Guid.Empty && !string.IsNullOrWhiteSpace(headerUserId) && Guid.TryParse(headerUserId, out var parsedId))
        {
            customerId = parsedId;
        }

        Guid correlationId = Guid.Empty;
        if (!string.IsNullOrWhiteSpace(headerCorrelationId))
        {
            Guid.TryParse(headerCorrelationId, out correlationId);
        }

        var command = new CreateShipmentCommand(
            request.OrderId,
            customerId,
            request.DeliveryAddress,
            request.Carrier,
            request.Items,
            correlationId);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error.Code, message = result.Error.Description });
        }

        return Ok(result.Value);
    }

    [HttpGet("order/{orderId:guid}")]
    public async Task<IActionResult> GetShipmentByOrderId(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetShipmentByOrderIdQuery(orderId), cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error.Code, message = result.Error.Description });
        }

        return Ok(result.Value);
    }

    [HttpGet("track/{trackingNumber}")]
    public async Task<IActionResult> TrackShipment(string trackingNumber, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetShipmentByTrackingNumberQuery(trackingNumber), cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error.Code, message = result.Error.Description });
        }

        return Ok(result.Value);
    }

    public sealed record UpdateStatusRequest(ShipmentStatus Status, string? FailureReason);

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateShipmentStatus(
        Guid id,
        [FromBody] UpdateStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateShipmentStatusCommand(id, request.Status, request.FailureReason);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(new { error = result.Error.Code, message = result.Error.Description }),
                _ => BadRequest(new { error = result.Error.Code, message = result.Error.Description })
            };
        }

        return NoContent();
    }
}
