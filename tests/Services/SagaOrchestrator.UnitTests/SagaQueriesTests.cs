using FluentAssertions;
using SagaOrchestrator.Application.Queries;
using SagaOrchestrator.Domain.Entities;
using SagaOrchestrator.Domain.Enums;

namespace SagaOrchestrator.UnitTests;

public class SagaQueriesTests
{
    [Fact]
    public async Task GetSagaByOrderId_ExistingSaga_ShouldReturnStatusDto()
    {
        // Arrange
        await using var ctx = SagaDbContextFactory.Create();
        var orderId = Guid.NewGuid();
        var saga = OrderSagaState.Start(orderId, Guid.NewGuid(), "customer@test.com", "Addr", 150m, Guid.NewGuid());
        saga.TransitionTo(SagaStateEnum.StockReserved, "StockReserved");
        ctx.SagaStates.Add(saga);
        await ctx.SaveChangesAsync();

        var handler = new GetSagaByOrderIdQueryHandler(ctx);

        // Act
        var result = await handler.Handle(new GetSagaByOrderIdQuery(orderId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.OrderId.Should().Be(orderId);
        result.Value.CurrentState.Should().Be("StockReserved");
        result.Value.CompletedSteps.Should().Contain("StockReserved");
    }

    [Fact]
    public async Task GetFailedOrEscalatedSagas_ShouldFilterCorrectly()
    {
        // Arrange
        await using var ctx = SagaDbContextFactory.Create();

        var failedSaga = OrderSagaState.Start(Guid.NewGuid(), Guid.NewGuid(), "a@test.com", "Addr", 100m, Guid.NewGuid());
        failedSaga.StartCompensation("Failed");
        failedSaga.CompleteCompensation();

        var escalatedSaga = OrderSagaState.Start(Guid.NewGuid(), Guid.NewGuid(), "b@test.com", "Addr", 200m, Guid.NewGuid());
        escalatedSaga.Escalate("Carrier issue");

        var completedSaga = OrderSagaState.Start(Guid.NewGuid(), Guid.NewGuid(), "c@test.com", "Addr", 300m, Guid.NewGuid());
        completedSaga.TransitionTo(SagaStateEnum.Completed, "NotificationSent");

        ctx.SagaStates.AddRange(failedSaga, escalatedSaga, completedSaga);
        await ctx.SaveChangesAsync();

        var handler = new GetFailedOrEscalatedSagasQueryHandler(ctx);

        // Act
        var result = await handler.Handle(new GetFailedOrEscalatedSagasQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(s => s.OrderId).Should().Contain([failedSaga.OrderId, escalatedSaga.OrderId]);
    }
}
