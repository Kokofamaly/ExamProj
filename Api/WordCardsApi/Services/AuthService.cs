using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using WordCardsApi.CustomExceptions;
using WordCardsApi.DTOs;
using WordCardsApi.Infrastructure.Providers;
using WordCardsApi.Models;

namespace WordCardsApi.Services;

public class AuthService
{
    private readonly IPasswordHasher<User> _hasher;
    private readonly UserProvider _userProvider;

    public AuthService(IPasswordHasher<User> hasher, UserProvider userProvider)
    {
        _hasher = hasher;
        _userProvider = userProvider;
    }

    public async Task<User?> LoginUserAsync(UserLoginDto userDto)
    {
        var user = await _userProvider.GetUserAsync(userDto.Email.ToLowerInvariant());

        if(user == null) return null;

        var passwordVerification = _hasher.VerifyHashedPassword(user, user.HashedPassword, userDto.Password);

        if(passwordVerification == PasswordVerificationResult.Failed) return null;
        
        return user;
        
    }

    public async Task<User?> RegisterUserAsync(UserRegisterDto userDto)
    {
        try{
            if(userDto == null || String.IsNullOrEmpty(userDto.Name) || String.IsNullOrEmpty(userDto.Email) || String.IsNullOrEmpty(userDto.Password))
                return null;
            
            var userToRegister = new User
            {
                Name = userDto.Name.Trim().ToLowerInvariant(),
                Email = userDto.Email.Trim().ToLowerInvariant(),
                HashedPassword = string.Empty
            };
            userToRegister.HashedPassword = _hasher.HashPassword(userToRegister, userDto.Password);
            
                var user = await _userProvider.CreateUserAsync(userToRegister);
            
            return user;
        }
        catch(MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new EmailAlreadyExistsException(userDto.Email.Trim().ToLowerInvariant());
        }

    }
}