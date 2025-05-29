using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StoreApp.Data.Concrete;
using StoreApp.Web.DTO;

namespace StoreApp.Web.Controllers
{
    // [Authorize] atributu: Bu controller'a sadece giriş yapmış kullanıcılar erişebilir.
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly StoreDbContext _context;
        // Kullanıcı yönetimini sağlayan Identity servisi
        private readonly UserManager<AppUser> _userManager;
        // Kullanıcı giriş/çıkış işlemlerini yöneten Identity servisi
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(StoreDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<ProfileController> logger)
        {
            _context = context; // Veritabanı bağlantısını başlatır
            _userManager = userManager; // Kullanıcı yönetimini başlatır
            _signInManager = signInManager; // Giriş/çıkış işlemlerini başlatır
            _logger = logger; // loglama
        }

        public async Task<IActionResult> Index()
        {
            // Kullanıcının adını al ve boşlukları temizle
            var userName = User.Identity.Name?.Trim();
            // Eğer kullanıcı adı boşsa veya null ise, hata logla ve giriş sayfasına yönlendir
            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning("User.Identity.Name is null or empty");
                return RedirectToAction("Login", "Users");
            }

            _logger.LogInformation("User.Identity.Name: '{UserName}' (length: {Length})", userName, userName.Length);

            // Kullanıcıyı adına göre veritabanından bul
            var user = await _userManager.FindByNameAsync(userName);

            // Eğer kullanıcı bulunamazsa, hata logla ve giriş sayfasına yönlendir
            if (user == null)
            {
                _logger.LogWarning("User not found for username: '{UserName}'", userName);
                return RedirectToAction("Login", "Users");
            }

            // Kullanıcı bilgilerini ProfileDTO nesnesine dönüştür
            var profileDTO = new ProfileDTO
            {
                UserName = user.UserName, // Kullanıcı adı
                Email = user.Email, // E-posta
                PhoneNumber = user.PhoneNumber, // Telefon numarası
                RegisterDate = user.DateAdded, // Kayıt tarihi
                FullName = user.FullName, // Tam ad
                Address = user.Address // Adres
            };

            return View(profileDTO);
        }

        public async Task<IActionResult> Edit()
        {
            // Kullanıcı adını al ve boşlukları temizle
            var userName = User.Identity.Name?.Trim();
            // Eğer kullanıcı adı boşsa veya null ise, hata logla ve giriş sayfasına yönlendir
            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning("User.Identity.Name is null or empty in Edit action");
                return RedirectToAction("Login", "Users");
            }

            // Kullanıcıyı adına göre veritabanından bul
            var user = await _userManager.FindByNameAsync(userName);
            // Eğer kullanıcı bulunamazsa, hata logla ve giriş sayfasına yönlendir
            if (user == null)
            {
                _logger.LogWarning("User not found for username: '{UserName}' in Edit action", userName);
                return RedirectToAction("Login", "Users");
            }

            // Kullanıcı bilgilerini ProfileDTO nesnesine dönüştür
            var profileDTO = new ProfileDTO
            {
                UserName = user.UserName, // Kullanıcı adı
                Email = user.Email, // E-posta
                PhoneNumber = user.PhoneNumber, // Telefon numarası
                FullName = user.FullName, // Tam ad
                Address = user.Address // Adres
            };

            return View(profileDTO);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Güvenlik için CSRF koruması
        public async Task<IActionResult> Edit(ProfileDTO model)
        {
            // Model geçerli değilse (örneğin zorunlu alanlar eksikse), düzenleme sayfasını tekrar göster
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kullanıcı adını al ve boşlukları temizle
            var userName = User.Identity.Name?.Trim();
            // Eğer kullanıcı adı boşsa veya null ise, hata logla ve giriş sayfasına yönlendir
            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning("User.Identity.Name is null or empty in Edit POST action");
                return RedirectToAction("Login", "Users");
            }

            // Kullanıcıyı adına göre veritabanından bul
            var user = await _userManager.FindByNameAsync(userName);
            // Eğer kullanıcı bulunamazsa, hata logla ve giriş sayfasına yönlendir
            if (user == null)
            {
                _logger.LogWarning("User not found for username: '{UserName}' in Edit POST action", userName);
                return RedirectToAction("Login", "Users");
            }

            // Kullanıcı bilgilerini modelden güncelle
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.FullName = model.FullName;
            user.Address = model.Address;

            // Kullanıcıyı güncelle
            var result = await _userManager.UpdateAsync(user);
            // Güncelleme başarılı ise
            if (result.Succeeded)
            {
                // Başarı mesajını logla ve profil sayfasına yönlendir
                _logger.LogInformation("User '{UserName}' updated their profile successfully", userName);
                return RedirectToAction("Index");
            }

            // Güncelleme başarısızsa, hataları ModelState'e ekle ve düzenleme sayfasını tekrar göster
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
                _logger.LogError("Error updating user '{UserName}': {Error}", userName, error.Description);
            }

            return View(model);
        }

        // GET: Şifre değiştirme sayfasını gösteren metod
        public IActionResult ChangePassword()
        {
            // Şifre değiştirme formunu görünüme (view) gönder
            return View();
        }

        // POST: Şifreyi değiştiren metod (asenkron)
        [HttpPost]
        [ValidateAntiForgeryToken] // Güvenlik için CSRF koruması
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO model)
        {
            // Model geçerli değilse (örneğin, zorunlu alanlar eksikse), şifre değiştirme sayfasını tekrar göster
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kullanıcı adını al ve boşlukları temizle
            var userName = User.Identity.Name?.Trim();
            // Eğer kullanıcı adı boşsa veya null ise, hata logla ve giriş sayfasına yönlendir
            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning("User.Identity.Name is null or empty in ChangePassword POST action");
                return RedirectToAction("Login", "Users");
            }

            // Kullanıcıyı adına göre veritabanından bul
            var user = await _userManager.FindByNameAsync(userName);
            // Eğer kullanıcı bulunamazsa, hata logla ve giriş sayfasına yönlendir
            if (user == null)
            {
                _logger.LogWarning("User not found for username: '{UserName}' in ChangePassword POST action", userName);
                return RedirectToAction("Login", "Users");
            }

            // Eski şifreyle yeni şifreyi değiştir
            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            // Şifre değişikliği başarılıysa
            if (result.Succeeded)
            {
                // Kullanıcıyı tekrar giriş yapmış gibi işaretle (oturum güncellenir)
                await _signInManager.SignInAsync(user, isPersistent: false);
                // Başarı mesajını logla ve profil sayfasına yönlendir
                _logger.LogInformation("User '{UserName}' changed their password successfully", userName);
                return RedirectToAction("Index");
            }

            // Şifre değişikliği başarısızsa, hataları ModelState'e ekle ve şifre değiştirme sayfasını tekrar göster
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
                _logger.LogError("Error changing password for user '{UserName}': {Error}", userName, error.Description);
            }

            return View(model);
        }
    }
}