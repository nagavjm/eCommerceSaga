using System.Security.Claims;
using Cart.Application.Commands;
using Cart.Application.DTOs;
using Cart.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cart.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ISender _mediator;

    public CartController(ISender mediator)
    {
        _mediator = mediator;
    }

    private Guid GetEffectiveCustomerId(Guid? requestedCustomerId = null)
    {
        if (HttpContext.Request.Headers.TryGetValue("X-User-Id", out var headerId) &&
            Guid.TryParse(headerId.FirstOrDefault(), out var parsedHeaderId))
        {
            return parsedHeaderId;
        }

        var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("sub")?.Value;

        if (!string.IsNullOrEmpty(claimId) && Guid.TryParse(claimId, out var parsedClaimId))
        {
            return parsedClaimId;
        }

        return requestedCustomerId ?? Guid.Parse("00000000-0000-0000-0000-000000000001");
    }

    [HttpGet]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCart([FromQuery] Guid? customerId)
    {
        var targetCustomerId = GetEffectiveCustomerId(customerId);
        var result = await _mediator.Send(new GetCartByCustomerIdQuery(targetCustomerId));
        return Ok(result.Value);
    }

    [HttpPost("items")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddItem([FromBody] AddItemRequest request)
    {
        var targetCustomerId = GetEffectiveCustomerId(request.CustomerId);

        var command = new AddItemToCartCommand(
            targetCustomerId,
            request.ProductId,
            request.ProductName,
            request.UnitPrice,
            request.Quantity,
            request.ImageUrl);

        var result = await _mediator.Send(command);
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Code, description = result.Error.Description });
        }

        return Ok(result.Value);
    }

    [HttpPut("items/{productId:guid}")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItemQuantity(
        [FromRoute] Guid productId,
        [FromBody] UpdateQuantityRequest request)
    {
        var targetCustomerId = GetEffectiveCustomerId(request.CustomerId);

        var result = await _mediator.Send(new UpdateCartItemQuantityCommand(
            targetCustomerId,
            productId,
            request.Quantity));

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error.Code, description = result.Error.Description });
        }

        return Ok(result.Value);
    }

    [HttpDelete("items/{productId:guid}")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(
        [FromRoute] Guid productId,
        [FromQuery] Guid? customerId)
    {
        var targetCustomerId = GetEffectiveCustomerId(customerId);

        var result = await _mediator.Send(new RemoveCartItemCommand(targetCustomerId, productId));
        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error.Code, description = result.Error.Description });
        }

        return Ok(result.Value);
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ClearCart([FromQuery] Guid? customerId)
    {
        var targetCustomerId = GetEffectiveCustomerId(customerId);
        await _mediator.Send(new ClearCartCommand(targetCustomerId));
        return NoContent();
    }
}

public sealed record AddItemRequest(
    Guid? CustomerId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    string ImageUrl);

public sealed record UpdateQuantityRequest(
    Guid? CustomerId,
    int Quantity);
