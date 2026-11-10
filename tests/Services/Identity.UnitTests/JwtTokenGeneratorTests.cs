using FluentAssertions;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Infrastructure.Authentication;
using Microsoft.Extensions.Options;
using Xunit;

namespace Identity.UnitTests;

public class JwtTokenGeneratorTests
{
    private readonly JwtTokenGenerator _tokenGenerator;

    public JwtTokenGeneratorTests()
    {
        var options = Options.Create(new JwtOptions
        {
            SecretKey = "SuperSecretKeyForTestingPurposesOnlyNeedsToBeLongEnough2026!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 15
        });

        _tokenGenerator = new JwtTokenGenerator(options);
    }

    [Fact]
    public void GenerateAccessToken_ShouldProduceValidToken()
    {
        // Arrange
        var user = User.Create("admin@test.com", "hash", "Admin", "User", UserRole.Admin);

        // Act
        var (token, expiresAtUtc) = _tokenGenerator.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        expiresAtUtc.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldProduceCryptographicallyRandomString()
    {
        // Act
        var token1 = _tokenGenerator.GenerateRefreshToken();
        var token2 = _tokenGenerator.GenerateRefreshToken();

        // Assert
        token1.Should().NotBeNullOrWhiteSpace();
        token2.Should().NotBeNullOrWhiteSpace();
        token1.Should().NotBe(token2);
    }
}
