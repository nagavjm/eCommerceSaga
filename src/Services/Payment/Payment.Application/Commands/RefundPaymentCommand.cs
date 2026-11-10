using System.Text.Json;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Messaging.Outbox;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Common.Interfaces;

namespace Payment.Application.Commands;

public sealed record RefundPaymentCommand(
    Guid OrderId,
    decimal Amount,
    Guid CorrelationId) : IRequest<Result>;

public class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId is required.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Refund amount must be greater than zero.");
    }
}

public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, Result>
{
    private readonly IPaymentDbContext _dbContext;
    private readonly IPaymentGateway _paymentGateway;

    public RefundPaymentCommandHandler(
        IPaymentDbContext dbContext,
        IPaymentGateway paymentGateway)
    {
        _dbContext = dbContext;
        _paymentGateway = paymentGateway;
    }

    public async Task<Result> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.OrderId == request.OrderId, cancellationToken);

        if (payment is null)
        {
            return Error.NotFound("Payment.NotFound", $"Payment for Order ID '{request.OrderId}' was not found.");
        }

        if (payment.Status != Domain.Enums.PaymentStatus.Completed)
        {
            return Error.Validation("Payment.NotRefundable", $"Payment with status '{payment.Status}' cannot be refunded.");
        }

        var gatewayResult = await _paymentGateway.RefundPaymentAsync(
            payment.TransactionReference ?? string.Empty,
            request.Amount,
            cancellationToken);

        if (!gatewayResult.IsSuccess)
        {
            return Error.Failure("Payment.RefundFailed", gatewayResult.FailureReason ?? "Payment gateway failed to process refund.");
        }

        payment.MarkAsRefunded();

        var integrationEvent = new PaymentRefundedIntegrationEvent(
            payment.OrderId,
            payment.Id,
            request.Amount,
            request.CorrelationId);

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            Type = typeof(PaymentRefundedIntegrationEvent).AssemblyQualifiedName ?? typeof(PaymentRefundedIntegrationEvent).Name,
            Payload = JsonSerializer.Serialize(integrationEvent),
            ProcessedOnUtc = null,
            RetryCount = 0
        };

        _dbContext.OutboxMessages.Add(outboxMessage);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
