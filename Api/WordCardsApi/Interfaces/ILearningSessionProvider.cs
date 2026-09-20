using WordCardsApi.Models;

namespace WordCardsApi.Interfaces;

public interface ILearningSessionProvider
{
    public Task<LearningSession> CreateSessionAsync(LearningSession session);
    public Task<IEnumerable<LearningSession>> GetSessionsByUserIdAsync(string userId);
    public Task<LearningSession?> GetSessionAsync(string sessionId);
    public Task DeleteSessionAsync(string sessionId, string userId);

}