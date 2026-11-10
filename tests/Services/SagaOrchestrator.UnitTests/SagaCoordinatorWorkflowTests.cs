using BuildingBlocks.Contracts.Commands;
using BuildingBlocks.Contracts.Events;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Domain.Enums;
using SagaOrchestrator.Infrastructure.Persistence;

namespace SagaOrchestrator.UnitTests;

public static class SagaDbContextFactory
{
    public static SagaDbContext Create()
    {
        var options = new DbContextOptionsBuilder<SagaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new SagaDbContext(options);
    }
}

public class SagaCoordinatorWorkflowTests
{
    [Fact]
    public async Task Scenario1_HappyPathOrderFulfillment_ShouldCompleteSagaAndEmitAllCommands()
    {
        // Arrange
        await using var ctx = SagaDbContextFactory.Create();
        var coordinator = new OrderSagaCoordinator(ctx);

        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var orderPlacedEvent = new OrderPlacedIntegrationEvent(
            orderId,
            customerId,
            "sarah@example.com",
            "123 Pine St, Seattle, WA",
            120.00m,
            new List<OrderItemDto> { new(Guid.NewGuid(), "Product A", 2, 60m) },
            correlationId);

        // Step 1: Order Placed -> Saga Starts and emits ReserveStockCommand
        var step1Result = await coordinator.HandleOrderPlacedAsync(orderPlacedEvent, CancellationToken.None);
        step1Result.CurrentState.Should().Be("ReservingStock");

        var reserveStockOutbox = await ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Type.Contains(nameof(ReserveStockCommand)));
        reserveStockOutbox.Should().NotBeNull();

        // Step 2: Stock Reserved -> Saga advances and emits ProcessPaymentCommand
        var stockReservedEvent = new StockReservedIntegrationEvent(orderId, correlationId);
        var step2Result = await coordinator.HandleStockReservedAsync(stockReservedEvent, CancellationToken.None);
        step2Result.CurrentState.Should().Be("ProcessingPayment");

