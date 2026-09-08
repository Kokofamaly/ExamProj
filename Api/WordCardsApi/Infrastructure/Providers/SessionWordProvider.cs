using MongoDB.Driver;
using WordCardsApi.Infrastructure.Data;
using WordCardsApi.Models;

namespace WordCardsApi.Infrastructure.Providers;

public class SessionWordProvider
{
    private readonly IMongoCollection<SessionWord> _sessionWords;

    public SessionWordProvider(MongoDbContext context)
    {
        _sessionWords = context.SessionWords;
    }

    public async Task<IEnumerable<SessionWord>> CreateSessionWordsAsync(IEnumerable<UserWord> words, string sessionId)
    {
        var sessionWords = words.OrderBy(w => w.DifficultyLevel).Select(w => new SessionWord{ SessionId = sessionId, UserWordId = w.Id!, Translation = w.Translation, Word = w.Word, UsageExample = w.UsageExample, Order = w.DifficultyLevel}).ToList();
        await _sessionWords.InsertManyAsync(sessionWords);
        return sessionWords;
    }
    public async Task<SessionWord?> SetCorrectAsync(string id, bool isCorrect)
    => await _sessionWords.FindOneAndUpdateAsync(
        w => w.Id == id, 
        Builders<SessionWord>.Update.Set(w => w.isCorrect, isCorrect), 
        new FindOneAndUpdateOptions<SessionWord>
        {
            ReturnDocument = ReturnDocument.After
        });
    public async Task<IEnumerable<SessionWord>> GetSessionWordsAsync(string sessionId)
    => await _sessionWords.Find(w => w.SessionId == sessionId).ToListAsync();
    public async Task DeleteSessionWordsAsync(string sessionId)
    {
        await _sessionWords.DeleteManyAsync(w => w.SessionId == sessionId);
    }
}