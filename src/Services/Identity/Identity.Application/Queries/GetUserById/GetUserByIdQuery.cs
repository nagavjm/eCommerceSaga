using BuildingBlocks.Common.Results;
using Identity.Application.Common.Interfaces;
using Identity.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IRequest<Result<UserDto>>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IIdentityDbContext _dbContext;

    public GetUserByIdQueryHandler(IIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            return Error.NotFound("User.NotFound", $"User with ID '{request.UserId}' was not found.");
        }

        return new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.CreatedAtUtc);
    }
}
