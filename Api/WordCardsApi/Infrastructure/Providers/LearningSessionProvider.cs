using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WordCardsApi.Infrastructure.Data;
using WordCardsApi.Infrastructure.Settings;
using WordCardsApi.Models;

namespace WordCardsApi.Infrastructure.Providers;

public class LearningSessionProvider
{    
    private readonly IMongoCollection<LearningSession> _sessions;

    public LearningSessionProvider(MongoDbContext context)
    {
        _sessions = context.LearningSessions;
    }

    public async Task<LearningSession> CreateSessionAsync(LearningSession session)
    {
        await _sessions.InsertOneAsync(session);
        return session;
    }

    public async Task<IEnumerable<LearningSession>> GetSessionsByUserIdAsync(string userId)
    => await _sessions.Find(s => s.UserId == userId).ToListAsync();
    public async Task<LearningSession?> GetSessionAsync(string sessionId) 
    => await _sessions.Find(s => s.Id == sessionId).FirstOrDefaultAsync();

    public async Task DeleteSessionAsync(string sessionId, string userId) 
    => await _sessions.DeleteOneAsync(s => s.Id == sessionId && s.UserId == userId);
}