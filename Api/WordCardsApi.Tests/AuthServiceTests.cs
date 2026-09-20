using Microsoft.AspNetCore.Identity;
using Moq;
using WordCardsApi.DTOs;
using WordCardsApi.Enum;
using WordCardsApi.Interfaces;
using WordCardsApi.Models;
using WordCardsApi.Services;

namespace WordCardsApi.Tests;

public class AuthServiceTests
{
    private readonly Mock<IPasswordHasher<User>> _hasher = new();
    private readonly Mock<IUserProvider> _userProvider = new();

    [Fact]
    public async Task LoginUserAsync_WhenUserDoesNotExist_ReturnsUserNotFound()
    {
        _userProvider
            .Setup(provider => provider.GetUserAsync("person@example.com"))
            .ReturnsAsync((User?)null);

        var result = await CreateService().LoginUserAsync(new UserLoginDto
        {
            Email = "Person@Example.com",
            Password = "password"
        });

        Assert.False(result.Succeeded);
        Assert.Equal(LoginErrorEnum.UserNotFound, result.LoginErrorEnum);
        _hasher.Verify(hasher => hasher.VerifyHashedPassword(
            It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginUserAsync_WhenPasswordIsInvalid_ReturnsInvalidCredentials()
    {
        var user = new User { Email = "person@example.com", HashedPassword = "hashed" };
        _userProvider.Setup(provider => provider.GetUserAsync("person@example.com")).ReturnsAsync(user);
        _hasher
            .Setup(hasher => hasher.VerifyHashedPassword(user, "hashed", "wrong"))
            .Returns(PasswordVerificationResult.Failed);

        var result = await CreateService().LoginUserAsync(new UserLoginDto
        {
            Email = "PERSON@EXAMPLE.COM",
            Password = "wrong"
        });

        Assert.False(result.Succeeded);
        Assert.Equal(LoginErrorEnum.InvalidCredentials, result.LoginErrorEnum);
    }

    [Fact]
    public async Task LoginUserAsync_WhenPasswordIsValid_ReturnsUser()
    {
        var user = new User { Id = "user-id", Email = "person@example.com", HashedPassword = "hashed" };
        _userProvider.Setup(provider => provider.GetUserAsync("person@example.com")).ReturnsAsync(user);
        _hasher
            .Setup(hasher => hasher.VerifyHashedPassword(user, "hashed", "correct"))
            .Returns(PasswordVerificationResult.Success);

        var result = await CreateService().LoginUserAsync(new UserLoginDto
        {
            Email = "person@example.com",
            Password = "correct"
        });

        Assert.True(result.Succeeded);
        Assert.Same(user, result.User);
    }

    [Fact]
    public async Task RegisterUserAsync_NormalizesUserBeforePersisting()
    {
        var persistedUser = new User { Id = "user-id" };
        _hasher
            .Setup(hasher => hasher.HashPassword(It.IsAny<User>(), "secret"))
            .Returns("hashed-secret");
        _userProvider
            .Setup(provider => provider.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(persistedUser);

        var result = await CreateService().RegisterUserAsync(new UserRegisterDto
        {
            Name = "  Alice  ",
            Email = "  Alice@Example.com  ",
            Password = "secret"
        });

        var capturedUser = _userProvider.Invocations.Single().Arguments[0] as User;
        Assert.NotNull(capturedUser);
        Assert.Equal("alice", capturedUser.Name);
        Assert.Equal("alice@example.com", capturedUser.Email);
        Assert.Equal("hashed-secret", capturedUser.HashedPassword);
        Assert.Same(persistedUser, result.User);
    }

    [Fact]
    public async Task RegisterUserAsync_WhenCredentialsAreEmpty_DoesNotCallProvider()
    {
        var result = await CreateService().RegisterUserAsync(new UserRegisterDto
        {
            Name = "",
            Email = "person@example.com",
            Password = "secret"
        });

        Assert.False(result.Succeeded);
        Assert.Equal(RegisterErrorEnum.EmptyCredentials, result.RegisterErrorEnum);
        _userProvider.Verify(provider => provider.CreateUserAsync(It.IsAny<User>()), Times.Never);
    }

    private AuthService CreateService() => new(_hasher.Object, _userProvider.Object);
}
