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
    IDraftTripConceptRepository conceptRepository,
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

        DraftTripConcept? concept = null;
        if (request.ConceptId.HasValue)
        {
            concept = await conceptRepository.GetByIdAsync(request.ConceptId.Value, cancellationToken)
                ?? throw new DomainException($"Draft trip concept {request.ConceptId.Value} not found.");

            if (concept.TenantId != request.TenantId || concept.TravelInquiryId != request.InquiryId)
                throw new DomainException("Draft trip concept does not belong to the inquiry context.");
        }

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

        var quotationTitle = string.IsNullOrWhiteSpace(request.QuotationTitle)
            ? concept?.Title ?? inquiry.Destination
            : request.QuotationTitle.Trim();
        var destination = concept?.Destination ?? inquiry.Destination;
        var travelDate = concept?.StartDate ?? inquiry.TravelDate ?? DateTimeOffset.UtcNow.AddDays(30);
        var returnDate = concept?.EndDate ?? inquiry.ReturnDate ?? travelDate.AddDays(5);
        var travellers = concept?.Travellers ?? inquiry.Travellers ?? 1;
        var currency = !string.IsNullOrWhiteSpace(request.Currency)
            ? request.Currency
            : concept?.Currency ?? inquiry.BudgetCurrency ?? "USD";
        var quotationNotes = string.Join(Environment.NewLine, new[]
        {
            string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes?.Trim(),
            $"Created from inquiry {inquiry.Id} ({inquiry.Source}).",
            concept is null ? null : $"Seeded from draft concept {concept.Id} ({concept.Title}).",
            string.IsNullOrWhiteSpace(concept?.Summary) ? null : $"Concept summary: {concept!.Summary}",
            string.IsNullOrWhiteSpace(concept?.Notes) ? null : $"Concept notes: {concept!.Notes}",
            string.IsNullOrWhiteSpace(inquiry.CustomerMessage) ? null : $"Customer message: {inquiry.CustomerMessage}",
            (concept?.BudgetAmount ?? inquiry.BudgetAmount).HasValue ? $"Indicative budget: {(concept?.BudgetAmount ?? inquiry.BudgetAmount)!.Value} {concept?.Currency ?? inquiry.BudgetCurrency}" : null
        }.Where(x => !string.IsNullOrWhiteSpace(x)));

        var quotation = Quotation.Create(
            request.TenantId,
            contact.Id,
            inquiry.FullName,
            quotationTitle,
            destination,
            travelDate,
            returnDate,
            travellers,
            currency,
            quotationNotes);

        quotation.AddLineItem("Trip proposal to be finalized", 0m, 1, currency);

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
                new { ContactId = contact.Id, QuotationId = quotation.Id, ConceptId = concept?.Id, inquiry.AssignedToUserId },
                actorContext.UserId),
            cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                quotation.TenantId,
                "Quotation",
                quotation.Id,
                "Created",
                $"Quotation created from inquiry {inquiry.Id}",
                new { InquiryId = inquiry.Id, ContactId = contact.Id, ConceptId = concept?.Id },
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
                new { Status = inquiry.Status.ToString(), ContactId = contact.Id, QuotationId = quotation.Id, ConceptId = concept?.Id },
                new { QuotationTitle = quotationTitle, Currency = currency }),
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
