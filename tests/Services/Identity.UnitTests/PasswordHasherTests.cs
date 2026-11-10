using FluentAssertions;
using Identity.Infrastructure.Authentication;
using Xunit;

namespace Identity.UnitTests;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_ShouldReturnValidBase64String()
    {
        // Act
        var hash = _hasher.HashPassword("P@ssword123!");

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        var bytes = Convert.FromBase64String(hash);
        bytes.Length.Should().Be(48); // 16 bytes salt + 32 bytes subkey
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "SecureEnterprisePassword#2026";
        var hash = _hasher.HashPassword(password);

        // Act
        var isValid = _hasher.VerifyPassword(password, hash);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithWrongPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "SecureEnterprisePassword#2026";
        var hash = _hasher.HashPassword(password);

        // Act
        var isValid = _hasher.VerifyPassword("WrongPassword123", hash);

        // Assert
        isValid.Should().BeFalse();
    }
}
