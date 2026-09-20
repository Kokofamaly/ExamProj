using Moq;
using WordCardsApi.DTOs;
using WordCardsApi.Interfaces;
using WordCardsApi.Models;
using WordCardsApi.Services;

namespace WordCardsApi.Tests;

public class UserWordServiceTests
{
    [Fact]
    public async Task CreateUserWordAsync_NormalizesFieldsAndAssociatesUser()
    {
        var provider = new Mock<IUserWordProvider>();
        var createdWord = new UserWord { Id = "word-id" };
        provider
            .Setup(item => item.CreateUserWordAsync(It.IsAny<UserWord>()))
            .ReturnsAsync(createdWord);

        var result = await new UserWordService(provider.Object).CreateUserWordAsync(new UserWordCreateDto
        {
            Word = "  Hello  ",
            Translation = "  Bonjour  ",
            Language = "  French  ",
            Category = "  Travel  ",
            UsageExample = "  Hello there.  "
        }, "user-id");

        var capturedWord = provider.Invocations.Single().Arguments[0] as UserWord;
        Assert.NotNull(capturedWord);
        Assert.Equal("hello", capturedWord.Word);
        Assert.Equal("bonjour", capturedWord.Translation);
        Assert.Equal("french", capturedWord.Language);
        Assert.Equal("travel", capturedWord.Category);
        Assert.Equal("hello there.", capturedWord.UsageExample);
        Assert.Equal("user-id", capturedWord.UserId);
        Assert.Same(createdWord, result);
    }

    [Fact]
    public async Task UpdateUserWordAsync_WhenWordDoesNotExist_ReturnsNullWithoutUpdating()
    {
        var provider = new Mock<IUserWordProvider>();
        provider.Setup(item => item.GetUserWordAsync("missing-id")).ReturnsAsync((UserWord?)null);

        var result = await new UserWordService(provider.Object).UpdateUserWordAsync("missing-id", new UserWordUpdateDto
        {
            Word = " New ",
            Translation = " Nouveau ",
            Language = " French "
        });

        Assert.Null(result);
        provider.Verify(item => item.UpdateUserWordAsync(It.IsAny<UserWord>(), It.IsAny<UserWordUpdateDto>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserWordAsync_WhenWordExists_NormalizesFieldsAndUpdatesIt()
    {
        var provider = new Mock<IUserWordProvider>();
        var oldWord = new UserWord { Id = "word-id" };
        var updatedWord = new UserWord { Id = "word-id", Word = "hello" };
        provider.Setup(item => item.GetUserWordAsync("word-id")).ReturnsAsync(oldWord);
        provider
            .Setup(item => item.UpdateUserWordAsync(oldWord, It.IsAny<UserWordUpdateDto>()))
            .ReturnsAsync(updatedWord);

        var result = await new UserWordService(provider.Object).UpdateUserWordAsync("word-id", new UserWordUpdateDto
        {
            Word = " Hello ",
            Translation = " Bonjour ",
            Language = " French ",
            Category = " Travel ",
            UsageExample = " A greeting "
        });

        var capturedDto = provider.Invocations.Single(item => item.Method.Name == nameof(IUserWordProvider.UpdateUserWordAsync)).Arguments[1] as UserWordUpdateDto;
        Assert.NotNull(capturedDto);
        Assert.Equal("hello", capturedDto.Word);
        Assert.Equal("bonjour", capturedDto.Translation);
        Assert.Equal("french", capturedDto.Language);
        Assert.Equal("travel", capturedDto.Category);
        Assert.Equal("a greeting", capturedDto.UsageExample);
        Assert.Same(updatedWord, result);
    }
    
    [Fact]
    public async Task DeleteUserWordAsync_DeletesUserWord()
    {
        var provider = new Mock<IUserWordProvider>();
        var wordToDelete = new UserWord(){Id = "word-id"};
        var service = new UserWordService(provider.Object);

        await service.DeleteUserWordAsync(wordToDelete);

        provider.Verify(provider => provider.DeleteUserWordAsync(It.IsAny<string>()), Times.Once);
    }
}
