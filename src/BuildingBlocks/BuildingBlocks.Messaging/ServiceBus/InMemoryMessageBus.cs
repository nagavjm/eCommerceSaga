using System.Collections.Concurrent;
using System.Text.Json;
using BuildingBlocks.Contracts.Commands;
using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Messaging.Abstractions;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Messaging.ServiceBus;

public interface IInMemoryMessageSubscriber
{
    void Subscribe<T>(Func<T, CancellationToken, Task> handler) where T : class;
}

public class InMemoryMessageBus : IMessagePublisher, IInMemoryMessageSubscriber
{
    private readonly ILogger<InMemoryMessageBus> _logger;
    private readonly ConcurrentDictionary<Type, List<Func<object, CancellationToken, Task>>> _handlers = new();

    public InMemoryMessageBus(ILogger<InMemoryMessageBus> logger)
    {
        _logger = logger;
    }

    public void Subscribe<T>(Func<T, CancellationToken, Task> handler) where T : class
    {
        var messageType = typeof(T);
        var handlerList = _handlers.GetOrAdd(messageType, _ => []);

        lock (handlerList)
        {
            handlerList.Add((msg, ct) => handler((T)msg, ct));
        }

        _logger.LogInformation("Subscribed in-memory handler for {MessageType}", messageType.Name);
    }

    public async Task PublishAsync<T>(
        T message,
        string? queueOrTopicName = null,
        Guid? correlationId = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var messageType = message.GetType();
        _logger.LogInformation("Publishing in-memory message of type {MessageType}", messageType.Name);

        if (_handlers.TryGetValue(messageType, out var handlerList))
        {
            List<Func<object, CancellationToken, Task>> handlersToInvoke;
            lock (handlerList)
            {
                handlersToInvoke = [.. handlerList];
            }

            foreach (var handler in handlersToInvoke)
            {
                try
                {
                    await handler(message, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing in-memory message of type {MessageType}", messageType.Name);
                    throw;
                }
            }
        }
        else
        {
            _logger.LogDebug("No in-memory handlers registered for {MessageType}", messageType.Name);
        }
    }
}
