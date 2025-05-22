using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StoreApp.Data.Concrete;
using StoreApp.Web.DTO;

namespace StoreApp.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly StoreDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(StoreDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<ProfileController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userName = User.Identity.Name?.Trim();
            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning("User.Identity.Name is null or empty");
                return RedirectToAction("Login", "Users");
            }

            _logger.LogInformation("User.Identity.Name: '{UserName}' (length: {Length})", userName, userName.Length);
            
            var user = await _userManager.FindByNameAsync(userName);
            
            if (user == null)
            {
                _logger.LogWarning("User not found for username: '{UserName}'", userName);
                return RedirectToAction("Login", "Users");
            }

            var profileDTO = new ProfileDTO
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RegisterDate = user.DateAdded,
                FullName = user.FullName,
                Address = user.Address
            };

            return View(profileDTO);
        }

        public async Task<IActionResult> Edit()
        {
            var userName = User.Identity.Name?.Trim();
            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning("User.Identity.Name is null or empty in Edit action");
                return RedirectToAction("Login", "Users");
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                _logger.LogWarning("User not found for username: '{UserName}' in Edit action", userName);
                return RedirectToAction("Login", "Users");
            }

            var profileDTO = new ProfileDTO
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,
                Address = user.Address
            };

            return View(profileDTO);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userName = User.Identity.Name?.Trim();
            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning("User.Identity.Name is null or empty in Edit POST action");
                return RedirectToAction("Login", "Users");
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                _logger.LogWarning("User not found for username: '{UserName}' in Edit POST action", userName);
                return RedirectToAction("Login", "Users");
            }

            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.FullName = model.FullName;
            user.Address = model.Address;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("User '{UserName}' updated their profile successfully", userName);
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
                _logger.LogError("Error updating user '{UserName}': {Error}", userName, error.Description);
            }

            return View(model);
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userName = User.Identity.Name?.Trim();
            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning("User.Identity.Name is null or empty in ChangePassword POST action");
                return RedirectToAction("Login", "Users");
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                _logger.LogWarning("User not found for username: '{UserName}' in ChangePassword POST action", userName);
                return RedirectToAction("Login", "Users");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("User '{UserName}' changed their password successfully", userName);
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
                _logger.LogError("Error changing password for user '{UserName}': {Error}", userName, error.Description);
            }

            return View(model);
        }
    }
}