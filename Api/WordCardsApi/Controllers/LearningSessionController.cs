using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WordCardsApi.DTOs;
using WordCardsApi.Models;
using WordCardsApi.Services;
using WordCardsApi.Infrastructure.Providers;
using WordCardsApi.Extensions;
using WordCardsApi.Interfaces;

namespace WordCardsApi.Controllers;

[ApiController]
[Route("[controller]")]
public class LearningSessionController : ControllerBase
{
    private readonly LearningSessionService _learningSessionService;
    private readonly ISessionWordProvider _sessionWordProvider;
    private readonly UserWordService _userWordService;
    private readonly ILogger<LearningSessionController> _logger;
    public LearningSessionController(
        LearningSessionService learningSessionService, 
        ISessionWordProvider sessionWordProvider, 
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
        var userId = User.GetUserId();

        if(userId == null) return Unauthorized();

        var sessions = await _learningSessionService.GetLearningSessionsByUserIdAsync(userId);
;
        var sessionsDto = sessions.Select(s => s.MapResponseDto());

        _logger.LogInformation("User {UserId} gets {NumberOfSessions} sessions", userId, sessionsDto.Count());

        return Ok(sessionsDto);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSession(string id)
    {
        var userId = User.GetUserId();
        var session = await _learningSessionService.GetLearningSessionAsync(id);

        if(session == null) return NotFound();
        if(session.UserId != userId) return Forbid();

        var sessionDto = session.MapResponseDto();

        var sessionWords = await _sessionWordProvider.GetSessionWordsAsync(session.Id!);
        var sessionWordsDto = sessionWords.OrderByDescending(w => w.Order).Select(w => new SessionWordResponseDto
        {
            Id = w.Id!,
            SessionId = w.SessionId,
            UserWordId = w.UserWordId,
            IsCorrect = w.IsCorrect,
            Word = w.Word.StartStringWithCapitalNormalize(),
            Translation = w.Translation.StartStringWithCapitalNormalize(),
            UsageExample = w.UsageExample?.StartStringWithCapitalNormalize(),
            Order = w.Order
        });
        
        _logger.LogInformation("User {UserId} gets session {SessionId}", userId, sessionDto.Id);

        return Ok(new {session = sessionDto, sessionWords = sessionWordsDto});

    }

    [HttpPost]
    public async Task<IActionResult> CreateSession(LearningSessionCreateDto dto)
    {
        var userId = User.GetUserId();
        if(userId == null) return Unauthorized();

        var session = await _learningSessionService.CreateSessionAsync(dto, userId);

        if(session == null) return BadRequest();

        var sessionDto = session.MapResponseDto();

        _logger.LogInformation("User {UserId} creates session {SessionId}", userId, sessionDto.Id);

        return Ok(sessionDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> SessionWordAnswer(string id, SessionWordAnswerDto answerDto)
    {
        if(id != answerDto.SessionId) return BadRequest();

        await _sessionWordProvider.SetCorrectAsync(answerDto.Id, answerDto.IsCorrect);

        if(answerDto.IsCorrect)
            await _userWordService.ResetUserWordDifficultyLevelAsync(answerDto.UserWordId);
        else
            await _userWordService.UpUserWordDifficultyLevelAsync(answerDto.UserWordId);

        _logger.LogInformation("Session {SessionId} got answer", id);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSession(string id)
    {
        var userId = User.GetUserId();
        var session = await _learningSessionService.GetLearningSessionAsync(id);

        if(session == null) return NotFound();
        if(session.UserId != userId) return Forbid();

        await _learningSessionService.DeleteSessionAsync(session);
        
        _logger.LogInformation("User {UserId} deletes session {SessionId}", userId, session.Id);

        return NoContent();
    }

}