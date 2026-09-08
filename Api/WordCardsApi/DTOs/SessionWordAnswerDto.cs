
namespace WordCardsApi.DTOs;

public class SessionWordAnswerDto
{
    public string Id { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string UserWordId { get; set; } = string.Empty;
    public bool isCorrect { get; set; }

}