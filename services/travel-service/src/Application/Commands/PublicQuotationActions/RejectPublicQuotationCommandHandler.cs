using MediatR;
using TravelService.Application.Commands.RejectQuotation;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.PublicQuotationActions;

public sealed class RejectPublicQuotationCommandHandler(IQuotationShareLinkRepository shareLinkRepository, ISender sender) : IRequestHandler<RejectPublicQuotationCommand, bool>
{
    public async Task<bool> Handle(RejectPublicQuotationCommand request, CancellationToken cancellationToken)
    {
        var shareLink = await shareLinkRepository.GetActiveByTokenAsync(request.Token, cancellationToken);
        if (shareLink is null)
            return false;

        await sender.Send(new RejectQuotationCommand(shareLink.TenantId, shareLink.QuotationId, request.Reason), cancellationToken);
        return true;
    }
}
