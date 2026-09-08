using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WordCardsApi.Models;

public class SessionWord
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    [Required]
    public string SessionId { get; set; } = string.Empty;
    [Required]
    public string UserWordId { get; set; } = string.Empty;
    public bool? isCorrect { get; set; }
    [Required]
    public string Word { get; set; } = string.Empty;
    [Required]
    public string Translation { get; set; } = string.Empty;
    public string? UsageExample { get; set; }
    public int Order { get; set; } = 0;
}