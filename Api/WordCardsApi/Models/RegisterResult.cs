using WordCardsApi.Enum;

namespace WordCardsApi.Models;

public record RegisterResult(User? User, RegisterErrorEnum? RegisterErrorEnum)
{
    public bool Succeeded => User != null;
    public static RegisterResult Success(User u) => new(u, null);
    public static RegisterResult Fail(RegisterErrorEnum error) => new(null, error);
}