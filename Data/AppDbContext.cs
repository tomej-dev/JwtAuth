using JwtAuth.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtAuth.Data 
{
    public class AppDbContext : DbContext 
    { 
    
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
    }
}