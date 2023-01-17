namespace WelcomeServer.Data.Models
{
    public class UserCredential
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
    }
}
