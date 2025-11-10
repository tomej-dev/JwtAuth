using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Net.Mail;
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
    private readonly EmailService _emailService;

    public AuthController(IConfiguration config, UserService users, EmailService emailService)
    {
        _config = config;
        _users = users;
        _emailService = emailService;
    }

    // ---------------------------
    // Registro com confirmação de e-mail
    // ---------------------------
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) ||
            string.IsNullOrWhiteSpace(req.Email) ||
            string.IsNullOrWhiteSpace(req.Password))
        {
            return BadRequest("Todos os campos são obrigatórios.");
        }

        var email = req.Email.ToLowerInvariant().Trim();

        // Valida formato do e-mail
        try { var _ = new MailAddress(email); }
        catch { return BadRequest("E-mail inválido."); }

        // Evita duplicação
        var existingEmail = await _users.GetByEmailAsync(email);
        if (existingEmail != null)
            return BadRequest("E-mail já cadastrado.");

        // Cria usuário com e-mail não confirmado
        var user = await _users.CreateAsync(req.Username, email, req.Password);
        user.IsEmailConfirmed = false;
        user.EmailConfirmationToken = Guid.NewGuid().ToString();
        user.CreatedAt = DateTime.UtcNow;
        await _users.UpdateAsync(user);

        // Monta link de confirmação
        var confirmUrl = $"http://localhost:5000/api/auth/confirm?token={user.EmailConfirmationToken}";

        // Envia e-mail
        await _emailService.SendEmailAsync(
            user.Email,
            "Confirme seu cadastro",
            $"<h2>Olá, {user.Username}!</h2>" +
            $"<p>Obrigado por se registrar. Confirme seu e-mail clicando abaixo:</p>" +
            $"<p><a href='{confirmUrl}'>Confirmar meu e-mail</a></p>"
        );

        return Ok("Usuário criado. Verifique seu e-mail para confirmar o cadastro.");
    }

    // ---------------------------
    // Confirmação do e-mail
    // ---------------------------
    [HttpGet("confirm")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token))
            return BadRequest("Token inválido.");

        var user = await _users.GetByConfirmationTokenAsync(token);
        if (user == null)
            return BadRequest("Token de confirmação inválido.");

        user.IsEmailConfirmed = true;
        user.EmailConfirmationToken = null;
        await _users.UpdateAsync(user);

        // ✅ URL do login do frontend
        var loginUrl = "https://jwt-auth-frontend-navy.vercel.app/";

        // ✅ Redireciona automaticamente o usuário
        return Redirect(loginUrl);
    }

    // ---------------------------
    // Login (somente após confirmação)
    // ---------------------------
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthRequest req)
    {
        var user = await _users.GetByEmailAsync(req.Email);
        if (user == null || !_users.CheckPassword(user, req.Password))
            return Unauthorized("E-mail ou senha inválidos.");

        if (!user.IsEmailConfirmed)
            return BadRequest("Confirme seu e-mail antes de fazer login.");

        var jwt = _config.GetSection("Jwt");
        var key = jwt["Key"];
        var issuer = jwt["Issuer"];
        var audience = jwt["Audience"];
        var expiryMinutes = int.Parse(jwt["ExpiryMinutes"] ?? "60");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key ?? string.Empty));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return Ok(new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = token.ValidTo
        });
    }
}
