using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using WordCardsApi.Models;
using System.Text;
using Microsoft.Extensions.Options;
using WordCardsApi.Infrastructure.Settings;
using WordCardsApi.Infrastructure.Providers;

namespace WordCardsApi.Services;

public class JwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly UserProvider _userProvider;
    public JwtService(IOptions<JwtSettings> jwtSettings, UserProvider userProvider)
    {
        _jwtSettings = jwtSettings.Value;
        _userProvider = userProvider;
    }

    public async Task<string> GenerateTokenAsync(string userId) 
    => GenerateToken((await _userProvider.GetUserByIdAsync(userId))!);
    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id!),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.Name, user.Name)
        };
        
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SignKey)), 
            SecurityAlgorithms.HmacSha256
            );

        var token = new JwtSecurityToken(            
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return tokenString;

    }

}