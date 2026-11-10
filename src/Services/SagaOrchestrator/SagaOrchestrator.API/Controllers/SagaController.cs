using BuildingBlocks.Contracts.Events;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SagaOrchestrator.Application.Queries;
using SagaOrchestrator.Application.Services;

namespace SagaOrchestrator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SagaController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IOrderSagaCoordinator _coordinator;

    public SagaController(
        IMediator mediator,
        IOrderSagaCoordinator coordinator)
    {
        _mediator = mediator;
        _coordinator = coordinator;
    }

    [HttpGet("order/{orderId:guid}")]
    public async Task<IActionResult> GetSagaByOrderId(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSagaByOrderIdQuery(orderId), cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error.Code, message = result.Error.Description });
        }

        return Ok(result.Value);
    }

    [HttpGet("failed")]
    public async Task<IActionResult> GetFailedSagas(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFailedOrEscalatedSagasQuery(), cancellationToken);
        return Ok(result.Value);
    }

    // Direct event ingestion endpoints (supporting both HTTP gateway webhooks and message bus handlers)
    [HttpPost("events/order-placed")]
    public async Task<IActionResult> OnOrderPlaced(
        [FromBody] OrderPlacedIntegrationEvent evt,
        CancellationToken cancellationToken)
    {
        var result = await _coordinator.HandleOrderPlacedAsync(evt, cancellationToken);
        return Ok(result);
    }

    [HttpPost("events/stock-reserved")]
    public async Task<IActionResult> OnStockReserved(
        [FromBody] StockReservedIntegrationEvent evt,
        CancellationToken cancellationToken)
    {
        var result = await _coordinator.HandleStockReservedAsync(evt, cancellationToken);
        return Ok(result);
    }

    [HttpPost("events/stock-reservation-failed")]
    public async Task<IActionResult> OnStockReservationFailed(
        [FromBody] StockReservationFailedIntegrationEvent evt,
        CancellationToken cancellationToken)
    {
        var result = await _coordinator.HandleStockReservationFailedAsync(evt, cancellationToken);
        return Ok(result);
    }

    [HttpPost("events/payment-completed")]
    public async Task<IActionResult> OnPaymentCompleted(
        [FromBody] PaymentCompletedIntegrationEvent evt,
        CancellationToken cancellationToken)
    {
        var result = await _coordinator.HandlePaymentCompletedAsync(evt, cancellationToken);
        return Ok(result);
    }

    [HttpPost("events/payment-failed")]
    public async Task<IActionResult> OnPaymentFailed(
        [FromBody] PaymentFailedIntegrationEvent evt,
        CancellationToken cancellationToken)
    {
        var result = await _coordinator.HandlePaymentFailedAsync(evt, cancellationToken);
        return Ok(result);
    }

    [HttpPost("events/shipment-created")]
    public async Task<IActionResult> OnShipmentCreated(
        [FromBody] ShipmentCreatedIntegrationEvent evt,
        CancellationToken cancellationToken)
    {
        var result = await _coordinator.HandleShipmentCreatedAsync(evt, cancellationToken);
        return Ok(result);
    }

    [HttpPost("events/shipment-failed")]
    public async Task<IActionResult> OnShipmentFailed(
        [FromBody] ShipmentCreationFailedIntegrationEvent evt,
        CancellationToken cancellationToken)
    {
        var result = await _coordinator.HandleShipmentCreationFailedAsync(evt, cancellationToken);
        return Ok(result);
    }
}
