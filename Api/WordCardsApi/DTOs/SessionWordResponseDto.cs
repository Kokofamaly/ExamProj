
namespace WordCardsApi.DTOs;

public class SessionWordResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string UserWordId { get; set; } = string.Empty;
    public bool? isCorrect { get; set; }
    public string Word { get; set; } = string.Empty;
    public string Translation { get; set; } = string.Empty;
    public string? UsageExample { get; set; }
    public int Order { get; set; }
}