using Payment.Application.Common.Interfaces;

namespace Payment.Infrastructure.Services;

public class SandboxPaymentGateway : IPaymentGateway
{
    public Task<PaymentGatewayResult> ProcessPaymentAsync(
        decimal amount,
        string currency,
        CancellationToken cancellationToken)
    {
        // Deterministic simulation rules for enterprise testing:
        // 1. Amount 9999.00 or amount >= 10000.00 -> simulated decline
        // 2. Otherwise -> successful payment
        if (amount >= 10000.00m || amount == 9999.00m)
        {
            return Task.FromResult(new PaymentGatewayResult(
                false,
                null,
                "Card declined: Insufficient funds or transaction limit exceeded."));
        }

        var txnRef = $"SANDBOX-{Guid.NewGuid():N}"[..20].ToUpperInvariant();

        return Task.FromResult(new PaymentGatewayResult(
            true,
            txnRef,
            null));
    }

    public Task<PaymentGatewayResult> RefundPaymentAsync(
        string transactionReference,
        decimal amount,
        CancellationToken cancellationToken)
    {
        if (transactionReference.Contains("FAIL-REFUND", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new PaymentGatewayResult(
                false,
                null,
                "Refund declined by banking network."));
        }

        var refundRef = $"REFUND-{Guid.NewGuid():N}"[..20].ToUpperInvariant();

        return Task.FromResult(new PaymentGatewayResult(
            true,
            refundRef,
            null));
    }
}
