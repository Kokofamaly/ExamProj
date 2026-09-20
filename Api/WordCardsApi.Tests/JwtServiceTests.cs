using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Moq;
using WordCardsApi.Infrastructure.Settings;
using WordCardsApi.Interfaces;
using WordCardsApi.Models;
using WordCardsApi.Services;

namespace WordCardsApi.Tests;

public class JwtServiceTests
{
    [Fact]
    public void GenerateToken_ContainsExpectedClaimsAndConfiguration()
    {
        var settings = new JwtSettings
        {
            Issuer = "word-cards",
            Audience = "word-cards-client",
            SignKey = "a-signing-key-that-is-long-enough-for-hmac"
        };
        var service = new JwtService(Options.Create(settings), new Mock<IUserProvider>().Object);
        var token = service.GenerateToken(new User
        {
            Id = "user-id",
            Name = "Alice",
            Email = "alice@example.com",
            Role = "User"
        });

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal(settings.Issuer, jwt.Issuer);
        Assert.Contains(settings.Audience, jwt.Audiences);
        Assert.Equal("user-id", jwt.Claims.Single(claim => claim.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal("alice@example.com", jwt.Claims.Single(claim => claim.Type == ClaimTypes.Email).Value);
        Assert.Equal("User", jwt.Claims.Single(claim => claim.Type == ClaimTypes.Role).Value);
        Assert.Equal("Alice", jwt.Claims.Single(claim => claim.Type == ClaimTypes.Name).Value);
    }

    [Fact]
    public async Task GenerateTokenAsync_LoadsUserBeforeGeneratingToken()
    {
        var userProvider = new Mock<IUserProvider>();
        userProvider
            .Setup(provider => provider.GetUserByIdAsync("user-id"))
            .ReturnsAsync(new User
            {
                Id = "user-id",
                Name = "Alice",
                Email = "alice@example.com",
                Role = "User"
            });
        var service = new JwtService(Options.Create(new JwtSettings
        {
            Issuer = "issuer",
            Audience = "audience",
            SignKey = "a-signing-key-that-is-long-enough-for-hmac"
        }), userProvider.Object);

        var token = await service.GenerateTokenAsync("user-id");

        Assert.False(string.IsNullOrWhiteSpace(token));
        userProvider.Verify(provider => provider.GetUserByIdAsync("user-id"), Times.Once);
    }
}
