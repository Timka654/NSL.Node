using Microsoft.EntityFrameworkCore;
using WelcomeServer.Data.Models;

namespace WelcomeServer.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> AddUserAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<User> GetUserAsync(Guid id)
        {
            var result = await _dbContext.Users.FirstOrDefaultAsync(u => u.ID == id);
            return result;
        }
    }
}
