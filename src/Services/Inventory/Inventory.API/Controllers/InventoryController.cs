using Inventory.Application.Commands;
using Inventory.Application.DTOs;
using Inventory.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly ISender _mediator;

    public InventoryController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<InventoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllStock()
    {
        var result = await _mediator.Send(new GetAllStockQuery());
        return Ok(result.Value);
    }

    [HttpGet("{productId:guid}")]
    [ProducesResponseType(typeof(InventoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStockByProductId([FromRoute] Guid productId)
    {
        var result = await _mediator.Send(new GetStockByProductIdQuery(productId));
        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error.Code, description = result.Error.Description });
        }

        return Ok(result.Value);
    }

    [HttpPost("reserve")]
    [ProducesResponseType(typeof(StockReservationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReserveStock([FromBody] ReserveStockCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsFailure || !result.Value.IsSuccessful)
        {
            return BadRequest(new
            {
                success = false,
                reason = result.IsFailure ? result.Error.Description : result.Value.FailureReason
            });
        }

        return Ok(result.Value);
    }

    [HttpPost("release")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReleaseStock([FromBody] ReleaseStockCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { success = result.IsSuccess });
    }

    [HttpPost("commit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CommitStockDeduction([FromBody] CommitStockDeductionCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { success = result.IsSuccess });
    }

    [HttpPost("restock")]
    [ProducesResponseType(typeof(InventoryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Restock([FromBody] RestockCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result.Value);
    }
}
