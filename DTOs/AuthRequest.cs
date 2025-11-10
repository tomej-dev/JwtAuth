namespace JwtAuth.DTOs;

public class AuthRequest
{
    public string? Username { get; set; }
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;


    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}