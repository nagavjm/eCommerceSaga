using BuildingBlocks.Common.Primitives;
using Identity.Domain.Enums;
using Identity.Domain.Events;

namespace Identity.Domain.Entities;

public class User : AggregateRoot<Guid>
{
    private readonly List<RefreshToken> _refreshTokens = [];

    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public UserRole Role { get; private set; } = UserRole.Customer;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { }

    public static User Create(
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        UserRole role = UserRole.Customer)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Role = role,
            CreatedAtUtc = DateTime.UtcNow
        };

        user.AddDomainEvent(new UserRegisteredDomainEvent(user.Id, user.Email, user.Role));

        return user;
    }

    public RefreshToken AddRefreshToken(string token, DateTime expiresOnUtc)
    {
        var refreshToken = RefreshToken.Create(Id, token, expiresOnUtc);
        _refreshTokens.Add(refreshToken);
        return refreshToken;
    }

    public void RevokeRefreshToken(string token)
    {
        var existingToken = _refreshTokens.FirstOrDefault(t => t.Token == token);
        existingToken?.Revoke();
    }
}
