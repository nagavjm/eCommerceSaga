namespace Payment.Application.Common.Interfaces;

public sealed record PaymentGatewayResult(
    bool IsSuccess,
    string? TransactionReference,
    string? FailureReason);

public interface IPaymentGateway
{
    Task<PaymentGatewayResult> ProcessPaymentAsync(
        decimal amount,
        string currency,
        CancellationToken cancellationToken);

    Task<PaymentGatewayResult> RefundPaymentAsync(
        string transactionReference,
        decimal amount,
        CancellationToken cancellationToken);
}
