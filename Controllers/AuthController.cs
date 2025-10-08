using AmazonReviewsCRM.Data;
using AmazonReviewsCRM.Models;
using AmazonReviewsCRM.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            Console.WriteLine($"[DEBUG] Login attempt for: {request.Email}");
            Console.WriteLine($"[DEBUG] Captcha token received: {!string.IsNullOrEmpty(request.CaptchaToken)}");
            
            // Temporarily bypass captcha for testing - remove this in production
            var captchaVerified = true;
            
            // Uncomment this line when you have the correct secret key configured
            // var captchaVerified = await VerifyCaptcha(request.CaptchaToken);
            
            Console.WriteLine($"[DEBUG] Captcha verified: {captchaVerified}");

            if (!captchaVerified)
            {
                Console.WriteLine($"[DEBUG] CAPTCHA verification failed for token: {request.CaptchaToken}");
                return BadRequest("CAPTCHA verification failed");
            }

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

        private async Task<bool> VerifyCaptcha(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("[DEBUG] Captcha token is null or empty");
                return false;
            }

            var secret = _config["ReCaptcha:SecretKey"];
            
            if (string.IsNullOrEmpty(secret) || secret == "YOUR_SECRET_KEY_FROM_GOOGLE_CONSOLE")
            {
                Console.WriteLine("[WARNING] ReCaptcha secret key not configured properly");
                // Return true for testing when secret key is not configured
                return true;
            }

            var client = new HttpClient();

            try
            {
                Console.WriteLine($"[DEBUG] Verifying captcha with Google API...");
                
                var formData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", secret),
                    new KeyValuePair<string, string>("response", token)
                });

                var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", formData);
                var json = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"[DEBUG] Google API response: {json}");

                var result = JsonSerializer.Deserialize<ReCaptchaResponse>(json);
                
                if (result?.ErrorCodes != null && result.ErrorCodes.Length > 0)
                {
                    Console.WriteLine($"[DEBUG] ReCaptcha errors: {string.Join(", ", result.ErrorCodes)}");
                }
                
                return result?.Success ?? false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Captcha verification failed: {ex.Message}");
                // Return true for testing when API call fails
                return true;
            }
            finally
            {
                client.Dispose();
            }
        }
    }



    public class ReCaptchaResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("challenge_ts")]
        public DateTime ChallengeTimeStamp { get; set; }

        [JsonPropertyName("hostname")]
        public string? Hostname { get; set; }

        [JsonPropertyName("error-codes")]
        public string[]? ErrorCodes { get; set; }
    }

}
