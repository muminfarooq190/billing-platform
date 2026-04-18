using CommunicationService.Api;
using CommunicationService.Api.Contracts;
using CommunicationService.Api.Controllers;
using CommunicationService.Application.Abstractions;
using CommunicationService.Application.Commands.SendNotification;
using CommunicationService.Application.Commands.SendWorkflowNotification;
using CommunicationService.Infrastructure.Channels;
using CommunicationService.Domain.Aggregates;
using CommunicationService.Domain.Enums;
using CommunicationService.Domain.Repositories;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CommunicationService.Tests;

public sealed class MvpPassTests
{
    [Fact]
    public async Task NotificationsController_Send_ShouldUseTenantContextInsteadOfCallerPayload()
    {
        var mediator = new CapturingMediator();
        var tenantId = Guid.NewGuid();
        var controller = new NotificationsController(mediator, new StubTenantContext(tenantId));

        await controller.Send(new SendNotificationRequest(
            Guid.NewGuid(),
            "EndUser",
            "Email",
            null,
            "Invoice ready",
            "Please review",
            "Normal",
            "invoice-123",
            "corr-1",
            null,
            [new DocumentReferenceRequest("invoice.pdf", "doc-1", "https://cdn.local/invoice.pdf", "application/pdf", 1200, null)],
            new Dictionary<string, string> { ["source"] = "test" },
            new Dictionary<string, string> { ["customerName"] = "Ada" }),
            CancellationToken.None);

        mediator.LastNotificationCommand.Should().NotBeNull();
        mediator.LastNotificationCommand!.TenantId.Should().Be(tenantId);
        mediator.LastNotificationCommand.ReferenceId.Should().Be("invoice-123");
        mediator.LastNotificationCommand.DocumentReferencesJson.Should().Contain("invoice.pdf");
        mediator.LastNotificationCommand.MetadataJson.Should().Contain("source");
    }

