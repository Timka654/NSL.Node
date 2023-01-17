using Microsoft.EntityFrameworkCore;
using WelcomeServer.Data.Models;

namespace WelcomeServer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> op) : base (op) { }
        public DbSet<WelcomeServer.Data.Models.User> Users { get; set; }
        public DbSet<WelcomeServer.Data.Models.UserCredential> UsersCredentials { get; set; }

    }
}
