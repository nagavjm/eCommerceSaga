using BuildingBlocks.Common.Pagination;
using BuildingBlocks.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Product.Application.Commands;
using Product.Application.DTOs;
using Product.Application.Queries;

namespace Product.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ISender _mediator;

    public ProductsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? searchTerm,
        [FromQuery] Guid? categoryId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetProductsQuery(searchTerm, categoryId, pageNumber, pageSize));
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));
        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error.Code, description = result.Error.Description });
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsFailure)
        {
            return result.Error.Type == ErrorType.Conflict
                ? Conflict(new { error = result.Error.Code, description = result.Error.Description })
                : BadRequest(new { error = result.Error.Code, description = result.Error.Description });
        }

        return CreatedAtAction(nameof(GetProductById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct([FromRoute] Guid id, [FromBody] UpdateProductCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID route parameter does not match command ID.");
        }

        var result = await _mediator.Send(command);
        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error.Code, description = result.Error.Description });
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new DeleteProductCommand(id));
        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error.Code, description = result.Error.Description });
        }

        return NoContent();
    }
}
