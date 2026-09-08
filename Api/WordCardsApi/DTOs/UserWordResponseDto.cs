namespace WordCardsApi.DTOs;

public class UserWordResponseDto
{
    public string Id { get; set; }
    public string Word { get; set; }
    public string Translation { get; set; }
    public string Language { get; set; }
    public string? Category { get; set; }
    public string? UsageExample { get; set; }
}