using WordCardsApi.Models;
using WordCardsApi.DTOs;

namespace WordCardsApi.Interfaces;

public interface IUserProvider
{
    public Task<User?> GetUserAsync(string email);
     public Task<User?> GetUserByIdAsync(string id);
    public Task<User> CreateUserAsync(User user);
    public Task<User> UpdateUserAsync(UserUpdateDto updatedUser, string userId);
    public Task DeleteUserAsync(string userId);
}