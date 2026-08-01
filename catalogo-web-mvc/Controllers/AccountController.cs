using catalogo_web_mvc.Interfaces.Audit;
using catalogo_web_mvc.Interfaces.Avatar;
using catalogo_web_mvc.Interfaces.Email;
using catalogo_web_mvc.Models;
using catalogo_web_mvc.Models.ViewModels;
using catalogo_web_mvc.Services.Avatar;
using catalogo_web_mvc.Services.Email;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IAvatarService _avatarService;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IAuditService audit, IEmailSender emailSender, IAvatarService avatarService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _audit = audit;
            _emailSender = emailSender;
            _avatarService = avatarService;
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

            // La imagen se valida y guarda antes de crear el usuario: si el archivo es
            // inválido no tiene sentido dejar la cuenta creada a medias.
            string? avatarUrl = null;
            if (model.Datos.Avatar is not null)
            {
                var guardado = await _avatarService.GuardarAsync(model.Datos.Avatar);
                if (!guardado.EsValido)
                {
                    ModelState.AddModelError("Datos.Avatar", guardado.Error!);
                    return View(model);
                }
                avatarUrl = guardado.RutaRelativa;
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Nombre = model.Datos.Nombre,
                Apellido = model.Datos.Apellido,
                FechaNacimiento = model.Datos.FechaNacimiento,
                Localidad = model.Datos.Localidad,
                CodigoPostal = model.Datos.CodigoPostal,
                AvatarUrl = avatarUrl
            };
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

            // El usuario no se creó: se borra la imagen ya guardada para no dejar
            // archivos huérfanos en disco por cada intento fallido de registro.
            _avatarService.Eliminar(avatarUrl);

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

        // ── Perfil ─────────────────────────────────────────────────────────────

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Perfil()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Challenge();

            return View(MapearAPerfil(user));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(AvatarValidator.MaxBytes)]
        public async Task<IActionResult> Perfil(PerfilViewModel model)
        {
            // Siempre se trabaja sobre el usuario de la sesión. El id nunca se toma del
            // formulario: si viniera del cliente, cualquiera podría editar otro perfil
            // cambiando el campo (IDOR, OWASP A01).
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Challenge();

            if (!ModelState.IsValid)
            {
                model.Email = user.Email ?? string.Empty;
                model.Edad = user.Edad;
                model.Datos.AvatarActual = user.AvatarUrl;
                return View(model);
            }

            var avatarAnterior = user.AvatarUrl;
            string? avatarNuevo = null;

            if (model.Datos.Avatar is not null)
            {
                var guardado = await _avatarService.GuardarAsync(model.Datos.Avatar);
                if (!guardado.EsValido)
                {
                    ModelState.AddModelError("Datos.Avatar", guardado.Error!);
                    model.Email = user.Email ?? string.Empty;
                    model.Edad = user.Edad;
                    model.Datos.AvatarActual = avatarAnterior;
                    return View(model);
                }
                avatarNuevo = guardado.RutaRelativa;
                user.AvatarUrl = avatarNuevo;
            }
            else if (model.Datos.QuitarAvatar)
            {
                user.AvatarUrl = null;
            }

            user.Nombre = model.Datos.Nombre;
            user.Apellido = model.Datos.Apellido;
            user.FechaNacimiento = model.Datos.FechaNacimiento;
            user.Localidad = model.Datos.Localidad;
            user.CodigoPostal = model.Datos.CodigoPostal;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                // Si no se pudo persistir, se descarta la imagen nueva y se conserva la anterior.
                _avatarService.Eliminar(avatarNuevo);

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                model.Email = user.Email ?? string.Empty;
                model.Edad = user.Edad;
                model.Datos.AvatarActual = avatarAnterior;
                return View(model);
            }

            // Recién con el update confirmado se borra el archivo viejo.
            if (user.AvatarUrl != avatarAnterior)
                _avatarService.Eliminar(avatarAnterior);

            // Los claims (avatar y nombre para mostrar) viven en la cookie, así que hay
            // que regenerarla para que el navbar refleje el cambio sin re-loguearse.
            await _signInManager.RefreshSignInAsync(user);

            await _audit.RegistrarAsync("PERFIL_ACTUALIZADO", user.Email, user.Id);

            TempData["PerfilActualizado"] = "Tus datos se guardaron correctamente.";
            return RedirectToAction(nameof(Perfil));
        }

        private static PerfilViewModel MapearAPerfil(ApplicationUser user) => new()
        {
            Email = user.Email ?? string.Empty,
            Edad = user.Edad,
            Datos = new PerfilCamposViewModel
            {
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                FechaNacimiento = user.FechaNacimiento,
                Localidad = user.Localidad,
                CodigoPostal = user.CodigoPostal,
                AvatarActual = user.AvatarUrl
            }
        };

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

        /// <summary>
        /// Destino al que Identity redirige cuando la sesión es válida pero el rol no
        /// alcanza. Sin esta acción el usuario recibía un 404, porque la ruta por defecto
        /// no existía en este controlador.
        /// </summary>
        [Authorize]
        public async Task<IActionResult> AccessDenied(string? returnUrl = null)
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Un intento de entrar a algo fuera del alcance del rol es justamente lo que
            // la auditoría tiene que registrar.
            await _audit.RegistrarAsync("ACCESO_DENEGADO", email, userId, returnUrl);

            return View();
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
