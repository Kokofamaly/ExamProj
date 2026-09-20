using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WordCardsApi.DTOs;
using WordCardsApi.Enum;
using WordCardsApi.Extensions;
using WordCardsApi.Services;

namespace WordCardsApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly JwtService _jwt;
    private readonly ILogger<AuthController> _logger;
    private readonly RefreshTokenService _refreshTokenService;
    private readonly UserService _userService;

    public AuthController(AuthService authService, JwtService jwt, ILogger<AuthController> logger, RefreshTokenService refreshTokenService, UserService userService)
    {
        _authService = authService;
        _jwt = jwt;
        _logger = logger;
        _refreshTokenService = refreshTokenService;
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto userLoginDto)
    {
        var loginResult = await _authService.LoginUserAsync(userLoginDto);
        
        if(!loginResult.Succeeded) {  
            return loginResult.LoginErrorEnum switch
            {
                LoginErrorEnum.InvalidCredentials => BadRequest(new { message = "Invalid Credentials"}),
                LoginErrorEnum.UserNotFound => NotFound(new { message = "User Not Found" }),
                _ => BadRequest()
            };
            }

        var userToLogin = loginResult.User!;
        var userResponse = new UserResponseDto{ Email = userToLogin.Email, Name = userToLogin.Name };

        var refreshToken = await _refreshTokenService.GenerateTokenAsync(userToLogin.Id!);
        SetRefreshTokenCookies(refreshToken);

        var accessToken = _jwt.GenerateToken(userToLogin);
        var result = new { user = userResponse, accessToken = accessToken};

        _logger.LogInformation("User {UserId} logged in.", userToLogin.Id);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto userRegisterDto)
    {
        var registerResult = await _authService.RegisterUserAsync(userRegisterDto);

        if(!registerResult.Succeeded){
            return registerResult.RegisterErrorEnum switch
            {
                RegisterErrorEnum.EmailAlreadyExists => Conflict(new { message = $"User with {userRegisterDto.Email} email already exists."}),
                RegisterErrorEnum.EmptyCredentials => BadRequest(new { message = "You did not fill all the fields."}),
                _ => BadRequest()
            };
        }

        var createdUser = registerResult.User!;
        var userResponse = new UserResponseDto{ Email = createdUser.Email, Name = createdUser.Name };

        var refreshToken = await _refreshTokenService.GenerateTokenAsync(createdUser.Id!);
        SetRefreshTokenCookies(refreshToken);
        
        var accessToken = _jwt.GenerateToken(createdUser);
        var result = new { user = userResponse, accessToken = accessToken};

        _logger.LogInformation("User {UserId} registered account.", createdUser.Id);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAccessToken()
    {
        if(!HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken)) return Unauthorized();

        var token = await _refreshTokenService.ValidateTokenAsync(refreshToken);

        if(token == null) return Unauthorized();

        var accessToken = await _jwt.GenerateTokenAsync(token.UserId);

        _logger.LogInformation("{Date}: Refresh token response.", DateTimeOffset.UtcNow);
        _logger.LogInformation("Access Token {AccessToken} created", accessToken);

        return Ok(new {accessToken = accessToken});
    }

    [AllowAnonymous]
    [HttpGet("me")]
    public async Task<IActionResult> DefaultAuth()
    {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userId == null) return Unauthorized();

        var user = await _userService.GetUserAsync(userId);

        if(user == null) return BadRequest();

        var userDto = new UserResponseDto
        {
            Name = user.Name,
            Email = user.Email
        };
        
        _logger.LogInformation("User {UserId} authenticated", user.Id);

        return Ok(userDto);
    }


    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.GetUserId();
        if(HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken)) 
            await _refreshTokenService.RevokeTokenAsync(refreshToken);
        
        Response.Cookies.Delete("refreshToken");

        _logger.LogInformation("User {UserId} logged out.", userId);

        return NoContent();
    }

    private void SetRefreshTokenCookies(string refreshToken)
    {
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });
    }
}