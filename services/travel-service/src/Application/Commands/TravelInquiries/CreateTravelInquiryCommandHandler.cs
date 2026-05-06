using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.TravelInquiries;

public sealed class CreateTravelInquiryCommandHandler(
    ITravelInquiryRepository inquiryRepository,
    IActivityWriter activityWriter,
    IAuditWriter auditWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateTravelInquiryCommand, Guid>
{
    public async Task<Guid> Handle(CreateTravelInquiryCommand request, CancellationToken cancellationToken)
    {
        var inquiry = TravelInquiry.Create(
            request.TenantId,
            request.Source,
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
            request.CustomerMessage);

        if (request.AssignedToUserId.HasValue)
            inquiry.Assign(request.AssignedToUserId);

        await inquiryRepository.AddAsync(inquiry, cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                request.TenantId,
                "TravelInquiry",
                inquiry.Id,
                "TravelInquiryCreated",
                $"Inquiry created for {inquiry.FullName}",
                new { inquiry.Destination, inquiry.Source },
                actorContext.UserId),
            cancellationToken);
        await auditWriter.WriteAsync(
            AuditLog.Create(
                request.TenantId,
                "TravelInquiry",
                inquiry.Id,
                "TravelInquiryCreated",
                actorContext.UserId,
                actorContext.IpAddress,
                actorContext.UserAgent,
                null,
                new { inquiry.FullName, inquiry.Destination, inquiry.Source, inquiry.AssignedToUserId },
                null),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return inquiry.Id;
    }
}
