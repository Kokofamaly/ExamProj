using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WordCardsApi.DTOs;
using WordCardsApi.Extensions;
using WordCardsApi.Models;
using WordCardsApi.Services;

namespace WordCardsApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserWordController : ControllerBase
{
    private readonly UserWordService _userWordService;
    private readonly ILogger<UserWordController> _logger;

    public UserWordController(UserWordService userWordService, ILogger<UserWordController> logger)
    {
        _userWordService = userWordService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetWords()
    {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        var words = await _userWordService.GetUserWordsByUserIdAsync(userId!);

        if(words == null) return BadRequest();

        var result = words.Select(w => w.MapResponseDto());

        _logger.LogInformation("User {UserId} gets {NumberOfWords} words", userId, result.Count());

        return Ok(result);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetWord(string id)
    {
        var userId = User.GetUserId();
        var word = await _userWordService.GetUserWordAsync(id);

        if(word == null) return NotFound();
        if(word.UserId != userId) return Forbid();

        var wordResponseDto = word.MapResponseDto();

        _logger.LogInformation("User {UserId} gets word {WordId}", userId, wordResponseDto.Id);

        return Ok(wordResponseDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateWord(UserWordCreateDto wordCreateDto)
    {
        var userId = User.GetUserId();
        
        if(userId == null) return Unauthorized();

        var word = await _userWordService.CreateUserWordAsync(wordCreateDto, userId);
        var wordResponseDto = word.MapResponseDto();

        _logger.LogInformation("User {UserId} creates word {WordId}", userId, wordResponseDto.Id);

        return Ok(wordResponseDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWord(string id, UserWordUpdateDto wordUpdateDto)
    {
        var userId = User.GetUserId();
        var updatedWord = await _userWordService.UpdateUserWordAsync(id, wordUpdateDto);

        if(updatedWord == null) return BadRequest();
        if(updatedWord.UserId != userId) return Forbid();

        _logger.LogInformation("User {UserId} updates word {WordId}", userId, updatedWord.Id);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWord(string id)
    {
        var userId = User.GetUserId();
        var word = await _userWordService.GetUserWordAsync(id);

        if(word == null) return NotFound();
        if(word.UserId != userId) return Forbid();

        await _userWordService.DeleteUserWordAsync(word);

        _logger.LogInformation("User {UserId} deleted word {WordId}", userId, word.Id);

        return NoContent();
    }

}