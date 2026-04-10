using CommunicationService.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CommunicationService.Api.Filters;

public sealed class GlobalExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var (status, payload) = context.Exception switch
        {
            DomainException domainException when IsEntitlementDenied(domainException.Message)
                => (StatusCodes.Status403Forbidden, new { code = "entitlement_denied", message = domainException.Message }),
            DomainException domainException
                => (StatusCodes.Status400BadRequest, (object)new ProblemDetails { Status = StatusCodes.Status400BadRequest, Detail = domainException.Message }),
            _ => (StatusCodes.Status500InternalServerError, (object)new ProblemDetails { Status = StatusCodes.Status500InternalServerError, Detail = context.Exception.Message })
        };

        context.Result = new ObjectResult(payload) { StatusCode = status };
        context.ExceptionHandled = true;
    }

    private static bool IsEntitlementDenied(string message)
        => message.Contains("not enabled", StringComparison.OrdinalIgnoreCase)
           || message.Contains("subscription", StringComparison.OrdinalIgnoreCase)
           || message.Contains("entitlement", StringComparison.OrdinalIgnoreCase);
}
