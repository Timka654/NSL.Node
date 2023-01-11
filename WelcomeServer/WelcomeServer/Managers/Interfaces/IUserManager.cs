using WelcomeServer.Data.Models;

namespace WelcomeServer.Managers.Interfaces
{
    public interface IUserManager
    {
        public Task<User> SigninAsync(string userName, Guid id);
        public Task<User> SignupAsync(string userName, Guid id);
    }
}
