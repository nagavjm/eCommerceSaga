using FluentAssertions;
using Identity.Application.Commands.Register;
using Identity.Application.Common.Interfaces;
using Identity.Infrastructure.Authentication;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace Identity.UnitTests;

public class RegisterCommandHandlerTests
{
    private readonly IdentityDbContext _dbContext;
    private readonly PasswordHasher _passwordHasher;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new IdentityDbContext(options);
        _passwordHasher = new PasswordHasher();
        _jwtTokenGenerator = new JwtTokenGenerator(Options.Create(new JwtOptions()));

        _handler = new RegisterCommandHandler(_dbContext, _passwordHasher, _jwtTokenGenerator);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRegisterUserSuccessfully()
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "newcustomer@example.com",
            Password: "SecurePassword1!",
            FirstName: "Alice",
            LastName: "Smith",
            Role: "Customer");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("newcustomer@example.com");
        result.Value.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.Value.RefreshToken.Should().NotBeNullOrWhiteSpace();

        var savedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == "newcustomer@example.com");
        savedUser.Should().NotBeNull();
        savedUser!.FirstName.Should().Be("Alice");
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldReturnConflict()
    {
        // Arrange
        var command1 = new RegisterCommand("dup@example.com", "Password123!", "First", "Last", "Customer");
        await _handler.Handle(command1, CancellationToken.None);

        var command2 = new RegisterCommand("DUP@example.com", "Password123!", "Second", "Last", "Customer");

        // Act
        var result = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.EmailAlreadyExists");
    }
}
