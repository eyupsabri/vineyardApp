using Entities;

namespace Business.Services
{
    public interface IUserService
    {
        public Task<User> AuthenticateUser(string email, string password);
        public Task<User> GetUserByEmail(string email);
    }
}
