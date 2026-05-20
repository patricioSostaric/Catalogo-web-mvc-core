using catalogo_web_mvc.Interfaces.Audit;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace catalogo_web_mvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAuditService _audit;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IAuditService audit)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _audit = audit;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var userId = (await _userManager.FindByEmailAsync(model.Email))?.Id;
                await _audit.RegistrarAsync("LOGIN_OK", model.Email, userId);
                return LocalRedirect(returnUrl ?? "/");
            }

            if (result.IsLockedOut)
            {
                await _audit.RegistrarAsync("LOCKOUT", model.Email, detalle: "Cuenta bloqueada por intentos fallidos");
                ModelState.AddModelError(string.Empty, "Cuenta bloqueada temporalmente por múltiples intentos fallidos. Intentá de nuevo en 5 minutos.");
                return View(model);
            }

            await _audit.RegistrarAsync("LOGIN_FAIL", model.Email, detalle: "Credenciales incorrectas");
            ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                await _audit.RegistrarAsync("REGISTER", model.Email, user.Id);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _signInManager.SignOutAsync();
            await _audit.RegistrarAsync("LOGOUT", email, userId);
            return RedirectToAction("Index", "Home");
        }
    }
}
