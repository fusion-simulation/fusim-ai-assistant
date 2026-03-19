using System.Security.Claims;
using FusimAiAssiant.Controllers;
using FusimAiAssiant.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FusimAiAssiant.Tests;

public sealed class AuthControllerTests
{
    [Fact]
    public async Task Login_ValidGuestCredentials_IssuesAuthenticationCookie()
    {
        var httpContext = CreateHttpContext();
        var controller = CreateController(httpContext);

        var result = await controller.Login(new LoginRequest("guest", "guest"));

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<LoginResponse>(ok.Value);
        Assert.True(payload.Success);
        Assert.Equal(1, payload.UserId);
        Assert.Equal("guest", payload.Username);
        Assert.Contains("Set-Cookie", httpContext.Response.Headers.Select(x => x.Key));
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        var httpContext = CreateHttpContext();
        var controller = CreateController(httpContext);

        var result = await controller.Login(new LoginRequest("guest", "bad-password"));

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var payload = Assert.IsType<LoginResponse>(unauthorized.Value);
        Assert.False(payload.Success);
        Assert.Equal(0, httpContext.Response.Headers.SetCookie.Count);
    }

    [Fact]
    public async Task Logout_ClearsAuthenticationCookie()
    {
        var httpContext = CreateHttpContext();
        var controller = CreateController(httpContext);

        var result = await controller.Logout();

        _ = Assert.IsType<OkResult>(result);
        Assert.True(httpContext.Response.Headers.SetCookie.Count > 0);
    }

    [Fact]
    public void Me_AuthenticatedUser_ReturnsIdentity()
    {
        var httpContext = CreateHttpContext(new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "7"),
            new Claim(ClaimTypes.Name, "guest")
        ], CookieAuthenticationDefaults.AuthenticationScheme)));
        var controller = CreateController(httpContext);

        var result = controller.Me();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<CurrentUserResponse>(ok.Value);
        Assert.True(payload.IsAuthenticated);
        Assert.Equal(7, payload.UserId);
        Assert.Equal("guest", payload.Username);
    }

    private static AuthController CreateController(HttpContext httpContext)
    {
        return new AuthController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    private static DefaultHttpContext CreateHttpContext(ClaimsPrincipal? user = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "FusimAiAssistant.Auth";
                options.LoginPath = "/login";
            });

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider(),
            User = user ?? new ClaimsPrincipal(new ClaimsIdentity())
        };

        return httpContext;
    }
}
