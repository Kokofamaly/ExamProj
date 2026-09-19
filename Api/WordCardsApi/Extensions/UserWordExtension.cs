using WordCardsApi.DTOs;
using WordCardsApi.Models;

namespace WordCardsApi.Extensions;

public static class UserWordExtension
{
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