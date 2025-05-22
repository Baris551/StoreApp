using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StoreApp.Data.Concrete;
using StoreApp.Web.DTO;
using StoreApp.Web.Models;
using System.Threading.Tasks;

namespace StoreApp.Web.Controllers
{
    public class UsersController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public UsersController(
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                TempData["Message"] = "Zaten giriş yapmışsınız.";
                return RedirectToAction("Index", "Home");
            }

            var model = new RegisterDTO { ReturnUrl = returnUrl };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO model, string? returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                TempData["Message"] = "Zaten giriş yapmışsınız.";
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                DateAdded = DateTime.Now,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                TempData["Success"] = "Kayıt başarılı. Giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                TempData["Message"] = "Zaten giriş yapmışsınız.";
                return RedirectToAction("Index", "Home");
            }

            var model = new LoginDTO { ReturnUrl = returnUrl };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO model, string? returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                TempData["Message"] = "Zaten giriş yapmışsınız.";
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı");
                return View(model);
            }

            // E-posta onayı kontrolü kaldırıldı

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: true, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı");
            return View(model);
        }
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult Profile()
        {
            return View();
        }
    }
}
