using WordCardsApi.DTOs;
using WordCardsApi.Models;

namespace WordCardsApi.Extensions;

public static class MappingExtension
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
    public static UserWordResponseDto MapResponseDto(this UserWord word)
    {
        var wordDto = new UserWordResponseDto
        {
            Id = word.Id!,
            Word = word.Word.StartStringWithCapitalNormalize(),
            Translation = word.Translation.StartStringWithCapitalNormalize(),
            Language = word.Language.StartStringWithCapitalNormalize(),
            Category = word.Category?.StartStringWithCapitalNormalize(),
            UsageExample = word.UsageExample?.StartStringWithCapitalNormalize()
        };

        return wordDto;
    }
}