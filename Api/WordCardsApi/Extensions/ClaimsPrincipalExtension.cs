

using System.Security.Claims;

namespace WordCardsApi.Extensions;

public static class ClaimsPrincipalExtension
{
    public static string? GetUserId(this ClaimsPrincipal claims) 
    => claims.FindFirstValue(ClaimTypes.NameIdentifier);
}