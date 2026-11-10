using BuildingBlocks.Messaging.Inbox;
using BuildingBlocks.Messaging.ServiceBus;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notification.Application.Commands;
using Notification.Domain.Enums;
using ContractSendOrderConfirmationEmailCommand = BuildingBlocks.Contracts.Commands.SendOrderConfirmationEmailCommand;

namespace Notification.API.Consumers;

public class NotificationMessageConsumerWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IInMemoryMessageSubscriber _subscriber;
    private readonly ILogger<NotificationMessageConsumerWorker> _logger;

    public NotificationMessageConsumerWorker(
        IServiceProvider serviceProvider,
        IInMemoryMessageSubscriber subscriber,
        ILogger<NotificationMessageConsumerWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _subscriber = subscriber;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _subscriber.Subscribe<ContractSendOrderConfirmationEmailCommand>(async (cmd, ct) =>
        {
            await HandleWithInboxAsync(cmd.CommandId, nameof(ContractSendOrderConfirmationEmailCommand), async scope =>
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                _logger.LogInformation("Processing SendOrderConfirmationEmailCommand for Order {OrderId}, Recipient {Recipient}",
                    cmd.OrderId, cmd.CustomerEmail);

                var subject = $"Order Confirmation - Order #{cmd.OrderId}";
                var body = $"Dear Customer,\n\nThank you for your order! Your order #{cmd.OrderId} totaling {cmd.TotalAmount:C} has been confirmed and is being prepared for shipment.\n\nBest regards,\neCommerce Team";

                var sendNotificationCmd = new SendNotificationCommand(
                    cmd.CustomerEmail,
                    subject,
                    body,
                    NotificationType.OrderConfirmation,
                    NotificationChannel.Email,
                    cmd.CorrelationId);

                var result = await mediator.Send(sendNotificationCmd, ct);
                if (result.IsFailure)
                {
                    _logger.LogError("Failed to send order confirmation email for Order {OrderId}: {Error}",
                        cmd.OrderId, result.Error);
                }
                else
                {
                    _logger.LogInformation("Order confirmation email successfully sent for Order {OrderId}", cmd.OrderId);
                }
            }, ct);
        });

        _logger.LogInformation("NotificationMessageConsumerWorker initialized and subscribed.");
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
