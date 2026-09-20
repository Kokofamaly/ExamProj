using WordCardsApi.Models;

namespace WordCardsApi.Interfaces;

public interface ISessionWordProvider
{
    public Task<IEnumerable<SessionWord>> CreateSessionWordsAsync(IEnumerable<UserWord> words, string sessionId);
    public Task<SessionWord?> SetCorrectAsync(string id, bool isCorrect);
    public Task<IEnumerable<SessionWord>> GetSessionWordsAsync(string sessionId);
    public Task DeleteSessionWordsAsync(string sessionId);
}