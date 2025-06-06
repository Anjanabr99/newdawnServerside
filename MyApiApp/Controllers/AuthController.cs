// using MyApiApp.Models;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.IdentityModel.Tokens;
// using System.IdentityModel.Tokens.Jwt;
// using System.Security.Claims;
// using System.Text;
// namespace MyApiApp.Controllers
// // {
// //     [ApiController]
// //     [Route("api/[controller]")]
// //     public class AuthController : ControllerBase
// //     {
// //         [HttpGet("ping")]
// //         public IActionResult Ping()
// //         {
// //             return Ok("API is working!");
// //         }
// //     }
// // }

// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class AuthController : ControllerBase
//     {
//         private readonly string jwtKey = "a1b2c3d4e5f6g7h8i9j0kLmNoPqRsTuVwXyZ123456";

//         [HttpPost("login")]
//         public IActionResult Login([FromBody] LoginModel model)
//         {
//             // In real apps, check DB for user
//             if (model.Email == "test@example.com" && model.Password == "password123")
//             {
//                 var tokenHandler = new JwtSecurityTokenHandler();
//                 var key = Encoding.UTF8.GetBytes(jwtKey);
//                 var tokenDescriptor = new SecurityTokenDescriptor
//                 {
//                     Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, model.Email) }),
//                     Expires = DateTime.UtcNow.AddHours(1),
//                     SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
//                 };

//                 var token = tokenHandler.CreateToken(tokenDescriptor);
//                 var jwt = tokenHandler.WriteToken(token);

//                 return Ok(new { token = jwt });
//             }

//             return Unauthorized(new { message = "Invalid credentials" });
//         }
//     }
// }

using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyApiApp.Data;
using MyApiApp.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyApiApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly string jwtKey = "a1b2c3d4e5f6g7h8i9j0kLmNoPqRsTuVwXyZ123456";

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User model)
        {
            var user = _context.Users.SingleOrDefault(u => u.UserId == model.UserId && u.Password == model.Password);
            if (user == null)
                return Unauthorized("Invalid credentials");

            var claims = new[]
            {
                new Claim("UserId", user.UserId.ToString()),
                new Claim("Email", user.Email),
                new Claim("UserType", user.UserType)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    SecurityAlgorithms.HmacSha256)
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                userType = user.UserType
            });
        }

        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirst("UserId")?.Value;
            var userType = User.FindFirst("UserType")?.Value;

            if (userType == "admin")
                return Ok(_context.Users.ToList());
            else
            {
                var user = _context.Users.SingleOrDefault(u => u.UserId.ToString() == userId);
                return Ok(user);
            }
        }
    }
}
