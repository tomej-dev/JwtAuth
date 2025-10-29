using JwtAuth.Data;
using JwtAuth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JwtAuth.Services;

public class UserService {

    private readonly AppDbContext _context;
    private readonly PasswordHasher<User> _hasher = new();

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username) 
     
        => await _context.Users
            .FirstOrDefaultAsync(user => user.Username == username);
    

    public async Task<User> CreateAsync(string username, string password) 
    {
        var user = new User { Username = username };
        user.PasswordHash = _hasher.HashPassword(user, password);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public bool CheckPassword(User user, string password)
    {
        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Success;
    }
}