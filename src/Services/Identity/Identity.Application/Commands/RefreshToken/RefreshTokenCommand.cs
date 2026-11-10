using System.Security.Claims;
using BuildingBlocks.Common.Results;
using FluentValidation;
using Identity.Application.Common.Interfaces;
using Identity.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<Result<AuthResponseDto>>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.AccessToken).NotEmpty().WithMessage("Access token is required.");
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token is required.");
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    private readonly IIdentityDbContext _dbContext;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RefreshTokenCommandHandler(
        IIdentityDbContext dbContext,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _dbContext = dbContext;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = _jwtTokenGenerator.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal is null)
        {
            return Error.Unauthorized("Auth.InvalidToken", "Invalid access token.");
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? principal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Error.Unauthorized("Auth.InvalidToken", "Invalid user claim in token.");
        }

        var user = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return Error.NotFound("User.NotFound", "User not found.");
        }

        var existingRefreshToken = user.RefreshTokens
            .FirstOrDefault(r => r.Token == request.RefreshToken);

        if (existingRefreshToken is null || !existingRefreshToken.IsActive)
        {
            return Error.Unauthorized("Auth.InvalidRefreshToken", "Refresh token is invalid or expired.");
        }

        // Rotate Refresh Token
        user.RevokeRefreshToken(request.RefreshToken);

        var (newAccessToken, expiresAt) = _jwtTokenGenerator.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        var tokenEntity = user.AddRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
        _dbContext.RefreshTokens.Add(tokenEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            newAccessToken,
            newRefreshToken,
            expiresAt);
    }
}
