using Microsoft.EntityFrameworkCore;
using WelcomeServer.Data;
using WelcomeServer.Data.Models;
using WelcomeServer.Data.Repositories.Interfaces;
using WelcomeServer.Extensions;
using WelcomeServer.Managers.Interfaces;

namespace WelcomeServer.Managers
{
    public class UserManager : IUserManager
    {
        private readonly IUserRepository _userRepository;
        private readonly ICredentialsRepository _credentialsRepository;
        public UserManager(IUserRepository userRepository, ICredentialsRepository credentialsRepository)
        {
            _userRepository = userRepository;
            _credentialsRepository = credentialsRepository;
        }
        public async Task<User> SigninAsync(string username, string password)
        {
            var user = await _userRepository.GetUserAsyncByName(username);
            if (user == null)
            {
                return null;
            }

            var userCreds = await _credentialsRepository.GetCredentialsAsync(user.UserName);
            var passwordHash = password.GetDeterministicHashCode();
            if (passwordHash.ToString() == userCreds.PasswordHash)
            {
                return user;
            }

            return null;
        }

        public async Task<User> SignupAsync(string userName, string password)
        {
            var userWithThisName = await _userRepository.GetUserAsyncByName(userName);
            if (userWithThisName != null)
            {
                return null;
            }


            var user = new User { UserName = userName, GameModel = string.Empty, ID = Guid.NewGuid() };
            var userCreds = new UserCredential { Id = user.ID, PasswordHash = password.GetDeterministicHashCode().ToString(), Username = userName };
            await _userRepository.AddUserAsync(user);
            await _credentialsRepository.AddCredentialsAsync(userCreds);
            return user;
        }
    }
}
