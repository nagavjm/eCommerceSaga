using System.Net;
using System.Text.Json;
using BuildingBlocks.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Observability.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var problemDetails = new ProblemDetails
        {
            Instance = context.Request.Path,
            Detail = exception.Message
        };

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                problemDetails.Title = "Validation Error";
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Extensions["errors"] = validationException.Errors;
                break;

            case NotFoundException notFoundException:
                statusCode = notFoundException.StatusCode;
                problemDetails.Title = notFoundException.Title;
                problemDetails.Status = (int)notFoundException.StatusCode;
                break;

            case ConflictException conflictException:
                statusCode = conflictException.StatusCode;
                problemDetails.Title = conflictException.Title;
                problemDetails.Status = (int)conflictException.StatusCode;
                break;

            case DomainException domainException:
                statusCode = domainException.StatusCode;
                problemDetails.Title = domainException.Title;
                problemDetails.Status = (int)domainException.StatusCode;
                break;

            default:
                problemDetails.Title = "An unexpected internal server error occurred.";
                problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                break;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(problemDetails);
        await context.Response.WriteAsync(json);
    }
}
