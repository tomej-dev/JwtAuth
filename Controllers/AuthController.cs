using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtAuth.Models;
using JwtAuth.Services;
using JwtAuth.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;


namespace JwtAuth.Controllers;

[ApiController]
[Route("api/[controller]")]

public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly UserService _users;

    public AuthController(IConfiguration config, UserService users)
    {
        _config = config;
        _users = users;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRequest req)
    {
        var existing = await _users.GetByUsernameAsync(req.Username);
        if (existing != null)
        {
            return BadRequest("Usuário já existe.");

            var user = await _users.CreateAsync(req.Username, req.Password);
            return Ok(new { user.Id, user.Username });)
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthRequest req) 
    { 
        
    }
}