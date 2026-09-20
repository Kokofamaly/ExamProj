using WordCardsApi.Enum;

namespace WordCardsApi.Models;

public record LoginResult(User? User, LoginErrorEnum? LoginErrorEnum)
{
    public bool Succeeded => User != null;
    public static LoginResult Success(User u) => new(u, null);
    public static LoginResult Fail(LoginErrorEnum error) => new(null, error);
}