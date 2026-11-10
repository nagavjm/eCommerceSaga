using FluentAssertions;
using Identity.Application.Commands.Login;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Infrastructure.Authentication;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace Identity.UnitTests;

public class LoginCommandHandlerTests
{
    private readonly IdentityDbContext _dbContext;
    private readonly PasswordHasher _passwordHasher;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new IdentityDbContext(options);
        _passwordHasher = new PasswordHasher();
        _jwtTokenGenerator = new JwtTokenGenerator(Options.Create(new JwtOptions()));

        _handler = new LoginCommandHandler(_dbContext, _passwordHasher, _jwtTokenGenerator);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var password = "CorrectPassword123!";
        var passwordHash = _passwordHasher.HashPassword(password);
        var user = User.Create("valid@example.com", passwordHash, "Valid", "User", UserRole.Customer);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var command = new LoginCommand("valid@example.com", password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("valid@example.com");
        result.Value.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.Value.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var passwordHash = _passwordHasher.HashPassword("CorrectPassword123!");
        var user = User.Create("valid@example.com", passwordHash, "Valid", "User");
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var command = new LoginCommand("valid@example.com", "WrongPassword!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
    }
}
