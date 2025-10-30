using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SampleController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult Public() => Ok("Endpoint público!");

    [Authorize]
    [HttpGet("protected")]
    public IActionResult Protected()
    {
        var username = User.Identity?.Name ?? User.FindFirst("sub")?.Value;
        return Ok(new
        {
            message = $"Bem-vindo, {username}!",
            claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }
}