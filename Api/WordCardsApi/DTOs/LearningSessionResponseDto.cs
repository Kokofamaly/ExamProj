namespace WordCardsApi.DTOs;

public class LearningSessionResponseDto
{
    public string Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? Language { get; set; }
    public string? Category { get; set; }
}