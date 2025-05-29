using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StoreApp.Data.Concrete;
using StoreApp.Web.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

// Bu sınıf, kullanıcı kayıt ve giriş işlemlerini API üzerinden yöneten bir controller'dır.
namespace StoreApp.Web.Controllers.Api
{
    // [Route("api/[controller]")]: API rotasını tanımlar (örneğin, /api/UsersApi)
    // [ApiController]: Bu sınıfın bir API controller'ı olduğunu belirtir
    [Route("api/[controller]")]
    [ApiController]
    public class UsersApiController : ControllerBase
    {
        // Kullanıcı yönetimini sağlayan Identity servisi
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        // Uygulama ayarlarını (örneğin, JWT anahtarı) okuyan yapılandırma servisi
        private readonly IConfiguration _configuration;

        // Constructor: Bağımlılıkları (userManager, signInManager, configuration) alır
        public UsersApiController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager; // Kullanıcı yönetimini başlatır
            _signInManager = signInManager; // Giriş/çıkış işlemlerini başlatır
            _configuration = configuration; // Yapılandırma ayarlarını başlatır
        }

        // Geçici e-posta adreslerini kontrol eden yardımcı metod
        // private bool IsTemporaryEmail(string email)
        // {
        //     // Geçici e-posta servislerinin listesi
        //     var temporaryDomains = new[] { "10minutemail.com", "tempmail.com", "guerrillamail.com", "mailinator.com" };
        //     // E-postanın bu servislerden birine ait olup olmadığını kontrol et
        //     return temporaryDomains.Any(domain => email.ToLower().EndsWith(domain));
        // }

        // POST: Kullanıcı kaydı yapan API metodu
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterApi([FromBody] RegisterDTO model)
        {
            // Model geçerli değilse (örneğin, zorunlu alanlar eksikse), hata döndür
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // // E-posta geçici bir servise aitse, hata mesajı döndür
            // if (IsTemporaryEmail(model.Email))
            // {
            //     return BadRequest(new
            //     {
            //         Message = "Geçici e-posta adresleriyle kayıt olamazsınız. Lütfen gerçek bir e-posta adresi kullanın."
            //     });
            // }

            // Yeni bir kullanıcı nesnesi oluştur
            var user = new AppUser
            {
                UserName = model.UserName, // Kullanıcı adı
                Email = model.Email, // E-posta
                FullName = model.FullName, // Tam ad
                DateAdded = DateTime.Now, // Kayıt tarihi
                Address = model.Address, // Adres
                PhoneNumber = model.PhoneNumber // Telefon numarası
            };

            // Kullanıcıyı ve şifresini veritabanına kaydet
            var result = await _userManager.CreateAsync(user, model.Password);
            // Kayıt başarılıysa
            if (result.Succeeded)
            {
                // Başarı mesajı ve kullanıcı ID'sini döndür
                return Ok(new
                {
                    Message = "Kayıt başarılı.",
                    UserId = user.Id
                });
            }

            // Kayıt başarısızsa, hataları topla ve hata mesajı döndür
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { Errors = errors });
        }

        // POST: Kullanıcı girişi yapan API metodu
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            // Kullanıcıyı hem kullanıcı adı hem de e-posta ile ara
            AppUser user = null;
            // Önce kullanıcı adına göre ara
            user = await _userManager.FindByNameAsync(model.UserName);
            // Eğer kullanıcı adı ile bulunamazsa, e-posta ile ara
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(model.UserName);
            }

            // Kullanıcı bulunamazsa, hata mesajı döndür
            if (user == null)
                return BadRequest(new { Message = "Geçersiz kullanıcı adı veya e-posta." });

            // Şifreyi kontrol et 
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            // Giriş başarılıysa
            if (result.Succeeded)
                // JWT token oluştur ve döndür
                return Ok(new { Token = GenerateJwtToken(user) });

            // Giriş başarısızsa (örneğin, yanlış şifre)
            return Unauthorized(new { Message = "Giriş başarısız, şifre yanlış." });
        }

        // JWT token üreten yardımcı metod
        private string GenerateJwtToken(AppUser user)
        {
            // JWT token işleyici oluştur
            var tokenHandler = new JwtSecurityTokenHandler();
            // Uygulama ayarlarından JWT gizli anahtarını al (appsettings.json'dan)
            var key = Encoding.UTF8.GetBytes(_configuration["AppSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret anahtarı bulunamadı."));

            // Token özelliklerini tanımla
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // Kullanıcı bilgilerini (ID ve kullanıcı adı) token'a ekle
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName ?? "")
                }),
                Expires = DateTime.UtcNow.AddDays(1), // Token 1 gün geçerli
                // Token imzalama ayarları (güvenlik için)
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            // Token'ı oluştur ve string olarak döndür
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}