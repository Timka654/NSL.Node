using Microsoft.EntityFrameworkCore;
using WelcomeServer.Data.Models;
using WelcomeServer.Data.Repositories.Interfaces;

namespace WelcomeServer.Data.Repositories
{
    public class CredentialRepository : ICredentialsRepository
    {
        private readonly AppDbContext _dbContext;

        public CredentialRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddCredentialsAsync(UserCredential userCreds)
        {
            await _dbContext.UsersCredentials.AddAsync(userCreds);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<UserCredential> GetCredentialsAsync(string username)
        {
            return await _dbContext.UsersCredentials.FirstOrDefaultAsync(x => x.Username == username);
        }

        public async Task<UserCredential> GetCredentialsAsyncByGuid(Guid x)
        {
            return await _dbContext.UsersCredentials.FirstOrDefaultAsync(cred => cred.Id.Equals(x));
        }
    }
}
