using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Results;
using KiraathaneTactics.Application.Features.Authentication.Common;

namespace KiraathaneTactics.Application.Features.Authentication.Rename;

public sealed class RenameCommandHandler(
    IUserRepository users,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork,
    IJwtTokenGenerator jwtTokenGenerator)
    : ICommandHandler<RenameCommand, AuthenticationResponse>
{
    public async Task<Result<AuthenticationResponse>> Handle(
        RenameCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
            return Result.Failure<AuthenticationResponse>(AuthenticationErrors.NotSignedIn);

        var user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result.Failure<AuthenticationResponse>(AuthenticationErrors.NotSignedIn);
        if (!user.IsActive)
            return Result.Failure<AuthenticationResponse>(AuthenticationErrors.UserInactive);

        var name = request.FullName.Trim();
        user.FullName = name;
        users.Update(user);
        await users.RenameWallAsync(userId, name, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var token = jwtTokenGenerator.Generate(user);
        return new AuthenticationResponse(
            user.Id, user.Email, user.FullName,
            string.IsNullOrWhiteSpace(user.RegisteredName) ? user.FullName : user.RegisteredName,
            user.CreatedAt, user.Role.ToString(), token.AccessToken, token.ExpiresAt);
    }
}
