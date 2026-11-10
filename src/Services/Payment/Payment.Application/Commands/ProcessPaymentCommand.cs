using System.Text.Json;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Messaging.Outbox;
using FluentValidation;
using MediatR;
using Payment.Application.Common.Interfaces;
using Payment.Application.DTOs;

namespace Payment.Application.Commands;

public sealed record ProcessPaymentCommand(
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    Guid CorrelationId) : IRequest<Result<PaymentDto>>;

public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId is required.");
        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("CustomerId is required.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(3).WithMessage("Currency is required.");
    }
}

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Result<PaymentDto>>
{
    private readonly IPaymentDbContext _dbContext;
    private readonly IPaymentGateway _paymentGateway;

    public ProcessPaymentCommandHandler(
        IPaymentDbContext dbContext,
        IPaymentGateway paymentGateway)
    {
        _dbContext = dbContext;
        _paymentGateway = paymentGateway;
    }

    public async Task<Result<PaymentDto>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var correlationId = request.CorrelationId == Guid.Empty ? Guid.NewGuid() : request.CorrelationId;

        var payment = Domain.Entities.Payment.Create(
            request.OrderId,
            request.CustomerId,
            request.Amount,
            request.Currency,
            correlationId);

        _dbContext.Payments.Add(payment);

        // Process through sandbox payment gateway
        var gatewayResult = await _paymentGateway.ProcessPaymentAsync(
            request.Amount,
            request.Currency,
            cancellationToken);

        OutboxMessage outboxMessage;

        if (gatewayResult.IsSuccess)
        {
            var txnRef = gatewayResult.TransactionReference ?? $"TXN-{Guid.NewGuid():N}";
            payment.MarkAsCompleted(txnRef);

            var integrationEvent = new PaymentCompletedIntegrationEvent(
                payment.OrderId,
                payment.Id,
                payment.Amount,
                txnRef,
                payment.CorrelationId);

            outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredOnUtc = DateTime.UtcNow,
                Type = typeof(PaymentCompletedIntegrationEvent).AssemblyQualifiedName ?? typeof(PaymentCompletedIntegrationEvent).Name,
                Payload = JsonSerializer.Serialize(integrationEvent),
                ProcessedOnUtc = null,
                RetryCount = 0
            };
        }
        else
        {
            var reason = gatewayResult.FailureReason ?? "Payment declined by issuing bank.";
            payment.MarkAsFailed(reason);

            var integrationEvent = new PaymentFailedIntegrationEvent(
                payment.OrderId,
                reason,
                payment.CorrelationId);

            outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredOnUtc = DateTime.UtcNow,
                Type = typeof(PaymentFailedIntegrationEvent).AssemblyQualifiedName ?? typeof(PaymentFailedIntegrationEvent).Name,
                Payload = JsonSerializer.Serialize(integrationEvent),
                ProcessedOnUtc = null,
                RetryCount = 0
            };
        }

        _dbContext.OutboxMessages.Add(outboxMessage);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = new PaymentDto(
            payment.Id,
            payment.OrderId,
            payment.CustomerId,
            payment.Amount,
            payment.Currency,
            payment.Status.ToString(),
            payment.TransactionReference,
            payment.FailureReason,
            payment.CreatedAtUtc,
            payment.CompletedAtUtc);

        return dto;
    }
}
