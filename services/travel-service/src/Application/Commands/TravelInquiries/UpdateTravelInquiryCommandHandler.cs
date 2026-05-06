using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.TravelInquiries;

public sealed class UpdateTravelInquiryCommandHandler(
    ITravelInquiryRepository inquiryRepository,
    IActivityWriter activityWriter,
    IAuditWriter auditWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateTravelInquiryCommand>
{
    public async Task Handle(UpdateTravelInquiryCommand request, CancellationToken cancellationToken)
    {
        var inquiry = await inquiryRepository.GetByIdAsync(request.InquiryId, cancellationToken)
            ?? throw new InvalidOperationException($"Inquiry {request.InquiryId} was not found.");

        if (inquiry.TenantId != request.TenantId)
            throw new InvalidOperationException("Inquiry does not belong to tenant context.");

        inquiry.UpdateDetails(
            request.FullName,
            request.Email,
            request.Phone,
            request.WhatsappNumber,
            request.DepartureCity,
            request.Destination,
            request.TravelDate,
            request.ReturnDate,
            request.IsDateFlexible,
            request.Travellers,
            request.BudgetAmount,
            request.BudgetCurrency,
            request.CustomerMessage,
            request.AssignedToUserId);

        await inquiryRepository.UpdateAsync(inquiry, cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                request.TenantId,
                "TravelInquiry",
                inquiry.Id,
                "TravelInquiryUpdated",
                $"Inquiry updated for {inquiry.FullName}",
                new { inquiry.Destination, inquiry.AssignedToUserId },
                actorContext.UserId),
            cancellationToken);
        await auditWriter.WriteAsync(
            AuditLog.Create(
                request.TenantId,
                "TravelInquiry",
                inquiry.Id,
                "TravelInquiryUpdated",
                actorContext.UserId,
                actorContext.IpAddress,
                actorContext.UserAgent,
                null,
                new { inquiry.FullName, inquiry.Destination, inquiry.AssignedToUserId },
                null),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
