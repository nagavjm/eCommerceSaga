using FluentAssertions;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Events;
using Xunit;

namespace Identity.UnitTests;

public class UserAggregateTests
{
    [Fact]
    public void Create_ShouldInitializeUserAndPublishDomainEvent()
    {
        // Arrange & Act
        var user = User.Create(
            email: "CUSTOMER@example.com",
            passwordHash: "SecureHashedPassword123",
            firstName: "John",
            lastName: "Doe",
            role: UserRole.Customer);

        // Assert
        user.Id.Should().NotBeEmpty();
        user.Email.Should().Be("customer@example.com"); // normalized
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.Role.Should().Be(UserRole.Customer);
        user.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        user.DomainEvents.Should().HaveCount(1);
        var domainEvent = user.DomainEvents.First().Should().BeOfType<UserRegisteredDomainEvent>().Subject;
        domainEvent.UserId.Should().Be(user.Id);
        domainEvent.Email.Should().Be("customer@example.com");
        domainEvent.Role.Should().Be(UserRole.Customer);
    }

    [Fact]
    public void AddRefreshToken_ShouldStoreActiveToken()
    {
        // Arrange
        var user = User.Create("jane@example.com", "hash", "Jane", "Doe");
        var token = "SampleRefreshToken123";

        // Act
        user.AddRefreshToken(token, DateTime.UtcNow.AddDays(7));

        // Assert
        user.RefreshTokens.Should().HaveCount(1);
        var storedToken = user.RefreshTokens.First();
        storedToken.Token.Should().Be(token);
        storedToken.IsActive.Should().BeTrue();
        storedToken.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void RevokeRefreshToken_ShouldDeactivateToken()
    {
        // Arrange
        var user = User.Create("jane@example.com", "hash", "Jane", "Doe");
        var token = "SampleRefreshToken123";
        user.AddRefreshToken(token, DateTime.UtcNow.AddDays(7));

        // Act
        user.RevokeRefreshToken(token);

        // Assert
        var storedToken = user.RefreshTokens.First();
        storedToken.IsActive.Should().BeFalse();
        storedToken.RevokedOnUtc.Should().NotBeNull();
    }
}
