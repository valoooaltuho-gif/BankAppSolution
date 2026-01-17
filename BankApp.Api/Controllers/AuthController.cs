using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using BankApp.Infrastructure; // Для доступа к ApiUser
using BankApp.Api.DTOs; // Для доступа к LoginDto
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace BankApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApiUser> _userManager;
        private readonly SignInManager<ApiUser> _signInManager;
        private readonly IConfiguration _configuration;

        // Внедряем менеджеры Identity и конфигурацию через конструктор
        public AuthController(UserManager<ApiUser> userManager, SignInManager<ApiUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // 1. Находим пользователя по имени
            var user = await _userManager.FindByNameAsync(loginDto.UserName);
            if (user == null) return Unauthorized(new { message = "Неверное имя пользователя или пароль." });

            // 2. Проверяем пароль НАПРЯМУЮ через UserManager. 
            // Это обходит проблему с SignInManager в тестовой среде.
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (isPasswordValid)
            {
                // 3. Если успешно, генерируем JWT токен
                var token = GenerateJwtToken(user);
                return Ok(new { Token = token, Message = "Вход выполнен успешно." });
            }

            // 4. Если неверный пароль
            return Unauthorized(new { message = "Неверное имя пользователя или пароль." }); // 401 Unauthorized
        }


        // Вспомогательный метод для генерации JWT токена
        private string GenerateJwtToken(ApiUser user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("JWT Key is missing in configuration");

            var key = Encoding.ASCII.GetBytes(jwtKey);
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.Id ?? ""),
        new Claim(ClaimTypes.Name, user.UserName ?? "")
    };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                // Здесь часто происходит сбой, если key слишком короткий
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

}
