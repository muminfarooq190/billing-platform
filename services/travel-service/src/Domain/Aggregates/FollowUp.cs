using TravelService.Domain.Common;
using TravelService.Domain.Enums;
using TravelService.Domain.Events;
using TravelService.Domain.Exceptions;

namespace TravelService.Domain.Aggregates;

public sealed class FollowUp : AggregateRoot
{
    private FollowUp() { }

    private FollowUp(Guid tenantId, Guid customerContactId, string customerName, string subject, string notes, FollowUpPriority priority, DateTimeOffset dueDate, Guid? assignedToUserId)
    {
        ValidateIdentity(tenantId, customerContactId);
        ValidateDetails(customerName, subject, dueDate);

        Id = Guid.NewGuid();
        TenantId = tenantId;
        CustomerContactId = customerContactId;
        CustomerName = customerName.Trim();
        Subject = subject.Trim();
        Notes = notes.Trim();
        Priority = priority;
        Status = FollowUpStatus.Pending;
        DueDate = dueDate;
        AssignedToUserId = assignedToUserId;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new FollowUpCreatedEvent(Id, TenantId, CustomerContactId));
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid CustomerContactId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Notes { get; private set; } = string.Empty;
    public FollowUpPriority Priority { get; private set; }
    public FollowUpStatus Status { get; private set; }
    public DateTimeOffset DueDate { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static FollowUp Create(Guid tenantId, Guid customerContactId, string customerName, string subject, string notes, FollowUpPriority priority, DateTimeOffset dueDate, Guid? assignedToUserId)
        => new(tenantId, customerContactId, customerName, subject, notes, priority, dueDate, assignedToUserId);

    public void MarkInProgress()
    {
        if (Status == FollowUpStatus.Completed)
            throw new DomainException("Cannot reopen a completed follow-up.");
        if (Status == FollowUpStatus.Cancelled)
            throw new DomainException("Cannot restart a cancelled follow-up.");
        if (Status == FollowUpStatus.InProgress)
            return;
        Status = FollowUpStatus.InProgress;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Complete()
    {
        if (Status == FollowUpStatus.Cancelled)
            throw new DomainException("Cannot complete a cancelled follow-up.");
        if (Status == FollowUpStatus.Completed)
            return;
        Status = FollowUpStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new FollowUpCompletedEvent(Id, TenantId));
    }

    public void Cancel()
    {
        if (Status == FollowUpStatus.Completed)
            throw new DomainException("Cannot cancel a completed follow-up.");
        if (Status == FollowUpStatus.Cancelled)
            return;
        Status = FollowUpStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string subject, string notes, FollowUpPriority priority, DateTimeOffset dueDate, Guid? assignedToUserId)
    {
        if (Status is FollowUpStatus.Completed or FollowUpStatus.Cancelled)
            throw new DomainException("Can only edit pending or in-progress follow-ups.");

        ValidateDetails(CustomerName, subject, dueDate);
        Subject = subject.Trim();
        Notes = notes.Trim();
        Priority = priority;
        DueDate = dueDate;
        AssignedToUserId = assignedToUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static void ValidateIdentity(Guid tenantId, Guid customerContactId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");
        if (customerContactId == Guid.Empty)
            throw new DomainException("CustomerContactId is required.");
    }

    private static void ValidateDetails(string customerName, string subject, DateTimeOffset dueDate)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("Customer name is required.");
        if (string.IsNullOrWhiteSpace(subject))
            throw new DomainException("Follow-up subject is required.");
        if (dueDate < DateTimeOffset.UtcNow.AddMinutes(-1))
            throw new DomainException("Follow-up due date cannot be in the past.");
    }
}
