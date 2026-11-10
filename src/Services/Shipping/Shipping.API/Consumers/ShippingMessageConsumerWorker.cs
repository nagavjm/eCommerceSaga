using BuildingBlocks.Messaging.Inbox;
using BuildingBlocks.Messaging.ServiceBus;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shipping.Application.DTOs;
using AppCreateShipmentCommand = Shipping.Application.Commands.CreateShipmentCommand;
using ContractCreateShipmentCommand = BuildingBlocks.Contracts.Commands.CreateShipmentCommand;

namespace Shipping.API.Consumers;

public class ShippingMessageConsumerWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IInMemoryMessageSubscriber _subscriber;
    private readonly ILogger<ShippingMessageConsumerWorker> _logger;

    public ShippingMessageConsumerWorker(
        IServiceProvider serviceProvider,
        IInMemoryMessageSubscriber subscriber,
        ILogger<ShippingMessageConsumerWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _subscriber = subscriber;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _subscriber.Subscribe<ContractCreateShipmentCommand>(async (cmd, ct) =>
        {
            await HandleWithInboxAsync(cmd.CommandId, nameof(ContractCreateShipmentCommand), async scope =>
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                _logger.LogInformation("Processing CreateShipmentCommand for Order {OrderId}", cmd.OrderId);

                var parts = cmd.ShippingAddress?.Split(',') ?? Array.Empty<string>();
                var street = parts.Length > 0 ? parts[0].Trim() : "123 Main St";
                var city = parts.Length > 1 ? parts[1].Trim() : "Metropolis";
                var state = parts.Length > 2 ? parts[2].Trim() : "NY";
                var country = parts.Length > 3 ? parts[3].Trim() : "USA";
                var zipCode = parts.Length > 4 ? parts[4].Trim() : "10001";

                var address = new AddressDto(street, city, state, country, zipCode);

                var items = (cmd.Items == null || cmd.Items.Count == 0)
                    ? new List<ShipmentItemDto> { new(Guid.NewGuid(), "Order Package", 1) }
                    : cmd.Items.Select(i => new ShipmentItemDto(i.ProductId, i.ProductName, i.Quantity)).ToList();

                var appCommand = new AppCreateShipmentCommand(
                    cmd.OrderId,
                    cmd.CustomerId,
                    address,
                    "Express Carrier",
                    items,
                    cmd.CorrelationId);

                var result = await mediator.Send(appCommand, ct);
                if (result.IsFailure)
                {
                    _logger.LogError("Failed to create shipment for Order {OrderId}: {Error}", cmd.OrderId, result.Error);
                }
                else
                {
                    _logger.LogInformation("Shipment created successfully for Order {OrderId}. Tracking: {TrackingNumber}",
                        cmd.OrderId, result.Value.TrackingNumber);
                }
            }, ct);
        });

        _logger.LogInformation("ShippingMessageConsumerWorker initialized and subscribed.");
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
