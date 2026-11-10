using BuildingBlocks.Messaging.Inbox;
using BuildingBlocks.Messaging.ServiceBus;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AppCancelOrderCommand = Order.Application.Commands.CancelOrderCommand;
using AppConfirmOrderCommand = Order.Application.Commands.ConfirmOrderCommand;
using ContractCancelOrderCommand = BuildingBlocks.Contracts.Commands.CancelOrderCommand;
using ContractConfirmOrderCommand = BuildingBlocks.Contracts.Commands.ConfirmOrderCommand;

namespace Order.API.Consumers;

public class OrderMessageConsumerWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IInMemoryMessageSubscriber _subscriber;
    private readonly ILogger<OrderMessageConsumerWorker> _logger;

    public OrderMessageConsumerWorker(
        IServiceProvider serviceProvider,
        IInMemoryMessageSubscriber subscriber,
        ILogger<OrderMessageConsumerWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _subscriber = subscriber;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _subscriber.Subscribe<ContractConfirmOrderCommand>(async (cmd, ct) =>
        {
            await HandleWithInboxAsync(cmd.CommandId, nameof(ContractConfirmOrderCommand), async scope =>
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                _logger.LogInformation("Processing ConfirmOrderCommand for Order {OrderId}", cmd.OrderId);
                await mediator.Send(new AppConfirmOrderCommand(cmd.OrderId, cmd.CorrelationId), ct);
            }, ct);
        });

        _subscriber.Subscribe<ContractCancelOrderCommand>(async (cmd, ct) =>
        {
            await HandleWithInboxAsync(cmd.CommandId, nameof(ContractCancelOrderCommand), async scope =>
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                _logger.LogInformation("Processing CancelOrderCommand for Order {OrderId}. Reason: {Reason}", cmd.OrderId, cmd.Reason);
                await mediator.Send(new AppCancelOrderCommand(cmd.OrderId, cmd.Reason, cmd.CorrelationId), ct);
            }, ct);
        });

        _logger.LogInformation("OrderMessageConsumerWorker initialized and subscribed.");
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
