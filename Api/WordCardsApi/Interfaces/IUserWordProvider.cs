using WordCardsApi.Models;
using WordCardsApi.DTOs;

namespace WordCardsApi.Interfaces;

public interface IUserWordProvider
{
    public Task<UserWord> CreateUserWordAsync(UserWord word);
    public Task<IEnumerable<UserWord>> GetUserWordsByUserIdAsync(string userId);
    public Task<UserWord?> GetUserWordAsync(string wordId);
    public Task DeleteUserWordAsync(string wordId);
    public Task UpUserWordDifficultyLevelAsync(string wordId);
    public Task ResetUserWordDifficultyLevelAsync(string wordId);
    public Task<UserWord?> UpdateUserWordAsync(UserWord oldWord, UserWordUpdateDto newWord);
}