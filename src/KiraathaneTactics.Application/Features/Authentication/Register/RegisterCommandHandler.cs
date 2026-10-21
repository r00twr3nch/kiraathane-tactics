using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Results;
using KiraathaneTactics.Application.Features.Authentication.Common;
using KiraathaneTactics.Domain.Entities;

namespace KiraathaneTactics.Application.Features.Authentication.Register;

public sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator)
    : ICommandHandler<RegisterCommand, AuthenticationResponse>
{
    public async Task<Result<AuthenticationResponse>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await userRepository.EmailExistsAsync(email, cancellationToken))
            return AuthenticationErrors.EmailAlreadyExists;

        var user = new User
        {
            Email = email,
            FullName = request.FullName.Trim(),
            PasswordHash = passwordHasher.Hash(request.Password)
        };

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var token = jwtTokenGenerator.Generate(user);

        return new AuthenticationResponse(
            user.Id, user.Email, user.FullName, user.Role.ToString(),
            token.AccessToken, token.ExpiresAt);
    }
}
