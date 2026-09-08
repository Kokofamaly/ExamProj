using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WordCardsApi.DTOs;
using WordCardsApi.Infrastructure.Data;
using WordCardsApi.Infrastructure.Settings;
using WordCardsApi.Models;

namespace WordCardsApi.Infrastructure.Providers;

public class UserProvider
{    
    private readonly IMongoCollection<User> _users;

    public UserProvider(MongoDbContext context)
    {
        _users = context.Users;
    }

    public async Task<User?> GetUserAsync(string email)
    => await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
     public async Task<User?> GetUserByIdAsync(string id)
    => await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
    

    public async Task<User> CreateUserAsync(User user)
    {
        await _users.InsertOneAsync(user);
        return user;
    }

    public async Task<User> UpdateUserAsync(UserUpdateDto updatedUser, string userId)
    {
        var user = await _users.FindOneAndUpdateAsync(
            u => u.Id == userId, 
            Builders<User>.Update
                .Set(u => u.Email, updatedUser.Email)
                .Set(u => u.Name, updatedUser.Name), 
            new FindOneAndUpdateOptions<User>
                { ReturnDocument = ReturnDocument.After});
        
        return user;
    }
    public async Task DeleteUserAsync(string userId)
    => await _users.DeleteOneAsync(u => u.Id == userId);
    


}