using Business.Services;
using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace VineyardApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;

        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Here you would typically validate the user credentials against a database
            // For this example, we'll just return a dummy response
            var user = await _userService.AuthenticateUser(request.Email, request.Password);
            if (user != null)
            {
                return Ok(new
                {
                    AccessToken = user.CurrentJwtId,
                    RefreshToken = user.RefreshJwtId,
                });
            }
            return Unauthorized(new { message = "Invalid username or password" });
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> RefreshToken(string token)
        {
            var user = await _userService.RefreshTokenHandler(token);
            if (user != null)
            {

                return Ok(new { accessToken = user.CurrentJwtId, refreshToken = user.RefreshJwtId });
            }
            return BadRequest();
        }
    }
}
