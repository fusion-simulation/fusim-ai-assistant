using System.Security.Claims;
using FusimAiAssiant.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FusimAiAssiant.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var isSuccess = string.Equals(request.Username, "guest", StringComparison.Ordinal)
            && string.Equals(request.Password, "guest", StringComparison.Ordinal);

        if (!isSuccess)
        {
            return Unauthorized(new LoginResponse(false, "账号或密码错误，仅支持 guest/guest。", 0, string.Empty));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, request.Username)
        };

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            });

        return Ok(new LoginResponse(true, "登录成功", 1, request.Username));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }

    [AllowAnonymous]
    [HttpGet("me")]
    public ActionResult<CurrentUserResponse> Me()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Ok(new CurrentUserResponse(false, 0, string.Empty));
        }

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = int.TryParse(userIdValue, out var parsedUserId) ? parsedUserId : 0;
        return Ok(new CurrentUserResponse(true, userId, User.Identity?.Name ?? string.Empty));
    }
}
