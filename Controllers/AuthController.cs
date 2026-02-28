using FusimAiAssiant.Models;
using Microsoft.AspNetCore.Mvc;

namespace FusimAiAssiant.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var isSuccess = string.Equals(request.Username, "guest", StringComparison.Ordinal)
            && string.Equals(request.Password, "guest", StringComparison.Ordinal);

        if (!isSuccess)
        {
            return Unauthorized(new LoginResponse(false, "账号或密码错误，仅支持 guest/guest。", 0));
        }

        return Ok(new LoginResponse(true, "登录成功", 1));
    }
}
