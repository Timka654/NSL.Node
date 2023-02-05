using WelcomeServer.Data.Models;

namespace WelcomeServer.Data.Repositories.Interfaces
{
    public interface IUserRepository
    {
        public Task<User> AddUserAsync(User user);
        public Task<User> GetUserAsync(Guid id);
        public Task<User> GetUserAsyncByName(string username);
    }
}
