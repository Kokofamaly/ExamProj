using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using WordCardsApi.CustomExceptions;
using WordCardsApi.DTOs;
using WordCardsApi.Infrastructure.Providers;
using WordCardsApi.Models;
using WordCardsApi.Enum;
using WordCardsApi.Interfaces;

namespace WordCardsApi.Services;

public class AuthService
{
    private readonly IPasswordHasher<User> _hasher;
    private readonly IUserProvider _userProvider;

    public AuthService(IPasswordHasher<User> hasher, IUserProvider userProvider)
    {
        _hasher = hasher;
        _userProvider = userProvider;
    }

    public async Task<LoginResult> LoginUserAsync(UserLoginDto userDto)
    {
        var user = await _userProvider.GetUserAsync(userDto.Email.ToLowerInvariant());

        if(user == null) return LoginResult.Fail(LoginErrorEnum.UserNotFound);

        var passwordVerification = _hasher.VerifyHashedPassword(user, user.HashedPassword, userDto.Password);

        if(passwordVerification == PasswordVerificationResult.Failed) return LoginResult.Fail(LoginErrorEnum.InvalidCredentials);
        
        return LoginResult.Success(user);
        
    }

    public async Task<RegisterResult> RegisterUserAsync(UserRegisterDto userDto)
    {
        try{
            if(userDto == null || String.IsNullOrEmpty(userDto.Name) || String.IsNullOrEmpty(userDto.Email) || String.IsNullOrEmpty(userDto.Password))
                return RegisterResult.Fail(RegisterErrorEnum.EmptyCredentials);
            
            var userToRegister = new User
            {
                Name = userDto.Name.Trim().ToLowerInvariant(),
                Email = userDto.Email.Trim().ToLowerInvariant(),
                HashedPassword = string.Empty
            };
            userToRegister.HashedPassword = _hasher.HashPassword(userToRegister, userDto.Password);
            
                var user = await _userProvider.CreateUserAsync(userToRegister);
            
            return RegisterResult.Success(user);
        }
        catch(MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            return RegisterResult.Fail(RegisterErrorEnum.EmailAlreadyExists);
        }

    }
}