using BuildingBlocks.Common.Primitives;
using FluentAssertions;
using Xunit;

namespace BuildingBlocks.Tests;

public class EntityTests
{
    private sealed class TestEntity : Entity<Guid>
    {
        public TestEntity(Guid id) : base(id) { }
    }

    private sealed class TestAggregate : AggregateRoot<Guid>
    {
        public TestAggregate(Guid id) : base(id) { }

        public void PerformBusinessAction()
        {
            AddDomainEvent(new TestDomainEvent(Id));
        }
    }

    private sealed record TestDomainEvent(Guid AggregateId) : IDomainEvent;

    [Fact]
    public void Entities_WithSameId_ShouldBeEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        // Assert
        (entity1 == entity2).Should().BeTrue();
        entity1.Equals(entity2).Should().BeTrue();
    }

    [Fact]
    public void Entities_WithDifferentId_ShouldNotBeEqual()
    {
        // Arrange
        var entity1 = new TestEntity(Guid.NewGuid());
        var entity2 = new TestEntity(Guid.NewGuid());

        // Assert
        (entity1 != entity2).Should().BeTrue();
        entity1.Equals(entity2).Should().BeFalse();
    }

    [Fact]
    public void AggregateRoot_ShouldCollectAndClearDomainEvents()
    {
        // Arrange
        var aggregate = new TestAggregate(Guid.NewGuid());

        // Act
        aggregate.PerformBusinessAction();

        // Assert
        aggregate.DomainEvents.Should().HaveCount(1);
        aggregate.DomainEvents.First().Should().BeOfType<TestDomainEvent>();

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }
}
