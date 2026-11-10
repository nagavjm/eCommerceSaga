using BuildingBlocks.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return result.Error.Type switch
        {
            ErrorType.NotFound => NotFound(new { error = result.Error.Code, description = result.Error.Description }),
            ErrorType.Validation => BadRequest(new { error = result.Error.Code, description = result.Error.Description }),
            ErrorType.Conflict => Conflict(new { error = result.Error.Code, description = result.Error.Description }),
            ErrorType.Unauthorized => Unauthorized(new { error = result.Error.Code, description = result.Error.Description }),
            ErrorType.Forbidden => Forbid(),
            _ => BadRequest(new { error = result.Error.Code, description = result.Error.Description })
        };
    }
}
