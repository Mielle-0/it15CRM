using AmazonReviewsCRM.Data;
using AmazonReviewsCRM.Models;
using AmazonReviewsCRM.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AmazonReviewsCRM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.Active);
            // var user = await _db.Users.FirstOrDefaultAsync(u => u.Name == request.Email && u.Active);

            // if (user == null)
            //     return Unauthorized("Invalid credentials");


            if (user == null)
            {
                Console.WriteLine($"[DEBUG] No active user found for email: {request.Email}");
                return Unauthorized("Email not found or user inactive");
            }

            var hashedInput = PasswordHelper.HashPassword(request.Password);


            Console.WriteLine($"[DEBUG] Input password: {request.Password}");
            Console.WriteLine($"[DEBUG] Input Hash: {hashedInput}");
            Console.WriteLine($"[DEBUG] DB Hash:    {user.PasswordHash}");

            if (!string.Equals(user.PasswordHash, hashedInput, StringComparison.OrdinalIgnoreCase))
                return Unauthorized("Invalid credentials");

            Console.WriteLine($"[DEBUG] Login succeeded for {user.Email}");

            // ✅ Build JWT
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email!),
                new Claim("role", user.Role ?? "user"),
                new Claim("id", user.UserId.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                message = "Login successful",
                token = tokenString,
                user = new { user.UserId, user.Name, user.Email, user.Role }
            });
        }




        // [HttpGet("me")]
        // public IActionResult GetCurrentUser()
        // {
        //     var role = User.FindFirst("role")?.Value ?? "Guest";
        //     var menuKeys = RolePerms.Permissions.ContainsKey(role)
        //         ? RolePerms.Permissions[role]
        //         : new List<string>();

        //     return Ok(new
        //     {
        //         role,
        //         menuKeys
        //     });
        // }
        
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            // Temporarily disable RBAC
            var role = "Guest"; 
            var menuKeys = new List<string>
            {
                "Dashboard", "games", "ReviewPage", "trends", 
                "Alerts", "Reports", "ModelManagement", "AdminSettings", 
                "ProfileSettings"
            };

            return Ok(new
            {
                role,
                menuKeys
            });
        }


    }


}
