using catalogo_web_mvc.Interfaces.Audit;
using catalogo_web_mvc.Interfaces.Email;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Models.ViewModels;
using catalogo_web_mvc.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace catalogo_web_mvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAuditService _audit;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IAuditService audit, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _audit = audit;
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("auth")]
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
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                await _audit.RegistrarAsync("REGISTER", model.Email, user.Id);

                try
                {
                    var (subject, body) = EmailTemplates.BuildWelcomeEmail(model.Email);
                    await _emailSender.SendEmailAsync(model.Email, subject, body);
                }
                catch (Exception ex)
                {
                    // El envío del mail no debe bloquear el registro del usuario.
                    await _audit.RegistrarAsync("WELCOME_EMAIL_FAIL", model.Email, user.Id, ex.Message);
                }

                return RedirectToAction("Index", "Home");
            }

            // No revelamos si el motivo fue un email/usuario duplicado (evita enumeración de cuentas).
            bool emailEnUso = result.Errors.Any(e => e.Code is "DuplicateUserName" or "DuplicateEmail");
            foreach (var error in result.Errors)
            {
                if (error.Code is "DuplicateUserName" or "DuplicateEmail") continue;
                ModelState.AddModelError(string.Empty, error.Description);
            }
            if (emailEnUso)
                ModelState.AddModelError(string.Empty, "No fue posible completar el registro. Verificá los datos ingresados.");

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = Url.Action("ResetPassword", "Account",
                    new { email = model.Email, token }, protocol: Request.Scheme)!;

                try
                {
                    var (subject, body) = EmailTemplates.BuildPasswordResetEmail(resetLink);
                    await _emailSender.SendEmailAsync(model.Email, subject, body);
                    await _audit.RegistrarAsync("PASSWORD_RESET_REQUESTED", model.Email, user.Id);
                }
                catch (Exception ex)
                {
                    await _audit.RegistrarAsync("PASSWORD_RESET_EMAIL_FAIL", model.Email, user.Id, ex.Message);
                }
            }

            // Mostramos siempre la misma confirmación, exista o no el usuario
            // (evita enumeración de cuentas registradas).
            return View("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ResetPassword(string? email = null, string? token = null)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return BadRequest("Enlace de restablecimiento inválido.");

            var model = new ResetPasswordViewModel { Email = email, Token = token };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Anti-enumeración: mostramos éxito igual aunque el usuario no exista.
                return View("ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                await _audit.RegistrarAsync("PASSWORD_RESET_OK", model.Email, user.Id);
                return View("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            await _audit.RegistrarAsync("PASSWORD_RESET_FAIL", model.Email, user.Id, "Token inválido o expirado");
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
