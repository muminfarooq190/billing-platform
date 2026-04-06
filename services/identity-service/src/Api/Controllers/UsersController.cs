using IdentityService.Api.Contracts;
using IdentityService.Application.Commands.CreateUser;
using IdentityService.Application.Commands.DeleteUser;
using IdentityService.Application.Commands.UpdateUser;
using IdentityService.Application.Queries.GetUserById;
using IdentityService.Application.Queries.GetUsersByTenant;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("identity/users")]
public sealed class UsersController(IMediator mediator, ITenantContext tenantContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new CreateUserCommand(tenantContext.TenantId, request.Email, request.Password, request.Role), cancellationToken);
        var user = await mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { userId = id }, user);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var users = await mediator.Send(new GetUsersByTenantQuery(tenantContext.TenantId), cancellationToken);
        return Ok(users);
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetById(Guid userId, CancellationToken cancellationToken)
    {
        var user = await mediator.Send(new GetUserByIdQuery(userId), cancellationToken);
        return Ok(user);
    }

    [HttpPut("{userId:guid}")]
    public async Task<IActionResult> Update(Guid userId, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateUserCommand(userId, request.Role, request.Password), cancellationToken);
        var user = await mediator.Send(new GetUserByIdQuery(userId), cancellationToken);
        return Ok(user);
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Delete(Guid userId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteUserCommand(userId), cancellationToken);
        return NoContent();
    }
}
