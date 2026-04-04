using CommunicationService.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CommunicationService.Api.Filters;

public sealed class GlobalExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var status = context.Exception is DomainException ? 400 : 500;
        context.Result = new ObjectResult(new ProblemDetails { Status = status, Detail = context.Exception.Message }) { StatusCode = status };
        context.ExceptionHandled = true;
    }
}
