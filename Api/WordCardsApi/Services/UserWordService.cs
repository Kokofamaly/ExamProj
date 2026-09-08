using WordCardsApi.DTOs;
using WordCardsApi.Infrastructure.Providers;
using WordCardsApi.Models;

namespace WordCardsApi.Services;

public class UserWordService
{
    private readonly UserWordProvider _userWordProvider;

    public UserWordService(UserWordProvider userWordProvider)
    {
        _userWordProvider = userWordProvider;
    }

    public async Task<UserWord> CreateUserWordAsync(UserWordCreateDto wordDto, string userId)
    {
        var userWord = new UserWord
        {
            Word = wordDto.Word.Trim().ToLowerInvariant(),
            Translation = wordDto.Translation.Trim().ToLowerInvariant(),
            UserId = userId,
            Language = wordDto.Language.Trim().ToLowerInvariant(),
            Category = wordDto.Category?.Trim().ToLowerInvariant(),
            UsageExample = wordDto.UsageExample?.Trim().ToLowerInvariant()
        };
        return await _userWordProvider.CreateUserWordAsync(userWord);
    }

    public async Task<IEnumerable<UserWord>> GetUserWordsByUserIdAsync(string userId)
    => await _userWordProvider.GetUserWordsByUserIdAsync(userId);

    public async Task<UserWord?> GetUserWordAsync(string wordId)
    => await _userWordProvider.GetUserWordAsync(wordId);


    public async Task<UserWord?> UpdateUserWordAsync(string wordId, UserWordUpdateDto wordUpdateDto)
    {
        wordUpdateDto.Word = wordUpdateDto.Word.Trim().ToLowerInvariant();
        wordUpdateDto.Translation = wordUpdateDto.Translation.Trim().ToLowerInvariant();
        wordUpdateDto.Language = wordUpdateDto.Language.Trim().ToLowerInvariant();
        wordUpdateDto.Category = wordUpdateDto.Category?.Trim().ToLowerInvariant();
        wordUpdateDto.UsageExample = wordUpdateDto.UsageExample?.Trim().ToLowerInvariant();

        var oldWord = await _userWordProvider.GetUserWordAsync(wordId);
        
        if(oldWord == null) return null;

        var word = await _userWordProvider.UpdateUserWordAsync(oldWord, wordUpdateDto);
        return word;
    }

    public async Task UpUserWordDifficultyLevelAsync(string wordId)
    => await _userWordProvider.UpUserWordDifficultyLevelAsync(wordId);

    public async Task ResetUserWordDifficultyLevelAsync(string wordId) 
    => await _userWordProvider.ResetUserWordDifficultyLevelAsync(wordId);

    public async Task DeleteUserWordAsync(UserWord userWord)
    => await _userWordProvider.DeleteUserWordAsync(userWord.Id!);
    
}