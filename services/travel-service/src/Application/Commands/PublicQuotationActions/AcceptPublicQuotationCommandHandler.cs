using MediatR;
using TravelService.Application.Commands.AcceptQuotation;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.PublicQuotationActions;

public sealed class AcceptPublicQuotationCommandHandler(IQuotationShareLinkRepository shareLinkRepository, ISender sender) : IRequestHandler<AcceptPublicQuotationCommand, bool>
{
    public async Task<bool> Handle(AcceptPublicQuotationCommand request, CancellationToken cancellationToken)
    {
        var shareLink = await shareLinkRepository.GetActiveByTokenAsync(request.Token, cancellationToken);
        if (shareLink is null)
            return false;

        await sender.Send(new AcceptQuotationCommand(shareLink.TenantId, shareLink.QuotationId, shareLink.QuotationRevisionId, request.Reason), cancellationToken);
        return true;
    }
}
