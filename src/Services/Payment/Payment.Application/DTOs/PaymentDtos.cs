namespace Payment.Application.DTOs;

public sealed record PaymentDto(
    Guid Id,
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    string Status,
    string? TransactionReference,
    string? FailureReason,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);

public sealed record ProcessPaymentRequest(
    Guid OrderId,
    decimal Amount,
    string Currency = "USD");

public sealed record RefundPaymentRequest(
    decimal Amount);
