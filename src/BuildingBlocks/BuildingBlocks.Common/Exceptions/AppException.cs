using System.Net;

namespace BuildingBlocks.Common.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string title, string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        : base(message)
    {
        Title = title;
        StatusCode = statusCode;
    }

    public string Title { get; }
    public HttpStatusCode StatusCode { get; }
}

public class NotFoundException : AppException
{
    public NotFoundException(string resourceName, object key)
        : base("Not Found", $"Entity '{resourceName}' with key '{key}' was not found.", HttpStatusCode.NotFound)
    {
    }

    public NotFoundException(string message)
        : base("Not Found", message, HttpStatusCode.NotFound)
    {
    }
}

public class ValidationException : AppException
{
    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Validation Failure", "One or more validation errors occurred.", HttpStatusCode.BadRequest)
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}

public class ConflictException : AppException
{
    public ConflictException(string message)
        : base("Conflict", message, HttpStatusCode.Conflict)
    {
    }
}

public class DomainException : AppException
{
    public DomainException(string message)
        : base("Domain Rule Violation", message, HttpStatusCode.UnprocessableEntity)
    {
    }
}
