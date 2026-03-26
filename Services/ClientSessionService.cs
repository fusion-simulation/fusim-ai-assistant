using System.Security.Claims;

namespace FusimAiAssiant.Services;

public class ClientSessionService
{
    public ClientSessionService(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        IsLoggedIn = true;
        Username = user.Identity?.Name ?? string.Empty;

        var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier);
        UserId = int.TryParse(userIdValue, out var userId) ? userId : 0;
    }

    public bool IsLoggedIn { get; }

    public int UserId { get; }

    public string Username { get; } = string.Empty;
}
