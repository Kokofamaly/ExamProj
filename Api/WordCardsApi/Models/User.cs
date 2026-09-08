using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WordCardsApi.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    [Required]
    [StringLength(maximumLength: 30, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    public string Email { get; set; }  = string.Empty;
    [Required]
    public string HashedPassword { get; set; }  = string.Empty;
    public string Role { get; set; } = "User";

}

