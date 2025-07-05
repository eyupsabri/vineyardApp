using Entities;

namespace DAL
{
    public interface IUserRepository
    {
        public Task<User> GetUserByEmailAsync(string email);
    }
}