        var processPaymentOutbox = await ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Type.Contains(nameof(ProcessPaymentCommand)));
        processPaymentOutbox.Should().NotBeNull();

        // Step 3: Payment Completed -> Saga advances and emits ConfirmOrder, ConfirmDeduction, and CreateShipment
        var paymentCompletedEvent = new PaymentCompletedIntegrationEvent(orderId, Guid.NewGuid(), 120m, "TXN-12345", correlationId);
        var step3Result = await coordinator.HandlePaymentCompletedAsync(paymentCompletedEvent, CancellationToken.None);
        step3Result.CurrentState.Should().Be("CreatingShipment");

        var confirmOrderOutbox = await ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Type.Contains(nameof(ConfirmOrderCommand)));
        var confirmDeductionOutbox = await ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Type.Contains(nameof(ConfirmStockDeductionCommand)));
        var createShipmentOutbox = await ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Type.Contains(nameof(CreateShipmentCommand)));

        confirmOrderOutbox.Should().NotBeNull();
        confirmDeductionOutbox.Should().NotBeNull();
        createShipmentOutbox.Should().NotBeNull();

        // Step 4: Shipment Created -> Saga advances and emits SendOrderConfirmationEmail, completes saga
        var shipmentCreatedEvent = new ShipmentCreatedIntegrationEvent(orderId, Guid.NewGuid(), "TRK-987654", "DHL", correlationId);
        var step4Result = await coordinator.HandleShipmentCreatedAsync(shipmentCreatedEvent, CancellationToken.None);
        step4Result.CurrentState.Should().Be("Completed");
        step4Result.TrackingNumber.Should().Be("TRK-987654");

        var sendEmailOutbox = await ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Type.Contains(nameof(SendOrderConfirmationEmailCommand)));
        sendEmailOutbox.Should().NotBeNull();

        // Verify full completed steps trace
        step4Result.CompletedSteps.Should().Contain(new[]
        {
            "OrderPlaced",
            "StockReserved",
            "PaymentCompleted",
            "OrderConfirmed",
            "ShipmentCreated",
            "NotificationSent"
        });
    }

    [Fact]
    public async Task Scenario2_StockReservationFailed_ShouldCompensateAndCancelOrder()
    {
        // Arrange
        await using var ctx = SagaDbContextFactory.Create();
        var coordinator = new OrderSagaCoordinator(ctx);

        var orderId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var orderPlacedEvent = new OrderPlacedIntegrationEvent(
            orderId,
            Guid.NewGuid(),
            "buyer@test.com",
            "Street 1",
            50m,
            new List<OrderItemDto> { new(Guid.NewGuid(), "Out of stock item", 10, 5m) },
            correlationId);

        await coordinator.HandleOrderPlacedAsync(orderPlacedEvent, CancellationToken.None);

        // Act: Stock reservation failed event
        var failureEvent = new StockReservationFailedIntegrationEvent(
            orderId,
            "Insufficient inventory: only 2 available, requested 10",
            correlationId);

        var result = await coordinator.HandleStockReservationFailedAsync(failureEvent, CancellationToken.None);

        // Assert
        result.CurrentState.Should().Be("Failed");
        result.CompensationStatus.Should().Be("Completed");
        result.FailedStep.Should().Be("ReserveStock");
        result.FailureReason.Should().Contain("Insufficient inventory");

        // Verify CancelOrderCommand was emitted for compensation
        var cancelOrderOutbox = await ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Type.Contains(nameof(CancelOrderCommand)));
        cancelOrderOutbox.Should().NotBeNull();
    }

    [Fact]
    public async Task Scenario3_PaymentDeclined_ShouldCompensateInventoryAndCancelOrder()
    {
        // Arrange
        await using var ctx = SagaDbContextFactory.Create();
        var coordinator = new OrderSagaCoordinator(ctx);

        var orderId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var orderPlacedEvent = new OrderPlacedIntegrationEvent(
            orderId,
            Guid.NewGuid(),
            "buyer@test.com",
            "Street 1",
            500m,
            new List<OrderItemDto> { new(Guid.NewGuid(), "Item", 1, 500m) },
            correlationId);

        await coordinator.HandleOrderPlacedAsync(orderPlacedEvent, CancellationToken.None);
        await coordinator.HandleStockReservedAsync(new StockReservedIntegrationEvent(orderId, correlationId), CancellationToken.None);

        // Act: Payment declined
        var paymentFailedEvent = new PaymentFailedIntegrationEvent(
            orderId,
            "Card declined: Insufficient funds",
            correlationId);

        var result = await coordinator.HandlePaymentFailedAsync(paymentFailedEvent, CancellationToken.None);

        // Assert
        result.CurrentState.Should().Be("Failed");
        result.CompensationStatus.Should().Be("Completed");
        result.FailedStep.Should().Be("ProcessPayment");

        // Verify both ReleaseStockCommand and CancelOrderCommand were emitted
        var releaseStockOutbox = await ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Type.Contains(nameof(ReleaseStockCommand)));
        var cancelOrderOutbox = await ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Type.Contains(nameof(CancelOrderCommand)));

        releaseStockOutbox.Should().NotBeNull();
        cancelOrderOutbox.Should().NotBeNull();
    }

    [Fact]
    public async Task Scenario4_ShippingCreationFailure_ShouldEscalateForManualIntervention()
    {
        // Arrange
        await using var ctx = SagaDbContextFactory.Create();
        var coordinator = new OrderSagaCoordinator(ctx);

        var orderId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var orderPlacedEvent = new OrderPlacedIntegrationEvent(
            orderId,
            Guid.NewGuid(),
            "buyer@test.com",
            "Street 1",
            75m,
            [],
            correlationId);

        await coordinator.HandleOrderPlacedAsync(orderPlacedEvent, CancellationToken.None);
        await coordinator.HandleStockReservedAsync(new StockReservedIntegrationEvent(orderId, correlationId), CancellationToken.None);
        await coordinator.HandlePaymentCompletedAsync(new PaymentCompletedIntegrationEvent(orderId, Guid.NewGuid(), 75m, "TXN-1", correlationId), CancellationToken.None);

        // Act: Shipping service failure
        var shippingFailedEvent = new ShipmentCreationFailedIntegrationEvent(
            orderId,
            "Carrier dispatch system unreachable",
            correlationId);

        var result = await coordinator.HandleShipmentCreationFailedAsync(shippingFailedEvent, CancellationToken.None);

        // Assert: Do not auto-refund, escalate for manual intervention
        result.CurrentState.Should().Be("EscalatedForManualIntervention");
        result.FailedStep.Should().Be("CreateShipment");
        result.FailureReason.Should().Contain("Carrier dispatch system unreachable");

        // Verify NO automatic RefundPaymentCommand was emitted
        var refundOutbox = await ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Type.Contains(nameof(RefundPaymentCommand)));
        refundOutbox.Should().BeNull();
    }

    [Fact]
    public async Task Scenario5_IdempotentOrderPlaced_ShouldReturnExistingSagaWithoutDuplication()
    {
        // Arrange
        await using var ctx = SagaDbContextFactory.Create();
        var coordinator = new OrderSagaCoordinator(ctx);

        var orderId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var orderPlacedEvent = new OrderPlacedIntegrationEvent(
            orderId,
            Guid.NewGuid(),
            "buyer@test.com",
            "Street 1",
            100m,
            [],
            correlationId);

        // Act: Handle same message twice
        var firstResult = await coordinator.HandleOrderPlacedAsync(orderPlacedEvent, CancellationToken.None);
        var secondResult = await coordinator.HandleOrderPlacedAsync(orderPlacedEvent, CancellationToken.None);

        // Assert
        secondResult.SagaId.Should().Be(firstResult.SagaId);
        (await ctx.SagaStates.CountAsync()).Should().Be(1);
    }
}
