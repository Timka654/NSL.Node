using WelcomeServer.Data.Models;

namespace WelcomeServer.Data.Repositories
{
    public interface IUserRepository
    {
        public Task<User> AddUserAsync(User user);
        public Task<User> GetUserAsync(Guid id);
    }
}
