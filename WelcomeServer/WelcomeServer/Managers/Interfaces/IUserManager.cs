using WelcomeServer.Data.Models;

namespace WelcomeServer.Managers.Interfaces
{
    public interface IUserManager
    {
        public Task<User> SigninAsync(string userName, string password);
        public Task<User> SignupAsync(string userName, string password);
    }
}
