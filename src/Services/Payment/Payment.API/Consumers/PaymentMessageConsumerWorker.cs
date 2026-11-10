using BuildingBlocks.Messaging.Inbox;
using BuildingBlocks.Messaging.ServiceBus;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AppProcessPaymentCommand = Payment.Application.Commands.ProcessPaymentCommand;
using AppRefundPaymentCommand = Payment.Application.Commands.RefundPaymentCommand;
using ContractProcessPaymentCommand = BuildingBlocks.Contracts.Commands.ProcessPaymentCommand;
using ContractRefundPaymentCommand = BuildingBlocks.Contracts.Commands.RefundPaymentCommand;

namespace Payment.API.Consumers;

public class PaymentMessageConsumerWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IInMemoryMessageSubscriber _subscriber;
    private readonly ILogger<PaymentMessageConsumerWorker> _logger;

    public PaymentMessageConsumerWorker(
        IServiceProvider serviceProvider,
        IInMemoryMessageSubscriber subscriber,
        ILogger<PaymentMessageConsumerWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _subscriber = subscriber;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _subscriber.Subscribe<ContractProcessPaymentCommand>(async (cmd, ct) =>
        {
            await HandleWithInboxAsync(cmd.CommandId, nameof(ContractProcessPaymentCommand), async scope =>
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                _logger.LogInformation("Processing ProcessPaymentCommand for Order {OrderId}, Amount {Amount}",
                    cmd.OrderId, cmd.Amount);

                var appCommand = new AppProcessPaymentCommand(
                    cmd.OrderId,
                    cmd.CustomerId,
                    cmd.Amount,
                    "USD",
                    cmd.CorrelationId);

                await mediator.Send(appCommand, ct);
            }, ct);
        });

        _subscriber.Subscribe<ContractRefundPaymentCommand>(async (cmd, ct) =>
        {
            await HandleWithInboxAsync(cmd.CommandId, nameof(ContractRefundPaymentCommand), async scope =>
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                _logger.LogInformation("Processing RefundPaymentCommand for Order {OrderId}, Amount {Amount}",
                    cmd.OrderId, cmd.Amount);

                var appCommand = new AppRefundPaymentCommand(cmd.OrderId, cmd.Amount, cmd.CorrelationId);
                await mediator.Send(appCommand, ct);
            }, ct);
        });

        _logger.LogInformation("PaymentMessageConsumerWorker initialized and subscribed.");
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
