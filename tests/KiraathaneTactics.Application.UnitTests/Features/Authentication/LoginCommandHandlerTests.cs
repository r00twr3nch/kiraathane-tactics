using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Results;
using KiraathaneTactics.Application.Features.Authentication.Common;
using KiraathaneTactics.Application.Features.Authentication.Login;
using KiraathaneTactics.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace KiraathaneTactics.Application.UnitTests.Features.Authentication;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly LoginCommandHandler _sut;

    public LoginCommandHandlerTests()
        => _sut = new LoginCommandHandler(_userRepository, _passwordHasher, _jwtTokenGenerator);

    [Fact]
    public async Task Handle_DogruBilgilerle_TokenDoner()
    {
        // Arrange
        var user = new User { Email = "test@test.com", PasswordHash = "hash", FullName = "Test", IsActive = true };
        _userRepository.GetByEmailAsync("test@test.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("Password1!", "hash").Returns(true);
        _jwtTokenGenerator.Generate(user).Returns(new JwtToken("token-123", DateTimeOffset.UtcNow.AddHours(1)));

        // Act
        var result = await _sut.Handle(
            new LoginCommand { Email = "test@test.com", Password = "Password1!" }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("token-123");
    }

    [Fact]
    public async Task Handle_KullaniciYoksa_InvalidCredentialsDoner()
    {
        // Arrange
        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _sut.Handle(
            new LoginCommand { Email = "yok@test.com", Password = "x" }, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthenticationErrors.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_PasifKullanici_UserInactiveDoner()
    {
        // Arrange
        var user = new User { Email = "test@test.com", PasswordHash = "hash", IsActive = false };
        _userRepository.GetByEmailAsync("test@test.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        // Act
        var result = await _sut.Handle(
            new LoginCommand { Email = "test@test.com", Password = "Password1!" }, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthenticationErrors.UserInactive);
    }
}
