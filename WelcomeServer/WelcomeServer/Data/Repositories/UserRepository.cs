using Microsoft.EntityFrameworkCore;
using WelcomeServer.Data.Models;
using WelcomeServer.Data.Repositories.Interfaces;

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
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.ID == id);
        }

        public async Task<User> GetUserAsyncByName(string username)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == username);
        }
    }
}
