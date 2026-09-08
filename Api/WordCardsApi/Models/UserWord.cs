using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WordCardsApi.Models;

public class UserWord
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    [Required]
    [StringLength(maximumLength: 100, MinimumLength = 1)]
    public string Word { get; set; } = string.Empty;
    [Required]
    [StringLength(maximumLength: 100, MinimumLength = 1)]
    public string Translation { get; set; } = string.Empty;
    [Required]
    public string UserId { get; set; } = string.Empty;
    public int DifficultyLevel { get; set; } = 0;
    public string Language { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? UsageExample { get; set; }
}
