using DAL;
using Entities;

namespace Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        public UserService(IUnitOfWork unitOfWork, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
        }
        public async Task<User> AuthenticateUser(string email, string password)
        {
            var user = await _unitOfWork.UserRepo.GetUserByEmailAsync(email);
            if (user == null || user.PasswordHash != password)
            {
                return null;
            }
            var accessToken = _tokenService.GenerateToken(user.Email, false);
            var refreshToken = _tokenService.GenerateToken(user.Email, true);
            user.CurrentJwtId = accessToken;
            user.RefreshJwtId = refreshToken;
            await _unitOfWork.SaveChangesAsync();
            return user;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _unitOfWork.UserRepo.GetUserByEmailAsync(email);
        }

        public async Task<User> RefreshTokenHandler(string token)
        {
            var tokenHelper = _tokenService.ValidateToken(token);
            if (tokenHelper.IsValidToken && !tokenHelper.IsExpired && tokenHelper.TokenType == "refresh")
            {
                var user = await _unitOfWork.UserRepo.GetUserByEmailAsync(tokenHelper.Email);
                if (user != null && user.RefreshJwtId == token)
                {
                    var newAccessToken = _tokenService.GenerateToken(user.Email, false);
                    var newRefreshToken = _tokenService.GenerateToken(user.Email, true);
                    user.CurrentJwtId = newAccessToken;
                    user.RefreshJwtId = newRefreshToken;
                    await _unitOfWork.SaveChangesAsync();
                    return user;
                }
            }
            return null;
        }
    }
}
