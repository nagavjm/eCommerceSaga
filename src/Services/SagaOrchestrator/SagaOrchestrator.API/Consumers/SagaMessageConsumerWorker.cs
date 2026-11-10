using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Messaging.Inbox;
using BuildingBlocks.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SagaOrchestrator.Application.Services;

namespace SagaOrchestrator.API.Consumers;

public class SagaMessageConsumerWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IInMemoryMessageSubscriber _subscriber;
    private readonly ILogger<SagaMessageConsumerWorker> _logger;

    public SagaMessageConsumerWorker(
        IServiceProvider serviceProvider,
        IInMemoryMessageSubscriber subscriber,
        ILogger<SagaMessageConsumerWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _subscriber = subscriber;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _subscriber.Subscribe<OrderPlacedIntegrationEvent>(async (evt, ct) =>
        {
            await HandleWithInboxAsync(evt.Id, nameof(OrderPlacedIntegrationEvent), async scope =>
            {
                var coordinator = scope.ServiceProvider.GetRequiredService<IOrderSagaCoordinator>();
                _logger.LogInformation("Saga received OrderPlacedIntegrationEvent for Order {OrderId}", evt.OrderId);
                await coordinator.HandleOrderPlacedAsync(evt, ct);
            }, ct);
        });

        _subscriber.Subscribe<StockReservedIntegrationEvent>(async (evt, ct) =>
        {
            await HandleWithInboxAsync(evt.Id, nameof(StockReservedIntegrationEvent), async scope =>
            {
                var coordinator = scope.ServiceProvider.GetRequiredService<IOrderSagaCoordinator>();
                _logger.LogInformation("Saga received StockReservedIntegrationEvent for Order {OrderId}", evt.OrderId);
                await coordinator.HandleStockReservedAsync(evt, ct);
            }, ct);
        });

        _subscriber.Subscribe<StockReservationFailedIntegrationEvent>(async (evt, ct) =>
        {
            await HandleWithInboxAsync(evt.Id, nameof(StockReservationFailedIntegrationEvent), async scope =>
            {
                var coordinator = scope.ServiceProvider.GetRequiredService<IOrderSagaCoordinator>();
                _logger.LogWarning("Saga received StockReservationFailedIntegrationEvent for Order {OrderId}. Reason: {Reason}",
                    evt.OrderId, evt.Reason);
                await coordinator.HandleStockReservationFailedAsync(evt, ct);
            }, ct);
        });

        _subscriber.Subscribe<PaymentCompletedIntegrationEvent>(async (evt, ct) =>
        {
            await HandleWithInboxAsync(evt.Id, nameof(PaymentCompletedIntegrationEvent), async scope =>
            {
                var coordinator = scope.ServiceProvider.GetRequiredService<IOrderSagaCoordinator>();
                _logger.LogInformation("Saga received PaymentCompletedIntegrationEvent for Order {OrderId}", evt.OrderId);
                await coordinator.HandlePaymentCompletedAsync(evt, ct);
            }, ct);
        });

        _subscriber.Subscribe<PaymentFailedIntegrationEvent>(async (evt, ct) =>
        {
            await HandleWithInboxAsync(evt.Id, nameof(PaymentFailedIntegrationEvent), async scope =>
            {
                var coordinator = scope.ServiceProvider.GetRequiredService<IOrderSagaCoordinator>();
                _logger.LogWarning("Saga received PaymentFailedIntegrationEvent for Order {OrderId}. Reason: {Reason}",
                    evt.OrderId, evt.Reason);
                await coordinator.HandlePaymentFailedAsync(evt, ct);
            }, ct);
        });

        _subscriber.Subscribe<ShipmentCreatedIntegrationEvent>(async (evt, ct) =>
        {
            await HandleWithInboxAsync(evt.Id, nameof(ShipmentCreatedIntegrationEvent), async scope =>
            {
                var coordinator = scope.ServiceProvider.GetRequiredService<IOrderSagaCoordinator>();
                _logger.LogInformation("Saga received ShipmentCreatedIntegrationEvent for Order {OrderId}, Tracking: {TrackingNumber}",
                    evt.OrderId, evt.TrackingNumber);
                await coordinator.HandleShipmentCreatedAsync(evt, ct);
            }, ct);
        });

        _subscriber.Subscribe<ShipmentCreationFailedIntegrationEvent>(async (evt, ct) =>
        {
            await HandleWithInboxAsync(evt.Id, nameof(ShipmentCreationFailedIntegrationEvent), async scope =>
            {
                var coordinator = scope.ServiceProvider.GetRequiredService<IOrderSagaCoordinator>();
                _logger.LogError("Saga received ShipmentCreationFailedIntegrationEvent for Order {OrderId}. Reason: {Reason}",
                    evt.OrderId, evt.Reason);
                await coordinator.HandleShipmentCreationFailedAsync(evt, ct);
            }, ct);
        });

        _logger.LogInformation("SagaMessageConsumerWorker initialized and subscribed to all 7 saga integration events.");
        return Task.CompletedTask;
    }

    private async Task HandleWithInboxAsync(
        Guid messageId,
        string handlerName,
        Func<IServiceScope, Task> action,
        CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var inboxStore = scope.ServiceProvider.GetRequiredService<IInboxStore>();

        if (await inboxStore.HasBeenProcessedAsync(messageId, handlerName, cancellationToken))
        {
            _logger.LogWarning("Message {MessageId} already processed by {HandlerName}. Skipping (idempotent).",
                messageId, handlerName);
            return;
        }

        await action(scope);
        await inboxStore.MarkAsProcessedAsync(messageId, handlerName, cancellationToken);
    }
}
