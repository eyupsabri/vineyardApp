using Entities.DTOs;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Business.Services
{
    public class TokenHelper
    {
        private bool _isExpired;
        private bool _isValidToken;
        private string _tokenType;
        private string _email;

        public bool IsExpired => _isExpired;
        public bool IsValidToken => _isValidToken;
        public string TokenType => _tokenType;
        public string Email => _email;

        public TokenHelper(bool isExpired, bool isValidToken, string tokenType, string email)
        {
            _isExpired = isExpired;
            _isValidToken = isValidToken;
            _tokenType = tokenType;
            _email = email;
        }
    }
    public interface ITokenService
    {
        TokenHelper ValidateToken(string token);
        string GenerateToken(string email, bool isRefresh);
    }

    public class TokenService : ITokenService
    {
        private readonly JwtSettings _settings;
        private readonly JwtSecurityTokenHandler _handler = new();

        public TokenService(IOptions<JwtSettings> opts)
        {
            _settings = opts.Value;
        }

        public TokenHelper ValidateToken(string token)
        {

            var key = Encoding.UTF8.GetBytes(_settings.SecretKey);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _settings.Issuer,
                ValidAudience = _settings.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                ClaimsPrincipal principal = _handler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
                var emailClaim = principal.FindFirst(ClaimTypes.Email);
                var tokenTypeClaim = principal.FindFirst("token_type");
                var result = validatedToken is JwtSecurityToken jwtToken && jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
                if (result)
                {
                    return new TokenHelper(false, true, tokenTypeClaim.Value, emailClaim.Value);

                }

            }
            catch (SecurityTokenException ex)
            {

                if (ex.Message.Contains("Lifetime validation failed."))
                {
                    var jwtToken = _handler.ReadToken(token) as JwtSecurityToken;
                    var emailClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                    var tokenTypeClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == "token_type");
                    return new TokenHelper(true, true, tokenTypeClaim.Value, emailClaim.Value);

                }
                else
                {
                    return new TokenHelper(false, false, "", "");
                }
            }
            catch (SecurityTokenMalformedException e)
            {
                return new TokenHelper(false, false, "", "");
            }
            return new TokenHelper(false, false, "", "");
        }


        public string GenerateToken(string email, bool isRefresh)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenType = isRefresh ? "refresh" : "access";

            var claims = new[] {
                new Claim(ClaimTypes.Email, email),
                new Claim("token_type", tokenType)
            };

            var token = new JwtSecurityToken(_settings.Issuer,
              _settings.Issuer,
              claims,
              expires: isRefresh ? DateTime.Now.AddDays(30) : DateTime.Now.AddSeconds(15),
              signingCredentials: credentials);

            return _handler.WriteToken(token);
        }

    }
}
