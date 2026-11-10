using BuildingBlocks.Contracts.Events;
using SagaOrchestrator.Application.DTOs;

namespace SagaOrchestrator.Application.Services;

public interface IOrderSagaCoordinator
{
    Task<SagaStatusDto> HandleOrderPlacedAsync(
        OrderPlacedIntegrationEvent evt,
        CancellationToken cancellationToken);

    Task<SagaStatusDto> HandleStockReservedAsync(
        StockReservedIntegrationEvent evt,
        CancellationToken cancellationToken);

    Task<SagaStatusDto> HandleStockReservationFailedAsync(
        StockReservationFailedIntegrationEvent evt,
        CancellationToken cancellationToken);

    Task<SagaStatusDto> HandlePaymentCompletedAsync(
        PaymentCompletedIntegrationEvent evt,
        CancellationToken cancellationToken);

    Task<SagaStatusDto> HandlePaymentFailedAsync(
        PaymentFailedIntegrationEvent evt,
        CancellationToken cancellationToken);

    Task<SagaStatusDto> HandleShipmentCreatedAsync(
        ShipmentCreatedIntegrationEvent evt,
        CancellationToken cancellationToken);

    Task<SagaStatusDto> HandleShipmentCreationFailedAsync(
        ShipmentCreationFailedIntegrationEvent evt,
        CancellationToken cancellationToken);
}
