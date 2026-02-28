namespace FusimAiAssiant.Services;

public class ClientSessionService
{
    public bool IsLoggedIn { get; private set; }

    public int UserId { get; private set; }

    public string Username { get; private set; } = string.Empty;

    public event Action? SessionChanged;

    public void SignIn(string username, int userId)
    {
        IsLoggedIn = true;
        Username = username;
        UserId = userId;
        SessionChanged?.Invoke();
    }

    public void SignOut()
    {
        IsLoggedIn = false;
        Username = string.Empty;
        UserId = 0;
        SessionChanged?.Invoke();
    }
}
