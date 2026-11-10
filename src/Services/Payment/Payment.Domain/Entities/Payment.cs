using BuildingBlocks.Common.Primitives;
using Payment.Domain.Enums;
using Payment.Domain.Events;

namespace Payment.Domain.Entities;

public class Payment : AggregateRoot<Guid>
{
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public PaymentStatus Status { get; private set; }
    public string? TransactionReference { get; private set; }
    public string? FailureReason { get; private set; }
    public Guid CorrelationId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    private Payment() { }

    public static Payment Create(
        Guid orderId,
        Guid customerId,
        decimal amount,
        string currency,
        Guid correlationId)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty.", nameof(orderId));
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty.", nameof(customerId));
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        return new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CustomerId = customerId,
            Amount = amount,
            Currency = string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant(),
            Status = PaymentStatus.Pending,
            CorrelationId = correlationId == Guid.Empty ? Guid.NewGuid() : correlationId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void MarkAsCompleted(string transactionReference)
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot complete payment with current status '{Status}'.");
        }

        Status = PaymentStatus.Completed;
        TransactionReference = transactionReference;
        CompletedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new PaymentCompletedDomainEvent(Id, OrderId, Amount, transactionReference, CorrelationId));
    }

    public void MarkAsFailed(string reason)
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot mark payment as failed with current status '{Status}'.");
        }

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        CompletedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new PaymentFailedDomainEvent(Id, OrderId, reason, CorrelationId));
    }

    public void MarkAsRefunded()
    {
        if (Status != PaymentStatus.Completed)
        {
            throw new InvalidOperationException($"Cannot refund payment with status '{Status}'. Only Completed payments can be refunded.");
        }

        Status = PaymentStatus.Refunded;
        CompletedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new PaymentRefundedDomainEvent(Id, OrderId, Amount, CorrelationId));
    }
}
