using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.TravelInquiries;

internal static class TravelInquiryCommandHandlerSupport
{
    public static async Task<TravelInquiry> LoadInquiryAsync(ITravelInquiryRepository repository, Guid tenantId, Guid inquiryId, CancellationToken cancellationToken)
    {
        var inquiry = await repository.GetByIdAsync(inquiryId, cancellationToken)
            ?? throw new DomainException($"Inquiry {inquiryId} not found.");

        if (inquiry.TenantId != tenantId)
            throw new DomainException("Inquiry does not belong to tenant.");

        return inquiry;
    }

    public static async Task PersistStatusChangeAsync(
        TravelInquiry inquiry,
        string? previousStatus,
        string? reason,
        ITravelInquiryRepository inquiryRepository,
        ITravelInquiryStatusHistoryRepository historyRepository,
        IActivityWriter activityWriter,
        IAuditWriter auditWriter,
        IActorContext actorContext,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        await inquiryRepository.UpdateAsync(inquiry, cancellationToken);
        await historyRepository.AddAsync(
            TravelInquiryStatusHistory.Create(inquiry.Id, inquiry.TenantId, previousStatus, inquiry.Status.ToString(), reason, actorContext.UserId),
            cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                inquiry.TenantId,
                "TravelInquiry",
                inquiry.Id,
                "StatusChanged",
                $"Inquiry status changed to {inquiry.Status}",
                new { FromStatus = previousStatus, ToStatus = inquiry.Status.ToString(), Reason = reason, inquiry.AssignedToUserId },
                actorContext.UserId),
            cancellationToken);
        await auditWriter.WriteAsync(
            AuditLog.Create(
                inquiry.TenantId,
                "TravelInquiry",
                inquiry.Id,
                $"Inquiry{inquiry.Status}",
                actorContext.UserId,
                actorContext.IpAddress,
                actorContext.UserAgent,
                new { Status = previousStatus },
                new { Status = inquiry.Status.ToString(), inquiry.AssignedToUserId },
                new { Reason = reason }),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class AssignInquiryCommandHandler(
    ITravelInquiryRepository inquiryRepository,
    IActivityWriter activityWriter,
    IAuditWriter auditWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<AssignInquiryCommand>
{
    public async Task Handle(AssignInquiryCommand request, CancellationToken cancellationToken)
    {
        var inquiry = await TravelInquiryCommandHandlerSupport.LoadInquiryAsync(inquiryRepository, request.TenantId, request.InquiryId, cancellationToken);
        var previousAssignee = inquiry.AssignedToUserId;
        inquiry.Assign(request.AssignedToUserId);
        await inquiryRepository.UpdateAsync(inquiry, cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                inquiry.TenantId,
                "TravelInquiry",
                inquiry.Id,
                "Assigned",
                request.AssignedToUserId.HasValue ? "Inquiry assigned" : "Inquiry unassigned",
                new { PreviousAssignee = previousAssignee, inquiry.AssignedToUserId },
                actorContext.UserId),
            cancellationToken);
        await auditWriter.WriteAsync(
            AuditLog.Create(
                inquiry.TenantId,
                "TravelInquiry",
                inquiry.Id,
                "InquiryAssigned",
                actorContext.UserId,
                actorContext.IpAddress,
                actorContext.UserAgent,
                new { AssignedToUserId = previousAssignee },
                new { inquiry.AssignedToUserId }),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class QualifyInquiryCommandHandler(
    ITravelInquiryRepository inquiryRepository,
    ITravelInquiryStatusHistoryRepository historyRepository,
    IActivityWriter activityWriter,
    IAuditWriter auditWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<QualifyInquiryCommand>
{
    public async Task Handle(QualifyInquiryCommand request, CancellationToken cancellationToken)
    {
        var inquiry = await TravelInquiryCommandHandlerSupport.LoadInquiryAsync(inquiryRepository, request.TenantId, request.InquiryId, cancellationToken);
        var previousStatus = inquiry.Status.ToString();
        inquiry.Qualify();
        await TravelInquiryCommandHandlerSupport.PersistStatusChangeAsync(inquiry, previousStatus, request.Reason, inquiryRepository, historyRepository, activityWriter, auditWriter, actorContext, unitOfWork, cancellationToken);
    }
}

public sealed class DisqualifyInquiryCommandHandler(
    ITravelInquiryRepository inquiryRepository,
    ITravelInquiryStatusHistoryRepository historyRepository,
    IActivityWriter activityWriter,
    IAuditWriter auditWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<DisqualifyInquiryCommand>
{
    public async Task Handle(DisqualifyInquiryCommand request, CancellationToken cancellationToken)
    {
        var inquiry = await TravelInquiryCommandHandlerSupport.LoadInquiryAsync(inquiryRepository, request.TenantId, request.InquiryId, cancellationToken);
        var previousStatus = inquiry.Status.ToString();

        if (string.Equals(request.Status, TravelInquiryStatus.Spam.ToString(), StringComparison.OrdinalIgnoreCase))
            inquiry.MarkSpam();
        else if (string.Equals(request.Status, TravelInquiryStatus.Lost.ToString(), StringComparison.OrdinalIgnoreCase))
            inquiry.MarkLost();
        else
            throw new DomainException("Disqualify status must be Lost or Spam.");

        await TravelInquiryCommandHandlerSupport.PersistStatusChangeAsync(inquiry, previousStatus, request.Reason, inquiryRepository, historyRepository, activityWriter, auditWriter, actorContext, unitOfWork, cancellationToken);
    }
}

public sealed class MarkInquiryContactedCommandHandler(
    ITravelInquiryRepository inquiryRepository,
    ITravelInquiryStatusHistoryRepository historyRepository,
    IActivityWriter activityWriter,
    IAuditWriter auditWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<MarkInquiryContactedCommand>
{
    public async Task Handle(MarkInquiryContactedCommand request, CancellationToken cancellationToken)
    {
        var inquiry = await TravelInquiryCommandHandlerSupport.LoadInquiryAsync(inquiryRepository, request.TenantId, request.InquiryId, cancellationToken);
        var previousStatus = inquiry.Status.ToString();
        inquiry.MarkContacted();
        await TravelInquiryCommandHandlerSupport.PersistStatusChangeAsync(inquiry, previousStatus, request.Reason, inquiryRepository, historyRepository, activityWriter, auditWriter, actorContext, unitOfWork, cancellationToken);
    }
}

public sealed class ArchiveInquiryCommandHandler(
    ITravelInquiryRepository inquiryRepository,
    ITravelInquiryStatusHistoryRepository historyRepository,
    IActivityWriter activityWriter,
    IAuditWriter auditWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<ArchiveInquiryCommand>
{
    public async Task Handle(ArchiveInquiryCommand request, CancellationToken cancellationToken)
    {
        var inquiry = await TravelInquiryCommandHandlerSupport.LoadInquiryAsync(inquiryRepository, request.TenantId, request.InquiryId, cancellationToken);
        var previousStatus = inquiry.Status.ToString();
        inquiry.Archive();
        await TravelInquiryCommandHandlerSupport.PersistStatusChangeAsync(inquiry, previousStatus, request.Reason, inquiryRepository, historyRepository, activityWriter, auditWriter, actorContext, unitOfWork, cancellationToken);
    }
}
