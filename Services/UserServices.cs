using JwtAuth.Data;
using JwtAuth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JwtAuth.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _hasher = new();

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        // 🔹 Busca por nome de usuário
        public async Task<User?> GetByUsernameAsync(string username) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        // 🔹 Busca por e-mail
        public async Task<User?> GetByEmailAsync(string email) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        // 🔹 Cria um novo usuário (com e-mail não confirmado)
        public async Task<User> CreateAsync(string username, string email, string password)
        {
            var user = new User
            {
                Username = username,
                Email = email,
                IsEmailConfirmed = false,
                EmailConfirmationToken = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _hasher.HashPassword(user, password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        // 🔹 Atualiza dados do usuário (ex: confirmar e-mail)
        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        // 🔹 Busca usuário pelo token de confirmação de e-mail
        public async Task<User?> GetByConfirmationTokenAsync(string token)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.EmailConfirmationToken == token);
        }

        // 🔹 Verifica senha
        public bool CheckPassword(User user, string password)
        {
            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}
