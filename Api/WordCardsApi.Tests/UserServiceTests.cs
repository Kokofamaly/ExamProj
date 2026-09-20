using Moq;
using WordCardsApi.DTOs;
using WordCardsApi.Interfaces;
using WordCardsApi.Models;
using WordCardsApi.Services;

namespace WordCardsApi.Tests;

public class UserServiceTests
{
    [Fact]
    public async Task UpdateUserAsync_NormalizesFieldsBeforePersisting()
    {
        var userProvider = new Mock<IUserProvider>();
        var refreshProvider = new Mock<IRefreshTokenProvider>();
        var updatedUser = new User { Id = "user-id" };
        userProvider
            .Setup(provider => provider.UpdateUserAsync(It.IsAny<UserUpdateDto>(), "user-id"))
            .ReturnsAsync(updatedUser);

        var result = await new UserService(
            userProvider.Object,
            new RefreshTokenService(refreshProvider.Object)).UpdateUserAsync("user-id", new UserUpdateDto
            {
                Name = "  Alice  ",
                Email = "  Alice@Example.com  "
            });

        var capturedDto = userProvider.Invocations.Single().Arguments[0] as UserUpdateDto;
        Assert.NotNull(capturedDto);
        Assert.Equal("alice", capturedDto.Name);
        Assert.Equal("alice@example.com", capturedDto.Email);
        Assert.Same(updatedUser, result);
    }

    [Fact]
    public async Task DeleteUserAsync_DeletesUserAndRevokesRefreshToken()
    {
        var userProvider = new Mock<IUserProvider>();
        var refreshProvider = new Mock<IRefreshTokenProvider>();
        var service = new UserService(
            userProvider.Object,
            new RefreshTokenService(refreshProvider.Object));

        await service.DeleteUserAsync("user-id", "refresh-token");

        userProvider.Verify(provider => provider.DeleteUserAsync("user-id"), Times.Once);
        refreshProvider.Verify(provider => provider.RevokeTokenAsync(It.IsAny<string>()), Times.Once);
    }
}
