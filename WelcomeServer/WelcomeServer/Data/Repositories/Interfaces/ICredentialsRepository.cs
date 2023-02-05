using WelcomeServer.Data.Models;

namespace WelcomeServer.Data.Repositories.Interfaces
{
    public interface ICredentialsRepository
    {
        Task AddCredentialsAsync(UserCredential userCreds);
        Task<UserCredential> GetCredentialsAsync(string username);
        Task<UserCredential> GetCredentialsAsyncByGuid(Guid x);
    }
}
