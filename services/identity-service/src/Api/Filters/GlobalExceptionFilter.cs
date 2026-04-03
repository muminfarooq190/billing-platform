using IdentityService.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IdentityService.Api.Filters;

public sealed class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var (statusCode, title) = context.Exception switch
        {
            DomainException => (StatusCodes.Status400BadRequest, "Domain validation failed"),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            UnauthorizedException => (StatusCodes.Status403Forbidden, "Unauthorized"),
            _ => (StatusCodes.Status500InternalServerError, "Unhandled error")
        };

        logger.LogError(context.Exception, "Request failed with {StatusCode}", statusCode);
        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = context.Exception.Message
        })
        {
            StatusCode = statusCode
        };
        context.ExceptionHandled = true;
    }
}
