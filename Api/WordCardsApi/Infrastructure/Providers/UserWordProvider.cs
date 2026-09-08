using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WordCardsApi.DTOs;
using WordCardsApi.Infrastructure.Data;
using WordCardsApi.Infrastructure.Settings;
using WordCardsApi.Models;

namespace WordCardsApi.Infrastructure.Providers;

public class UserWordProvider
{    
    private readonly IMongoCollection<UserWord> _userWords;

    public UserWordProvider(MongoDbContext context)
    {
        _userWords = context.UserWords;
    }

    public async Task<UserWord> CreateUserWordAsync(UserWord word)
    {
        await _userWords.InsertOneAsync(word);
        return word;
    }

    public async Task<IEnumerable<UserWord>> GetUserWordsByUserIdAsync(string userId) 
    => await _userWords.Find(w => w.UserId == userId).ToListAsync();
    
    public async Task<UserWord?> GetUserWordAsync(string wordId)
    => await _userWords.Find(w => w.Id == wordId).FirstOrDefaultAsync();
    

    public async Task DeleteUserWordAsync(string wordId)
    => await _userWords.DeleteOneAsync(w => w.Id == wordId);
    
    public async Task UpUserWordDifficultyLevelAsync(string wordId)
    => await _userWords.UpdateOneAsync(w => w.Id == wordId, Builders<UserWord>.Update.Set(w => w.DifficultyLevel, 1));
    public async Task ResetUserWordDifficultyLevelAsync(string wordId)
    => await _userWords.UpdateOneAsync(w => w.Id == wordId, Builders<UserWord>.Update.Set(w => w.DifficultyLevel, 0));

    public async Task<UserWord?> UpdateUserWordAsync(UserWord oldWord, UserWordUpdateDto newWord)
    {
        var update = Builders<UserWord>.Update
        .Set(w => w.Word, newWord.Word)
        .Set(w => w.Translation, newWord.Translation)
        .Set(w => w.Language, newWord.Language)
        .Set(w => w.Category, newWord.Category)
        .Set(w => w.UsageExample, newWord.UsageExample);

        var result = await _userWords.FindOneAndUpdateAsync(
            w => w.Id == oldWord.Id, 
            update, 
            new FindOneAndUpdateOptions<UserWord>
            { ReturnDocument = ReturnDocument.After});
        
        return result;
    }
}