using BuildingBlocks.Common.Primitives;

namespace Identity.Domain.Entities;

public class RefreshToken : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresOnUtc { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? RevokedOnUtc { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresOnUtc;
    public bool IsActive => RevokedOnUtc is null && !IsExpired;

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string token, DateTime expiresOnUtc)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresOnUtc = expiresOnUtc,
            CreatedOnUtc = DateTime.UtcNow
        };
    }

    public void Revoke()
    {
        RevokedOnUtc = DateTime.UtcNow;
    }
}
