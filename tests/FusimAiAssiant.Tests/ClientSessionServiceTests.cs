using System.Security.Claims;
using FusimAiAssiant.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace FusimAiAssiant.Tests;

public sealed class ClientSessionServiceTests
{
    [Fact]
    public void Constructor_AuthenticatedPrincipal_MapsSessionProperties()
    {
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, "12"),
                    new Claim(ClaimTypes.Name, "guest")
                ], CookieAuthenticationDefaults.AuthenticationScheme))
            }
        };

        var session = new ClientSessionService(httpContextAccessor);

        Assert.True(session.IsLoggedIn);
        Assert.Equal(12, session.UserId);
        Assert.Equal("guest", session.Username);
    }

    [Fact]
    public void Constructor_AnonymousPrincipal_LeavesSessionLoggedOut()
    {
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        var session = new ClientSessionService(httpContextAccessor);

        Assert.False(session.IsLoggedIn);
        Assert.Equal(0, session.UserId);
        Assert.Equal(string.Empty, session.Username);
    }
}
