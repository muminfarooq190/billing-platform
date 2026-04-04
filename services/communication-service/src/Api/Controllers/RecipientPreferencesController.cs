using CommunicationService.Api.Contracts;
using CommunicationService.Application.Commands.UpdateRecipientPreferences;
using CommunicationService.Application.Queries.GetRecipientPreferences;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CommunicationService.Api.Controllers;

[ApiController]
[Route("communication/recipient-preferences")]
public sealed class RecipientPreferencesController(IMediator mediator) : ControllerBase
{
    [HttpPut]
    public async Task<IActionResult> Upsert([FromBody] UpdateRecipientPreferencesRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new UpdateRecipientPreferencesCommand(
            request.TenantId,
            request.RecipientId,
            request.RecipientType,
            request.Email,
            request.Phone,
            request.DeviceToken,
            request.Timezone,
            request.ChannelPreferences?.Select(x => new ChannelPreferenceDto(x.Channel, x.Enabled, x.QuietHoursEnabled, x.QuietStart, x.QuietEnd)).ToList()), cancellationToken);

        return Ok(new { id });
    }

    [HttpGet("{tenantId:guid}/{recipientId:guid}")]
    public async Task<IActionResult> Get(Guid tenantId, Guid recipientId, CancellationToken cancellationToken)
    {
        var model = await mediator.Send(new GetRecipientPreferencesQuery(recipientId, tenantId), cancellationToken);
        return model is null ? NotFound() : Ok(model);
    }
}
