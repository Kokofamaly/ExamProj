using System.Security.Cryptography;
using System.Text;
using WordCardsApi.Infrastructure.Providers;
using WordCardsApi.Models;

namespace WordCardsApi.Services;

public class RefreshTokenService
{
    private readonly RefreshTokenProvider _refreshProvider;
    public RefreshTokenService(RefreshTokenProvider refreshProvider)
    {
        _refreshProvider = refreshProvider;
    }

    public async Task<string> GenerateTokenAsync(string userId)
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(bytes);

        var hashedToken = HashToken(token);
        var refreshToken = new RefreshToken
        {
            HashedToken = hashedToken,
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        };

        await _refreshProvider.CreateTokenAsync(refreshToken);

        return token;
    }

    public async Task<RefreshToken?> ValidateTokenAsync(string providedToken)
    {
        var hashedProvidedToken = HashToken(providedToken);
        var token = await _refreshProvider.GetTokenAsync(hashedProvidedToken);

        if(token == null || token.IsExpired || token.IsRevoked) return null;
        
        return token;
    }

    public async Task RevokeTokenAsync(string providedToken)
    {
        var hashedProvidedToken = HashToken(providedToken);
        await _refreshProvider.RevokeTokenAsync(hashedProvidedToken);
    }

    private string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);

        return Convert.ToBase64String(hash);
    }

}