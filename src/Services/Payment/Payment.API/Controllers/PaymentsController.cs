using System.Security.Claims;
using BuildingBlocks.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Commands;
using Payment.Application.DTOs;
using Payment.Application.Queries;

namespace Payment.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment(
        [FromBody] ProcessPaymentRequest request,
        [FromHeader(Name = "X-User-Id")] string? headerUserId,
        [FromHeader(Name = "X-Correlation-Id")] string? headerCorrelationId,
        CancellationToken cancellationToken)
    {
        var customerId = GetCustomerId(headerUserId);
        if (customerId == Guid.Empty)
        {
            customerId = Guid.NewGuid(); // Allow service-to-service / saga invocation
        }

        Guid correlationId = Guid.Empty;
        if (!string.IsNullOrWhiteSpace(headerCorrelationId))
        {
            Guid.TryParse(headerCorrelationId, out correlationId);
        }

        var command = new ProcessPaymentCommand(
            request.OrderId,
            customerId,
            request.Amount,
            request.Currency,
            correlationId);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error.Code, message = result.Error.Description });
        }

        return Ok(result.Value);
    }

    [HttpPost("{orderId:guid}/refund")]
    public async Task<IActionResult> RefundPayment(
        Guid orderId,
        [FromBody] RefundPaymentRequest request,
        [FromHeader(Name = "X-Correlation-Id")] string? headerCorrelationId,
        CancellationToken cancellationToken)
    {
        Guid correlationId = Guid.Empty;
        if (!string.IsNullOrWhiteSpace(headerCorrelationId))
        {
            Guid.TryParse(headerCorrelationId, out correlationId);
        }

        var command = new RefundPaymentCommand(orderId, request.Amount, correlationId);
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

    [HttpGet("order/{orderId:guid}")]
    public async Task<IActionResult> GetPaymentByOrderId(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPaymentByOrderIdQuery(orderId), cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error.Code, message = result.Error.Description });
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPaymentById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPaymentByIdQuery(id), cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error.Code, message = result.Error.Description });
        }

        return Ok(result.Value);
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
