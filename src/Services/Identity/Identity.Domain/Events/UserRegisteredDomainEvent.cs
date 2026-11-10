using BuildingBlocks.Common.Primitives;
using Identity.Domain.Enums;

namespace Identity.Domain.Events;

public sealed record UserRegisteredDomainEvent(
    Guid UserId,
    string Email,
    UserRole Role) : IDomainEvent;
