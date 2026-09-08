using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WordCardsApi.DTOs;
using WordCardsApi.Models;
using WordCardsApi.Services;
using WordCardsApi.Infrastructure.Providers;
using WordCardsApi.Extensions;

namespace WordCardsApi.Controllers;

[ApiController]
[Route("[controller]")]
public class LearningSessionController : ControllerBase
{
    private readonly LearningSessionService _learningSessionService;
    private readonly SessionWordProvider _sessionWordProvider;
    private readonly UserWordService _userWordService;
    private readonly ILogger<LearningSessionController> _logger;
    public LearningSessionController(
        LearningSessionService learningSessionService, 
        SessionWordProvider sessionWordProvider, 
        UserWordService userWordService,
        ILogger<LearningSessionController> logger)
    {
        _learningSessionService = learningSessionService;
        _sessionWordProvider = sessionWordProvider;
        _userWordService = userWordService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetSessions()
    {
        var userId = GetUserId();

        if(userId == null) return Unauthorized();

        var sessions = await _learningSessionService.GetLearningSessionsByUserIdAsync(userId);
;
        var sessionsDto = sessions.Select(s => MapResponseDto(s));

        _logger.LogInformation($"{DateTimeOffset.UtcNow}: user:{userId} gets {sessionsDto.Count()} sessions");

        return Ok(sessionsDto);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSession(string id)
    {
        var userId = GetUserId();
        var session = await _learningSessionService.GetLearningSessionAsync(id);

        if(session == null) return NotFound();
        if(session.UserId != userId) return Forbid();

        var sessionDto = MapResponseDto(session);

        var sessionWords = await _sessionWordProvider.GetSessionWordsAsync(session.Id!);
        var sessionWordsDto = sessionWords.OrderByDescending(w => w.Order).Select(w => new SessionWordResponseDto
        {
            Id = w.Id!,
            SessionId = w.SessionId,
            UserWordId = w.UserWordId,
            isCorrect = w.isCorrect,
            Word = w.Word.StartStringWithCapitalNormalize(),
            Translation = w.Translation.StartStringWithCapitalNormalize(),
            UsageExample = w.UsageExample?.StartStringWithCapitalNormalize(),
            Order = w.Order
        });
        
        _logger.LogInformation($"{DateTimeOffset.UtcNow}: user:{userId} gets session {sessionDto.Id}");

        return Ok(new {session = sessionDto, sessionWords = sessionWordsDto});

    }

    [HttpPost]
    public async Task<IActionResult> CreateSession(LearningSessionCreateDto dto)
    {
        var userId = GetUserId();
        if(userId == null) return Unauthorized();

        var session = await _learningSessionService.CreateSessionAsync(dto, userId);

        if(session == null) return BadRequest();

        var sessionDto = MapResponseDto(session);

        _logger.LogInformation($"${DateTimeOffset.UtcNow}: user:{userId} creates session{sessionDto.Id}");

        return Ok(sessionDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> SessionWordAnswer(string id, SessionWordAnswerDto answerDto)
    {
        if(id != answerDto.SessionId) return BadRequest();

        await _sessionWordProvider.SetCorrectAsync(answerDto.Id, answerDto.isCorrect);

        if(answerDto.isCorrect)
            await _userWordService.ResetUserWordDifficultyLevelAsync(answerDto.UserWordId);
        else
            await _userWordService.UpUserWordDifficultyLevelAsync(answerDto.UserWordId);

        _logger.LogInformation($"{DateTimeOffset.UtcNow}: session answer");

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSession(string id)
    {
        var userId = GetUserId();
        var session = await _learningSessionService.GetLearningSessionAsync(id);

        if(session == null) return NotFound();
        if(session.UserId != userId) return Forbid();

        await _learningSessionService.DeleteSessionAsync(session);
        
        _logger.LogInformation($"${DateTimeOffset.UtcNow}: user:{userId} deletes session{session.Id}");

        return NoContent();
    }
    private string? GetUserId()
    {
        return HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private LearningSessionResponseDto MapResponseDto(LearningSession session)
    {
        var sessionDto = new LearningSessionResponseDto
        {
            Id = session.Id!,
            CreatedAt = session.CreatedAt,
            Category = session.Category?.StartStringWithCapitalNormalize(),
            Language = session.Language?.StartStringWithCapitalNormalize()
        };
        return sessionDto;
    }

}