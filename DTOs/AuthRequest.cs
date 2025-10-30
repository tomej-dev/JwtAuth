namespace JwtAuth.DTOs;

public class AuthRequest
{
    public string? Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }


    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}