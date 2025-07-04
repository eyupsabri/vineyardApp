using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace VineyardApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public AuthController()
        {
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Here you would typically validate the user credentials against a database
            // For this example, we'll just return a dummy response
            if (request.Email == "admin" && request.Password == "password")
            {
                return Ok(new { Token = "dummy-jwt-token", RefreshToken = "dummy-refresh-token" });
            }
            return Unauthorized(new { message = "Invalid username or password" });
        }
    }
}
