using WordCardsApi.DTOs;
using WordCardsApi.Models;

namespace WordCardsApi.Extensions;

public static class LearningSessionExtension
{
    public static LearningSessionResponseDto MapResponseDto(this LearningSession session)
    {
        var sessionDto = new LearningSessionResponseDto
        {
            Id = session.Id!,
            CreatedAt = session.CreatedAt,
            Category = session.Category?.StartStringWithCapitalNormalize(),
            Language = session.Language?.StartStringWithCapitalNormalize()
        };
        return sessionDto;
    }
}