    [Fact]
    public async Task TemplatesController_Create_ShouldUseTenantContextInsteadOfCallerPayload()
    {
        var mediator = new CapturingMediator();
        var tenantId = Guid.NewGuid();
        var controller = new TemplatesController(mediator, new StubTenantContext(tenantId));

        await controller.Create(new CreateTemplateRequest("quote-email", "Quote", "Hello {{customerName}}", "Email", null), CancellationToken.None);

        mediator.LastCreateTemplateCommand.Should().NotBeNull();
        mediator.LastCreateTemplateCommand!.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task SendNotificationHandler_ShouldReturnExistingNotificationId_ForSameTenantIdempotencyKey()
    {
        var tenantId = Guid.NewGuid();
        var existing = Notification.Create(tenantId, Guid.NewGuid(), RecipientType.EndUser, ChannelType.Email, "Existing", "Existing", NotificationPriority.Normal, null, "ref", "corr", null, "idem-1", "[]", "{}");
        var repo = new InMemoryNotificationRepository(existing);
        var handler = new SendNotificationCommandHandler(
            repo,
            new NullTemplateRepository(),
            new NullPreferenceRepository(),
            new AllowFeatureGate(),
            new PassthroughBrandingRenderer(),
            new ChannelPreferenceResolver(),
            new NoOpUnitOfWork(),
            new StubTenantContext(tenantId));

        var id = await handler.Handle(new SendNotificationCommand(tenantId, Guid.NewGuid(), "EndUser", "Email", null, "Subject", "Body", "Normal", "ref-2", "corr-2", "idem-1", null, "[]", "{}", null), CancellationToken.None);

        id.Should().Be(existing.Id);
        repo.AddedCount.Should().Be(0);
    }

    [Fact]
    public async Task WorkflowHandler_ShouldPopulateWorkflowTypeAndDefaultPriority()
    {
        var mediator = new CapturingMediator();
        var handler = new SendWorkflowNotificationCommandHandler(mediator);

        await handler.Handle(new SendWorkflowNotificationCommand(Guid.NewGuid(), "payment-reminder", Guid.NewGuid(), "EndUser", "Email", null, null, null, null, "invoice-9", "corr-9", "idem-9", "[]", "{}", null), CancellationToken.None);

        mediator.LastNotificationCommand.Should().NotBeNull();
        mediator.LastNotificationCommand!.WorkflowType.Should().Be("payment-reminder");
        mediator.LastNotificationCommand.Priority.Should().Be("High");
        mediator.LastNotificationCommand.Subject.Should().Be("Payment reminder");
    }

    private sealed class StubTenantContext(Guid tenantId) : ITenantContext
    {
        public Guid TenantId { get; } = tenantId;
        public Guid? UserId { get; } = Guid.NewGuid();
    }

    private sealed class CapturingMediator : IMediator
    {
        public SendNotificationCommand? LastNotificationCommand { get; private set; }
        public CommunicationService.Application.Commands.CreateTemplate.CreateTemplateCommand? LastCreateTemplateCommand { get; private set; }

        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification => Task.CompletedTask;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case SendNotificationCommand command:
                    LastNotificationCommand = command;
                    return Task.FromResult((TResponse)(object)Guid.NewGuid());
                case CommunicationService.Application.Commands.CreateTemplate.CreateTemplateCommand templateCommand:
                    LastCreateTemplateCommand = templateCommand;
                    return Task.FromResult((TResponse)(object)Guid.NewGuid());
                default:
                    throw new InvalidOperationException($"Unhandled request type {request.GetType().Name}");
            }
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
            => Task.CompletedTask;

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class InMemoryNotificationRepository(params Notification[] existing) : INotificationRepository
    {
        private readonly List<Notification> _notifications = existing.ToList();
        public int AddedCount { get; private set; }

        public Task AddAsync(Notification notification, CancellationToken cancellationToken)
        {
            AddedCount++;
            _notifications.Add(notification);
            return Task.CompletedTask;
        }

        public Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(_notifications.SingleOrDefault(x => x.Id == id));
        public Task<Notification?> GetByIdempotencyKeyAsync(Guid tenantId, string idempotencyKey, CancellationToken cancellationToken) => Task.FromResult(_notifications.SingleOrDefault(x => x.TenantId == tenantId && x.IdempotencyKey == idempotencyKey));
        public Task<IReadOnlyList<Notification>> ListByRecipientIdAsync(Guid recipientId, CancellationToken cancellationToken) => Task.FromResult((IReadOnlyList<Notification>)_notifications.Where(x => x.RecipientId == recipientId).ToList());
        public Task<IReadOnlyList<Notification>> ListPendingAsync(int batchSize, CancellationToken cancellationToken) => Task.FromResult((IReadOnlyList<Notification>)_notifications.Where(x => x.Status == NotificationStatus.Queued).Take(batchSize).ToList());
        public Task<IReadOnlyList<Notification>> ListRetryableAsync(int maxRetries, int batchSize, CancellationToken cancellationToken) => Task.FromResult((IReadOnlyList<Notification>)_notifications.Where(x => x.Status == NotificationStatus.Failed && x.RetryCount < maxRetries).Take(batchSize).ToList());
        public Task UpdateAsync(Notification notification, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class NullTemplateRepository : INotificationTemplateRepository
    {
        public Task AddAsync(NotificationTemplate template, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<NotificationTemplate?>(null);
        public Task<NotificationTemplate?> GetByNameAndTenantAsync(string name, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<NotificationTemplate?>(null);
        public Task<IReadOnlyList<NotificationTemplate>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult((IReadOnlyList<NotificationTemplate>)[]);
        public Task UpdateAsync(NotificationTemplate template, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class NullPreferenceRepository : IRecipientPreferencesRepository
    {
        public Task AddAsync(RecipientPreferences preferences, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<RecipientPreferences?> GetByRecipientIdAsync(Guid recipientId, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<RecipientPreferences?>(null);
        public Task UpdateAsync(RecipientPreferences preferences, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class AllowFeatureGate : IFeatureGate
    {
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
    }

    private sealed class PassthroughBrandingRenderer : IBrandingTemplateRenderer
    {
        public Task<Dictionary<string, string>> EnrichAsync(Guid tenantId, string channel, Dictionary<string, string> placeholders, CancellationToken cancellationToken)
            => Task.FromResult(placeholders);
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }
}
