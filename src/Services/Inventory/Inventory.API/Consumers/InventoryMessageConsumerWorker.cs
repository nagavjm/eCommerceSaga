using System.Text.Json;
using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Messaging.Inbox;
using BuildingBlocks.Messaging.Outbox;
using BuildingBlocks.Messaging.ServiceBus;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.DTOs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AppCommitStockCommand = Inventory.Application.Commands.CommitStockDeductionCommand;
using AppReleaseStockCommand = Inventory.Application.Commands.ReleaseStockCommand;
using AppReserveStockCommand = Inventory.Application.Commands.ReserveStockCommand;
using ContractConfirmDeductionCommand = BuildingBlocks.Contracts.Commands.ConfirmStockDeductionCommand;
using ContractReleaseStockCommand = BuildingBlocks.Contracts.Commands.ReleaseStockCommand;
using ContractReserveStockCommand = BuildingBlocks.Contracts.Commands.ReserveStockCommand;

namespace Inventory.API.Consumers;

public class InventoryMessageConsumerWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IInMemoryMessageSubscriber _subscriber;
    private readonly ILogger<InventoryMessageConsumerWorker> _logger;

    public InventoryMessageConsumerWorker(
        IServiceProvider serviceProvider,
        IInMemoryMessageSubscriber subscriber,
        ILogger<InventoryMessageConsumerWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _subscriber = subscriber;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _subscriber.Subscribe<ContractReserveStockCommand>(async (cmd, ct) =>
        {
            await HandleWithInboxAsync(cmd.CommandId, nameof(ContractReserveStockCommand), async scope =>
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var dbContext = scope.ServiceProvider.GetRequiredService<IInventoryDbContext>();

                _logger.LogInformation("Processing ReserveStockCommand for Order {OrderId}", cmd.OrderId);

                var reservationItems = cmd.Items
                    .Select(i => new StockReservationItemDto(i.ProductId, i.Quantity))
                    .ToList();

                var appCommand = new AppReserveStockCommand(cmd.OrderId, reservationItems);
                var result = await mediator.Send(appCommand, ct);

                OutboxMessage outboxMessage;

                if (result.IsSuccess && result.Value.IsSuccessful)
                {
                    var reservedEvent = new StockReservedIntegrationEvent(cmd.OrderId, cmd.CorrelationId);
                    outboxMessage = new OutboxMessage
                    {
                        Id = Guid.NewGuid(),
                        OccurredOnUtc = DateTime.UtcNow,
                        Type = typeof(StockReservedIntegrationEvent).AssemblyQualifiedName ?? typeof(StockReservedIntegrationEvent).Name,
                        Payload = JsonSerializer.Serialize(reservedEvent),
                        ProcessedOnUtc = null,
                        RetryCount = 0
                    };
                }
                else
                {
                    var reason = result.Value?.FailureReason ?? result.Error.Description ?? "Stock reservation rejected.";
                    var failedEvent = new StockReservationFailedIntegrationEvent(cmd.OrderId, reason, cmd.CorrelationId);
                    outboxMessage = new OutboxMessage
                    {
                        Id = Guid.NewGuid(),
                        OccurredOnUtc = DateTime.UtcNow,
                        Type = typeof(StockReservationFailedIntegrationEvent).AssemblyQualifiedName ?? typeof(StockReservationFailedIntegrationEvent).Name,
                        Payload = JsonSerializer.Serialize(failedEvent),
                        ProcessedOnUtc = null,
                        RetryCount = 0
                    };
                }

                dbContext.OutboxMessages.Add(outboxMessage);
                await dbContext.SaveChangesAsync(ct);
            }, ct);
        });

        _subscriber.Subscribe<ContractReleaseStockCommand>(async (cmd, ct) =>
        {
            await HandleWithInboxAsync(cmd.CommandId, nameof(ContractReleaseStockCommand), async scope =>
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                _logger.LogInformation("Processing ReleaseStockCommand for Order {OrderId}", cmd.OrderId);

                var items = cmd.Items
                    .Select(i => new StockReservationItemDto(i.ProductId, i.Quantity))
                    .ToList();

                await mediator.Send(new AppReleaseStockCommand(cmd.OrderId, items), ct);
            }, ct);
        });

        _subscriber.Subscribe<ContractConfirmDeductionCommand>(async (cmd, ct) =>
        {
            await HandleWithInboxAsync(cmd.CommandId, nameof(ContractConfirmDeductionCommand), async scope =>
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                _logger.LogInformation("Processing ConfirmStockDeductionCommand for Order {OrderId}", cmd.OrderId);

                await mediator.Send(new AppCommitStockCommand(cmd.OrderId, []), ct);
            }, ct);
        });

        _logger.LogInformation("InventoryMessageConsumerWorker initialized and subscribed.");
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
