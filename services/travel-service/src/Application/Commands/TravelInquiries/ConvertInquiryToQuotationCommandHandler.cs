using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.TravelInquiries;

public sealed class ConvertInquiryToQuotationCommandHandler(
    ITravelInquiryRepository inquiryRepository,
    ITravelInquiryStatusHistoryRepository historyRepository,
    IContactRepository contactRepository,
    IQuotationRepository quotationRepository,
    IActivityWriter activityWriter,
    IAuditWriter auditWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<ConvertInquiryToQuotationCommand, ConvertInquiryToQuotationResult>
{
    public async Task<ConvertInquiryToQuotationResult> Handle(ConvertInquiryToQuotationCommand request, CancellationToken cancellationToken)
    {
        var inquiry = await TravelInquiryCommandHandlerSupport.LoadInquiryAsync(inquiryRepository, request.TenantId, request.InquiryId, cancellationToken);

        if (inquiry.ConvertedAt.HasValue)
            throw new DomainException("Inquiry has already been converted.");

        Contact contact;
        if (request.ContactId.HasValue)
        {
            contact = await contactRepository.GetByIdAsync(request.ContactId.Value, cancellationToken)
                ?? throw new DomainException($"Contact {request.ContactId.Value} not found.");

            if (contact.TenantId != request.TenantId)
                throw new DomainException("Contact does not belong to the active tenant.");
        }
        else
        {
            if (!request.CreateContactIfMissing)
                throw new DomainException("ContactId is required when CreateContactIfMissing is false.");

            var (firstName, lastName) = SplitName(inquiry.FullName);
            var contactNotes = string.Join(Environment.NewLine, new[]
            {
                string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes?.Trim(),
                string.IsNullOrWhiteSpace(inquiry.CustomerMessage) ? null : $"Inquiry message: {inquiry.CustomerMessage}"
            }.Where(x => !string.IsNullOrWhiteSpace(x)));

            contact = Contact.Create(request.TenantId, firstName, lastName, inquiry.Email, inquiry.Phone ?? inquiry.WhatsappNumber, null, contactNotes, null);
            await contactRepository.AddAsync(contact, cancellationToken);
        }

        var travelDate = inquiry.TravelDate ?? DateTimeOffset.UtcNow.AddDays(30);
        var returnDate = inquiry.ReturnDate ?? travelDate.AddDays(5);
        var quotationNotes = string.Join(Environment.NewLine, new[]
        {
            string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes?.Trim(),
            $"Created from inquiry {inquiry.Id} ({inquiry.Source}).",
            string.IsNullOrWhiteSpace(inquiry.CustomerMessage) ? null : $"Customer message: {inquiry.CustomerMessage}",
            inquiry.BudgetAmount.HasValue ? $"Indicative budget: {inquiry.BudgetAmount.Value} {inquiry.BudgetCurrency}" : null
        }.Where(x => !string.IsNullOrWhiteSpace(x)));

        var quotation = Quotation.Create(
            request.TenantId,
            contact.Id,
            inquiry.FullName,
            request.QuotationTitle,
            inquiry.Destination,
            travelDate,
            returnDate,
            inquiry.Travellers ?? 1,
            request.Currency,
            quotationNotes);

        quotation.AddLineItem("Trip proposal to be finalized", 0m, 1, request.Currency);

        await quotationRepository.AddAsync(quotation, cancellationToken);

        var previousStatus = inquiry.Status.ToString();
        inquiry.Assign(request.AssignedToUserId ?? inquiry.AssignedToUserId);
        inquiry.MarkQuoted(contact.Id, quotation.Id);
        await inquiryRepository.UpdateAsync(inquiry, cancellationToken);
        await historyRepository.AddAsync(
            TravelInquiryStatusHistory.Create(inquiry.Id, inquiry.TenantId, previousStatus, inquiry.Status.ToString(), "Converted to quotation.", actorContext.UserId),
            cancellationToken);

        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                inquiry.TenantId,
                "TravelInquiry",
                inquiry.Id,
                "Converted",
                $"Inquiry converted to quotation {quotation.Id}",
                new { ContactId = contact.Id, QuotationId = quotation.Id, inquiry.AssignedToUserId },
                actorContext.UserId),
            cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                quotation.TenantId,
                "Quotation",
                quotation.Id,
                "Created",
                $"Quotation created from inquiry {inquiry.Id}",
                new { InquiryId = inquiry.Id, ContactId = contact.Id },
                actorContext.UserId),
            cancellationToken);
        await auditWriter.WriteAsync(
            AuditLog.Create(
                inquiry.TenantId,
                "TravelInquiry",
                inquiry.Id,
                "InquiryConvertedToQuotation",
                actorContext.UserId,
                actorContext.IpAddress,
                actorContext.UserAgent,
                new { Status = previousStatus },
                new { Status = inquiry.Status.ToString(), ContactId = contact.Id, QuotationId = quotation.Id },
                new { request.QuotationTitle, request.Currency }),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new ConvertInquiryToQuotationResult(inquiry.Id, contact.Id, quotation.Id);
    }

    private static (string FirstName, string LastName) SplitName(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
            return ("Customer", "Inquiry");
        if (parts.Length == 1)
            return (parts[0], "Inquiry");

        return (parts[0], string.Join(' ', parts.Skip(1)));
    }
}
