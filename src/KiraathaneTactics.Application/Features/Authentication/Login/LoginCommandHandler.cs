using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Results;
using KiraathaneTactics.Application.Features.Authentication.Common;

namespace KiraathaneTactics.Application.Features.Authentication.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator)
    : ICommandHandler<LoginCommand, AuthenticationResponse>
{
    public async Task<Result<AuthenticationResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return AuthenticationErrors.InvalidCredentials;

        if (!user.IsActive)
            return AuthenticationErrors.UserInactive;

        var token = jwtTokenGenerator.Generate(user);

        var username = string.IsNullOrWhiteSpace(user.RegisteredName) ? user.FullName : user.RegisteredName;
        return new AuthenticationResponse(
            user.Id, user.Email, username, username,
            user.CreatedAt, user.Role.ToString(), token.AccessToken, token.ExpiresAt);
    }
}
