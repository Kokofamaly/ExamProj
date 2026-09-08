using MongoDB.Driver;
using WordCardsApi.Infrastructure.Data;
using WordCardsApi.Models;

namespace WordCardsApi.Infrastructure.Providers;

public class RefreshTokenProvider
{
    private readonly IMongoCollection<RefreshToken> _refreshTokens;

    public RefreshTokenProvider(MongoDbContext context)
    {
        _refreshTokens = context.RefreshTokens;
    }
    public async Task<RefreshToken> CreateTokenAsync(RefreshToken token)
    {
        await _refreshTokens.InsertOneAsync(token);
        return token;
    }

    public async Task<RefreshToken?> GetTokenAsync(string hashedToken)
    => await _refreshTokens.Find(t => t.HashedToken == hashedToken).FirstOrDefaultAsync();

    public async Task RevokeTokenAsync(string hashedToken)
    => await _refreshTokens.UpdateOneAsync(
        t => t.HashedToken == hashedToken, 
        Builders<RefreshToken>.Update.Set(t => t.RevokedAt, DateTimeOffset.UtcNow));
    
    
}