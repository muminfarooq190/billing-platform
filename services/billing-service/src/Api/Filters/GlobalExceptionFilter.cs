using BillingService.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BillingService.Api.Filters;

public sealed class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var status = context.Exception is DomainException ? 400 : 500;
        if (status == 500)
        {
            logger.LogError(context.Exception, "Unhandled exception on {Method} {Path}",
                context.HttpContext.Request.Method, context.HttpContext.Request.Path);
        }
        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = status,
            Detail = context.Exception.Message,
            Title = context.Exception.GetType().Name,
        }) { StatusCode = status };
        context.ExceptionHandled = true;
    }
}
