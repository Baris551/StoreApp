using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StoreApp.Data.Concrete;
using StoreApp.Web.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using StoreApp.Web.Models;

namespace StoreApp.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersApiController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;

        public UsersApiController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        private bool IsTemporaryEmail(string email)
        {
            var temporaryDomains = new[] { "10minutemail.com", "tempmail.com", "guerrillamail.com", "mailinator.com" };
            return temporaryDomains.Any(domain => email.ToLower().EndsWith(domain));
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterApi([FromBody] RegisterDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (IsTemporaryEmail(model.Email))
            {
                return BadRequest(new
                {
                    Message = "Geçici e-posta adresleriyle kayıt olamazsınız. Lütfen gerçek bir e-posta adresi kullanın."
                });
            }

            var user = new AppUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                DateAdded = DateTime.Now,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                return Ok(new
                {
                    Message = "Kayıt başarılı.",
                    UserId = user.Id
                });
            }

            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { Errors = errors });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
                return BadRequest(new { Message = "Geçersiz kullanıcı adı." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (result.Succeeded)
                return Ok(new { Token = GenerateJwtToken(user) });

            return Unauthorized(new { Message = "Giriş başarısız, şifre yanlış." });
        }

        private string GenerateJwtToken(AppUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["AppSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret anahtarı bulunamadı."));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName ?? "")
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
