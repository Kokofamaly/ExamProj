using Moq;
using WordCardsApi.Interfaces;
using WordCardsApi.Models;
using WordCardsApi.Services;

namespace WordCardsApi.Tests;

public class RefreshTokenServiceTests
{
    [Fact]
    public async Task GenerateTokenAsync_PersistsHashedTokenAndReturnsRawToken()
    {
        var provider = new Mock<IRefreshTokenProvider>();
        provider
            .Setup(item => item.CreateTokenAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken token) => token);

        var rawToken = await new RefreshTokenService(provider.Object).GenerateTokenAsync("user-id");

        var savedToken = provider.Invocations.Single().Arguments[0] as RefreshToken;
        Assert.NotNull(savedToken);
        Assert.NotEqual(rawToken, savedToken.HashedToken);
        Assert.Equal("user-id", savedToken.UserId);
        Assert.True(savedToken.ExpiresAt > savedToken.CreatedAt);
        Assert.False(string.IsNullOrWhiteSpace(rawToken));
    }

    [Fact]
    public async Task ValidateTokenAsync_WhenTokenIsValid_ReturnsTokenUsingItsHash()
    {
        var provider = new Mock<IRefreshTokenProvider>();
        var storedToken = new RefreshToken
        {
            UserId = "user-id",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10)
        };
        provider
            .Setup(item => item.GetTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(storedToken);

        var result = await new RefreshTokenService(provider.Object).ValidateTokenAsync("raw-token");

        Assert.Same(storedToken, result);
        var hashedArgument = provider.Invocations.Single().Arguments[0] as string;
        Assert.NotEqual("raw-token", hashedArgument);
        Assert.False(string.IsNullOrWhiteSpace(hashedArgument));
    }

    [Fact]
    public async Task ValidateTokenAsync_WhenTokenIsExpired_ReturnsNull()
    {
        var provider = new Mock<IRefreshTokenProvider>();
        provider
            .Setup(item => item.GetTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(new RefreshToken { ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1) });

        var result = await new RefreshTokenService(provider.Object).ValidateTokenAsync("raw-token");

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateTokenAsync_WhenTokenIsRevoked_ReturnsNull()
    {
        var provider = new Mock<IRefreshTokenProvider>();
        provider
            .Setup(item => item.GetTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(new RefreshToken
            {
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
                RevokedAt = DateTimeOffset.UtcNow.AddMinutes(-1)
            });

        var result = await new RefreshTokenService(provider.Object).ValidateTokenAsync("raw-token");

        Assert.Null(result);
    }

    [Fact]
    public async Task RevokeTokenAsync_SendsHashedTokenToProvider()
    {
        var provider = new Mock<IRefreshTokenProvider>();

        await new RefreshTokenService(provider.Object).RevokeTokenAsync("raw-token");

        var hashedArgument = provider.Invocations.Single().Arguments[0] as string;
        Assert.NotEqual("raw-token", hashedArgument);
        Assert.False(string.IsNullOrWhiteSpace(hashedArgument));
    }
}
