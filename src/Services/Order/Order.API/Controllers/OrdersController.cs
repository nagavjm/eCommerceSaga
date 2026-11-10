using System.Security.Claims;
using BuildingBlocks.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Order.Application.Commands;
using Order.Application.DTOs;
using Order.Application.Queries;

namespace Order.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        [FromHeader(Name = "X-User-Id")] string? headerUserId,
        [FromHeader(Name = "X-Correlation-Id")] string? headerCorrelationId,
        CancellationToken cancellationToken)
    {
        var customerId = GetCustomerId(headerUserId);
        if (customerId == Guid.Empty)
        {
            return Unauthorized(new { message = "User identity could not be determined." });
        }

        var customerEmail = User.FindFirstValue(ClaimTypes.Email) ?? "customer@example.com";

        Guid correlationId = Guid.Empty;
        if (!string.IsNullOrWhiteSpace(headerCorrelationId))
        {
            Guid.TryParse(headerCorrelationId, out correlationId);
        }

        var command = new CreateOrderCommand(
            customerId,
            customerEmail,
            request.ShippingAddress,
            request.Items,
            correlationId);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(new { error = result.Error.Code, message = result.Error.Description }),
                ErrorType.Conflict => Conflict(new { error = result.Error.Code, message = result.Error.Description }),
                _ => BadRequest(new { error = result.Error.Code, message = result.Error.Description })
            };
        }

        // Return 202 Accepted for async Saga processing
        return AcceptedAtAction(
            nameof(GetOrderById),
            new { id = result.Value.OrderId },
            result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error.Code, message = result.Error.Description });
        }

        return Ok(result.Value);
    }

    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders(
        [FromHeader(Name = "X-User-Id")] string? headerUserId,
        CancellationToken cancellationToken)
    {
        var customerId = GetCustomerId(headerUserId);
        if (customerId == Guid.Empty)
        {
            return Unauthorized(new { message = "User identity could not be determined." });
        }

        var result = await _mediator.Send(new GetCustomerOrdersQuery(customerId), cancellationToken);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> ConfirmOrder(
        Guid id,
        [FromHeader(Name = "X-Correlation-Id")] string? correlationHeader,
        CancellationToken cancellationToken)
    {
        Guid correlationId = Guid.Empty;
        if (!string.IsNullOrWhiteSpace(correlationHeader))
        {
            Guid.TryParse(correlationHeader, out correlationId);
        }

        var result = await _mediator.Send(new ConfirmOrderCommand(id, correlationId), cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error.Code, message = result.Error.Description });
        }

        return NoContent();
    }

    public sealed record CancelOrderRequestBody(string Reason);

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder(
        Guid id,
        [FromBody] CancelOrderRequestBody body,
        [FromHeader(Name = "X-Correlation-Id")] string? correlationHeader,
        CancellationToken cancellationToken)
    {
        Guid correlationId = Guid.Empty;
        if (!string.IsNullOrWhiteSpace(correlationHeader))
        {
            Guid.TryParse(correlationHeader, out correlationId);
        }

        var result = await _mediator.Send(new CancelOrderCommand(id, body.Reason, correlationId), cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error.Code, message = result.Error.Description });
        }

        return NoContent();
    }

    private Guid GetCustomerId(string? headerUserId)
    {
        if (!string.IsNullOrWhiteSpace(headerUserId) && Guid.TryParse(headerUserId, out var parsedFromHeader))
        {
            return parsedFromHeader;
        }

        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(claimValue) && Guid.TryParse(claimValue, out var parsedFromClaim))
        {
            return parsedFromClaim;
        }

        return Guid.Empty;
    }
}
