using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ScoutingTool.Api.DTOs;

namespace ScoutingTool.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// POST /api/auth/login
        /// Generates a signed JWT bearer token with assigned role (Scout or Analyst).
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto request)
        {
            // Simple mock authentication check
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Role))
            {
                return BadRequest("Username and Role are required.");
            }

            var token = GenerateJwtToken(request.Username, request.Role);
            return Ok(new { token });
        }

        private string GenerateJwtToken(string username, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? "ChelseaFC_ScoutingTool_SecretKey_2026_MustBeLongEnough!"));

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role) // Attaches 'Scout' or 'Analyst' role to the token
            };

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "ScoutingToolApi",
                audience: _configuration["Jwt:Audience"] ?? "ScoutingToolClients",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
        
    }
}