using BuildingBlocks.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Common.Interfaces;
using Payment.Application.DTOs;

namespace Payment.Application.Queries;

public sealed record GetPaymentByOrderIdQuery(Guid OrderId) : IRequest<Result<PaymentDto>>;

public class GetPaymentByOrderIdQueryHandler : IRequestHandler<GetPaymentByOrderIdQuery, Result<PaymentDto>>
{
    private readonly IPaymentDbContext _dbContext;

    public GetPaymentByOrderIdQueryHandler(IPaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PaymentDto>> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.OrderId == request.OrderId, cancellationToken);

        if (payment is null)
        {
            return Error.NotFound("Payment.NotFound", $"Payment for Order ID '{request.OrderId}' was not found.");
        }

        return new PaymentDto(
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
    }
}

public sealed record GetPaymentByIdQuery(Guid PaymentId) : IRequest<Result<PaymentDto>>;

public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, Result<PaymentDto>>
{
    private readonly IPaymentDbContext _dbContext;

    public GetPaymentByIdQueryHandler(IPaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PaymentDto>> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment is null)
        {
            return Error.NotFound("Payment.NotFound", $"Payment with ID '{request.PaymentId}' was not found.");
        }

        return new PaymentDto(
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
    }
}
