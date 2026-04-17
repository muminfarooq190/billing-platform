using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.TravelInquiries;

public sealed class CreatePublicInquiryCommandHandler(
    ITravelInquiryRepository inquiryRepository,
    ITravelInquiryStatusHistoryRepository historyRepository,
    IActivityWriter activityWriter,
    IAuditWriter auditWriter,
    IUnitOfWork unitOfWork) : IRequestHandler<CreatePublicInquiryCommand, Guid>
{
    public async Task<Guid> Handle(CreatePublicInquiryCommand request, CancellationToken cancellationToken)
    {
        var inquiry = TravelInquiry.Create(
            request.TenantId,
            string.IsNullOrWhiteSpace(request.Source) ? "Website" : request.Source,
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
            request.Message);

        await inquiryRepository.AddAsync(inquiry, cancellationToken);
        await historyRepository.AddAsync(
            TravelInquiryStatusHistory.Create(inquiry.Id, inquiry.TenantId, null, inquiry.Status.ToString(), "Public inquiry received.", null),
            cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                inquiry.TenantId,
                "TravelInquiry",
                inquiry.Id,
                "Created",
                "Public travel inquiry received",
                new { inquiry.Source, inquiry.Destination, inquiry.TravelDate, inquiry.ReturnDate, inquiry.Travellers },
                null),
            cancellationToken);
        await auditWriter.WriteAsync(
            AuditLog.Create(
                inquiry.TenantId,
                "TravelInquiry",
                inquiry.Id,
                "PublicInquiryCreated",
                null,
                request.IpAddress,
                request.UserAgent,
                null,
                new { inquiry.Source, inquiry.Destination, inquiry.TravelDate, inquiry.ReturnDate, inquiry.Travellers },
                new { request.Email, request.Phone, request.WhatsappNumber }),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return inquiry.Id;
    }
}
