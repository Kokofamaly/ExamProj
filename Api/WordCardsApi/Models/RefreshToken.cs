using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WordCardsApi.Models;

public class RefreshToken
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string HashedToken { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsExpired => ExpiresAt <=  DateTimeOffset.UtcNow;
}