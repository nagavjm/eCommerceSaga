using System.Text.Json;
using BuildingBlocks.Contracts.Commands;
using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using SagaOrchestrator.Application.Common.Interfaces;
using SagaOrchestrator.Application.DTOs;
using SagaOrchestrator.Domain.Entities;
using SagaOrchestrator.Domain.Enums;

namespace SagaOrchestrator.Application.Services;

public class OrderSagaCoordinator : IOrderSagaCoordinator
{
    private readonly ISagaDbContext _dbContext;

    public OrderSagaCoordinator(ISagaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SagaStatusDto> HandleOrderPlacedAsync(
        OrderPlacedIntegrationEvent evt,
        CancellationToken cancellationToken)
    {
        // 1. Check idempotency: check if saga already exists
        var existingSaga = await _dbContext.SagaStates
            .FirstOrDefaultAsync(s => s.OrderId == evt.OrderId, cancellationToken);

        if (existingSaga is not null)
        {
            return MapToDto(existingSaga);
        }

        // 2. Start Saga State Machine
        var saga = OrderSagaState.Start(
            evt.OrderId,
            evt.CustomerId,
            evt.CustomerEmail,
            evt.ShippingAddress,
            evt.TotalAmount,
            evt.CorrelationId);

        saga.TransitionTo(SagaStateEnum.ReservingStock, "OrderPlaced");
        _dbContext.SagaStates.Add(saga);

        // 3. Emit Saga Command: ReserveStockCommand to Outbox
        var reserveStockCommand = new ReserveStockCommand(
            saga.Id,
            evt.OrderId,
            evt.Items,
            evt.CorrelationId);

        EnqueueOutbox(reserveStockCommand);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(saga);
    }

    public async Task<SagaStatusDto> HandleStockReservedAsync(
        StockReservedIntegrationEvent evt,
        CancellationToken cancellationToken)
    {
        var saga = await _dbContext.SagaStates
            .FirstOrDefaultAsync(s => s.OrderId == evt.OrderId, cancellationToken);

        if (saga is null)
        {
            throw new InvalidOperationException($"No active saga found for Order ID '{evt.OrderId}'.");
        }

        saga.TransitionTo(SagaStateEnum.ProcessingPayment, "StockReserved");

        // Emit Saga Command: ProcessPaymentCommand
        var processPaymentCommand = new ProcessPaymentCommand(
            saga.Id,
            saga.OrderId,
            saga.CustomerId,
            saga.TotalAmount,
            saga.CorrelationId);

        EnqueueOutbox(processPaymentCommand);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(saga);
    }

    public async Task<SagaStatusDto> HandleStockReservationFailedAsync(
        StockReservationFailedIntegrationEvent evt,
        CancellationToken cancellationToken)
    {
        var saga = await _dbContext.SagaStates
            .FirstOrDefaultAsync(s => s.OrderId == evt.OrderId, cancellationToken);

        if (saga is null)
        {
            throw new InvalidOperationException($"No active saga found for Order ID '{evt.OrderId}'.");
        }

        saga.FailStep("ReserveStock", evt.Reason);
        saga.StartCompensation($"Inventory reservation failed: {evt.Reason}");

        // Compensation: Cancel the pending order
        var cancelOrderCommand = new CancelOrderCommand(
            saga.Id,
            saga.OrderId,
            $"Inventory reservation failed: {evt.Reason}",
            saga.CorrelationId);

        EnqueueOutbox(cancelOrderCommand);

        saga.CompleteCompensation();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(saga);
    }

    public async Task<SagaStatusDto> HandlePaymentCompletedAsync(
        PaymentCompletedIntegrationEvent evt,
        CancellationToken cancellationToken)
    {
        var saga = await _dbContext.SagaStates
            .FirstOrDefaultAsync(s => s.OrderId == evt.OrderId, cancellationToken);

        if (saga is null)
        {
            throw new InvalidOperationException($"No active saga found for Order ID '{evt.OrderId}'.");
        }

        saga.TransitionTo(SagaStateEnum.ConfirmingOrder, "PaymentCompleted");

        // Forward Actions: Confirm Order & Confirm Stock Deduction
        var confirmOrderCommand = new ConfirmOrderCommand(saga.Id, saga.OrderId, saga.CorrelationId);
        var confirmStockDeductionCommand = new ConfirmStockDeductionCommand(saga.Id, saga.OrderId, saga.CorrelationId);

        EnqueueOutbox(confirmOrderCommand);
        EnqueueOutbox(confirmStockDeductionCommand);

        // Step 6: Create Shipment
        saga.TransitionTo(SagaStateEnum.CreatingShipment, "OrderConfirmed");
        var createShipmentCommand = new CreateShipmentCommand(
            saga.Id,
            saga.OrderId,
            saga.CustomerId,
            saga.ShippingAddress,
            [],
            saga.CorrelationId);

        EnqueueOutbox(createShipmentCommand);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(saga);
    }

    public async Task<SagaStatusDto> HandlePaymentFailedAsync(
        PaymentFailedIntegrationEvent evt,
        CancellationToken cancellationToken)
    {
        var saga = await _dbContext.SagaStates
            .FirstOrDefaultAsync(s => s.OrderId == evt.OrderId, cancellationToken);

        if (saga is null)
        {
            throw new InvalidOperationException($"No active saga found for Order ID '{evt.OrderId}'.");
        }

        saga.FailStep("ProcessPayment", evt.Reason);
        saga.StartCompensation($"Payment failed: {evt.Reason}");
        saga.TransitionTo(SagaStateEnum.CompensatingInventory, "PaymentFailed");

        // Compensating actions:
        // 1. Release reserved inventory
        var releaseStockCommand = new ReleaseStockCommand(
            saga.Id,
            saga.OrderId,
            [],
            saga.CorrelationId);

        // 2. Cancel the order
        var cancelOrderCommand = new CancelOrderCommand(
            saga.Id,
            saga.OrderId,
            $"Payment processing declined: {evt.Reason}",
            saga.CorrelationId);

        EnqueueOutbox(releaseStockCommand);
        EnqueueOutbox(cancelOrderCommand);

        saga.CompleteCompensation();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(saga);
    }

    public async Task<SagaStatusDto> HandleShipmentCreatedAsync(
        ShipmentCreatedIntegrationEvent evt,
        CancellationToken cancellationToken)
    {
        var saga = await _dbContext.SagaStates
            .FirstOrDefaultAsync(s => s.OrderId == evt.OrderId, cancellationToken);

        if (saga is null)
        {
            throw new InvalidOperationException($"No active saga found for Order ID '{evt.OrderId}'.");
        }

        saga.SetTrackingNumber(evt.TrackingNumber);
        saga.TransitionTo(SagaStateEnum.SendingNotification, "ShipmentCreated");

        // Step 7: Send Order Confirmation Email
        var sendEmailCommand = new SendOrderConfirmationEmailCommand(
            saga.Id,
            saga.OrderId,
            saga.CustomerId,
            saga.CustomerEmail,
            saga.TotalAmount,
            saga.CorrelationId);

        EnqueueOutbox(sendEmailCommand);

        saga.TransitionTo(SagaStateEnum.Completed, "NotificationSent");

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(saga);
    }

    public async Task<SagaStatusDto> HandleShipmentCreationFailedAsync(
        ShipmentCreationFailedIntegrationEvent evt,
        CancellationToken cancellationToken)
    {
        var saga = await _dbContext.SagaStates
            .FirstOrDefaultAsync(s => s.OrderId == evt.OrderId, cancellationToken);

        if (saga is null)
        {
            throw new InvalidOperationException($"No active saga found for Order ID '{evt.OrderId}'.");
        }

        saga.FailStep("CreateShipment", evt.Reason);

        // Per architecture requirement:
        // "Shipping creation fails: Follow a clearly defined retry or manual-recovery policy; do not automatically refund every payment failure."
        saga.Escalate($"Shipping creation failed: {evt.Reason}. Escalated for manual carrier intervention.");

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(saga);
    }

    private void EnqueueOutbox<T>(T message) where T : class
    {
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            Type = typeof(T).AssemblyQualifiedName ?? typeof(T).Name,
            Payload = JsonSerializer.Serialize(message),
            ProcessedOnUtc = null,
            RetryCount = 0
        };

        _dbContext.OutboxMessages.Add(outboxMessage);
    }

    private static SagaStatusDto MapToDto(OrderSagaState saga)
    {
        return new SagaStatusDto(
            saga.Id,
            saga.OrderId,
            saga.CustomerId,
            saga.CustomerEmail,
            saga.CurrentState.ToString(),
            saga.CompletedSteps.ToList(),
            saga.FailedStep,
            saga.FailureReason,
            saga.RetryCount,
            saga.CompensationStatus.ToString(),
            saga.TrackingNumber,
            saga.CorrelationId,
            saga.CreatedAtUtc,
            saga.UpdatedAtUtc);
    }
}
