using Moq;
using WordCardsApi.CustomExceptions;
using WordCardsApi.DTOs;
using WordCardsApi.Interfaces;
using WordCardsApi.Models;
using WordCardsApi.Services;

namespace WordCardsApi.Tests;

public class LearningSessionServiceTests
{
    [Fact]
    public async Task CreateSessionAsync_WithFewerThan100MatchingWords_ThrowsNotEnoughWords()
    {
        var userWordProvider = new Mock<IUserWordProvider>();
        userWordProvider
            .Setup(provider => provider.GetUserWordsByUserIdAsync("user-id"))
            .ReturnsAsync(Enumerable.Range(1, 99).Select(index => new UserWord
            {
                Id = $"word-{index}",
                UserId = "user-id",
                Language = "english"
            }));

        var sessionProvider = new Mock<ILearningSessionProvider>();
        var sessionWordProvider = new Mock<ISessionWordProvider>();
        var service = new LearningSessionService(
            userWordProvider.Object,
            sessionWordProvider.Object,
            sessionProvider.Object);

        await Assert.ThrowsAsync<NotEnoughWordsException>(() => service.CreateSessionAsync(
            new LearningSessionCreateDto { Language = " English " }, "user-id"));

        sessionProvider.Verify(provider => provider.CreateSessionAsync(It.IsAny<LearningSession>()), Times.Never);
        sessionWordProvider.Verify(provider => provider.CreateSessionWordsAsync(
            It.IsAny<IEnumerable<UserWord>>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateSessionAsync_With100MatchingWords_NormalizesFiltersAndCreatesSessionWords()
    {
        var words = Enumerable.Range(1, 100).Select(index => new UserWord
        {
            Id = $"word-{index}",
            UserId = "user-id",
            Language = "english",
            Category = "travel"
        }).ToList();
        var userWordProvider = new Mock<IUserWordProvider>();
        userWordProvider.Setup(provider => provider.GetUserWordsByUserIdAsync("user-id")).ReturnsAsync(words);

        var createdSession = new LearningSession { Id = "session-id" };
        var sessionProvider = new Mock<ILearningSessionProvider>();
        sessionProvider
            .Setup(provider => provider.CreateSessionAsync(It.IsAny<LearningSession>()))
            .ReturnsAsync(createdSession);
        var sessionWordProvider = new Mock<ISessionWordProvider>();
        sessionWordProvider
            .Setup(provider => provider.CreateSessionWordsAsync(It.IsAny<IEnumerable<UserWord>>(), "session-id"))
            .ReturnsAsync([]);

        var result = await new LearningSessionService(
            userWordProvider.Object,
            sessionWordProvider.Object,
            sessionProvider.Object).CreateSessionAsync(
                new LearningSessionCreateDto { Language = " English ", Category = " Travel " }, "user-id");

        Assert.Same(createdSession, result);
        sessionProvider.Verify(provider => provider.CreateSessionAsync(It.Is<LearningSession>(session =>
            session.UserId == "user-id" && session.Language == "english" && session.Category == "travel")), Times.Once);
        sessionWordProvider.Verify(provider => provider.CreateSessionWordsAsync(
            It.Is<IEnumerable<UserWord>>(selected => selected.Count() == 100), "session-id"), Times.Once);
    }

    [Fact]
    public async Task CreateSessionAsync_WhenUserIdIsEmpty_ReturnsNullWithoutLoadingWords()
    {
        var userWordProvider = new Mock<IUserWordProvider>();
        var service = new LearningSessionService(
            userWordProvider.Object,
            new Mock<ISessionWordProvider>().Object,
            new Mock<ILearningSessionProvider>().Object);

        var result = await service.CreateSessionAsync(new LearningSessionCreateDto(), "");

        Assert.Null(result);
        userWordProvider.Verify(provider => provider.GetUserWordsByUserIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteSessionAsync_DeletesSessionAndItsWords()
    {
        var sessionProvider = new Mock<ILearningSessionProvider>();
        var sessionWordProvider = new Mock<ISessionWordProvider>();
        var service = new LearningSessionService(
            new Mock<IUserWordProvider>().Object,
            sessionWordProvider.Object,
            sessionProvider.Object);
        var session = new LearningSession { Id = "session-id", UserId = "user-id" };

        await service.DeleteSessionAsync(session);

        sessionProvider.Verify(provider => provider.DeleteSessionAsync("session-id", "user-id"), Times.Once);
        sessionWordProvider.Verify(provider => provider.DeleteSessionWordsAsync("session-id"), Times.Once);
    }
}
