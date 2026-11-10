namespace BuildingBlocks.Common.Primitives;

public interface IDomainEvent : MediatR.INotification
{
    Guid EventId => Guid.NewGuid();
    DateTime OccurredOnUtc => DateTime.UtcNow;
}
