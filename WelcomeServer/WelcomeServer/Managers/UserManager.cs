using Microsoft.EntityFrameworkCore;
using WelcomeServer.Data;
using WelcomeServer.Data.Models;
using WelcomeServer.Data.Repositories;
using WelcomeServer.Managers.Interfaces;

namespace WelcomeServer.Managers
{
    public class UserManager : IUserManager
    {
        private readonly IUserRepository _userRepository;
        public UserManager(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<User> SigninAsync(string username, Guid id)
        {
            var user = await _userRepository.GetUserAsync(id);
            if (user == null)
            {
                user = await SignupAsync(username, id);
            }

            return user;
        }

        public async Task<User> SignupAsync(string userName, Guid id)
        {
            var user = new User { UserName = userName, GameModel = string.Empty, ID = id };
            await _userRepository.AddUserAsync(user);
            return user;
        }
    }
}
