using WordCardsApi.Models;

namespace WordCardsApi.Interfaces;

public interface IRefreshTokenProvider
{
    public Task<RefreshToken> CreateTokenAsync(RefreshToken token);
    public Task<RefreshToken?> GetTokenAsync(string hashedToken);
    public Task RevokeTokenAsync(string hashedToken);
}