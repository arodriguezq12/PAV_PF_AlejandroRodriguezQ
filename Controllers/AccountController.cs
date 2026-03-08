using FarmaDual.Models.ViewModels;
using System;
using System.Net;
using System.Linq;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using PAV_PF_AlejandroRodriguezQ.Models;

namespace FarmaDual.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly FarmaDualEntities1 db = new FarmaDualEntities1();

        private static string NormalizeEmail(string email)
        {
            return (email ?? string.Empty).Trim().ToLowerInvariant();
        }


        private bool PasswordMatches(UsuarioAuth auth, string plainPassword)
        {
            if (auth == null || string.IsNullOrEmpty(plainPassword))
                return false;

            var stored = auth.PasswordHash ?? string.Empty;

            // Compatibilidad con cuentas legacy que pudieron guardarse en texto plano.
            if (string.Equals(stored, plainPassword, StringComparison.Ordinal))
            {
                auth.PasswordHash = Crypto.HashPassword(plainPassword);
                db.SaveChanges();
                return true;
            }

            try
            {
                return Crypto.VerifyHashedPassword(stored, plainPassword);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private bool IsDebugLoginEnabled()
        {
            var debugValue = Request != null ? Request["debug"] : null;
            return string.Equals(debugValue, "1", StringComparison.OrdinalIgnoreCase);
        }

        private void AddLoginDebugStep(System.Collections.Generic.List<string> steps, string message)
        {
            if (steps != null)
                steps.Add(string.Format("[{0:HH:mm:ss}] {1}", DateTime.Now, message));
        }


        private string SanitizeReturnUrl(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl))
                return null;

            var pathOnly = returnUrl.Split('?')[0].Trim();
            if (string.IsNullOrWhiteSpace(pathOnly))
                return null;

            if (string.Equals(pathOnly, "/", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(pathOnly, "/Account/Login", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(pathOnly, "/Account/Login/", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return returnUrl;
        }

        private void SignInUser(string correo, string role, bool rememberMe)
        {
            var ticket = new FormsAuthenticationTicket(
                1,
                correo,
                DateTime.Now,
                DateTime.Now.AddMinutes(30),
                rememberMe,
                string.IsNullOrWhiteSpace(role) ? "Cliente" : role
            );

            string encTicket = FormsAuthentication.Encrypt(ticket);

            var cookie = new System.Web.HttpCookie(FormsAuthentication.FormsCookieName, encTicket)
            {
                HttpOnly = true,
                Secure = Request.IsSecureConnection
            };

            if (rememberMe)
                cookie.Expires = ticket.Expiration;

            Response.Cookies.Add(cookie);
        }

        [HttpGet]
        public ActionResult Register(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            ViewBag.TipoTarjetaId = new SelectList(
                db.TipoTarjeta.Where(t => t.Activo),
                "TipoTarjetaId",
                "Nombre"
            );

            return View(new RegisterVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM vm, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            ViewBag.TipoTarjetaId = new SelectList(
                db.TipoTarjeta.Where(t => t.Activo),
                "TipoTarjetaId",
                "Nombre",
                vm.TipoTarjetaId
            );

            if (!ModelState.IsValid) return View(vm);

            if (db.UserProfile.Any(x => x.Correo == vm.Correo))
            {
                ModelState.AddModelError("Correo", "Ese correo ya está registrado.");
                return View(vm);
            }

            if (db.UserProfile.Any(x => x.Identificacion == vm.Identificacion))
            {
                ModelState.AddModelError("Identificacion", "Esa identificación ya existe.");
                return View(vm);
            }

            var userId = Guid.NewGuid().ToString();

            var profile = new UserProfile
            {
                UserId = userId,
                Correo = vm.Correo,
                Identificacion = vm.Identificacion,
                NombreCompleto = vm.NombreCompleto,
                Genero = vm.Genero,
                TipoTarjetaId = vm.TipoTarjetaId,
                NumeroTarjeta = vm.NumeroTarjeta,
                FechaCreacion = DateTime.Now
            };

            db.UserProfile.Add(profile);

            var auth = new UsuarioAuth
            {
                UserId = userId,
                Correo = vm.Correo,
                PasswordHash = Crypto.HashPassword(vm.Password),
                Rol = "Cliente",
                Activo = true,
                FechaCreacion = DateTime.Now
            };


            db.UsuarioAuth.Add(auth);
            db.SaveChanges();

            FormsAuthentication.SetAuthCookie(vm.Correo, false);

            // ReturnUrl si venía de una página protegida
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Medicamentos");
        }

        [HttpGet]
        public ActionResult RegisterAdmin()
        {
            // Permitir crear el primer admin si no existe ninguno, o permitir a administradores crear más.
            var hasAdmin = db.UsuarioAuth.Any(u => u.Rol == "Admin" && u.Activo);
            if (hasAdmin && !(User != null && User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Admin")))
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            ViewBag.TipoTarjetaId = new SelectList(
                db.TipoTarjeta.Where(t => t.Activo),
                "TipoTarjetaId",
                "Nombre"
            );

            return View(new RegisterVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterAdmin(RegisterVM vm)
        {
            var hasAdmin = db.UsuarioAuth.Any(u => u.Rol == "Admin" && u.Activo);
            if (hasAdmin && !(User != null && User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Admin")))
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            ViewBag.TipoTarjetaId = new SelectList(
                db.TipoTarjeta.Where(t => t.Activo),
                "TipoTarjetaId",
                "Nombre",
                vm.TipoTarjetaId
            );

            if (!ModelState.IsValid) return View(vm);

            if (db.UserProfile.Any(x => x.Correo == vm.Correo))
            {
                ModelState.AddModelError("Correo", "Ese correo ya está registrado.");
                return View(vm);
            }

            if (db.UserProfile.Any(x => x.Identificacion == vm.Identificacion))
            {
                ModelState.AddModelError("Identificacion", "Esa identificación ya existe.");
                return View(vm);
            }

            var userId = Guid.NewGuid().ToString();

            var profile = new UserProfile
            {
                UserId = userId,
                Correo = vm.Correo,
                Identificacion = vm.Identificacion,
                NombreCompleto = vm.NombreCompleto,
                Genero = vm.Genero,
                TipoTarjetaId = vm.TipoTarjetaId,
                NumeroTarjeta = vm.NumeroTarjeta,
                FechaCreacion = DateTime.Now
            };

            db.UserProfile.Add(profile);

            var auth = new UsuarioAuth
            {
                UserId = userId,
                Correo = vm.Correo,
                PasswordHash = Crypto.HashPassword(vm.Password),
                Rol = "Admin",
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            db.UsuarioAuth.Add(auth);
            db.SaveChanges();

            // Si quien creó no está autenticado (creación del primer admin), autenticar al nuevo admin
            if (!(User != null && User.Identity != null && User.Identity.IsAuthenticated))
                FormsAuthentication.SetAuthCookie(vm.Correo, false);

            TempData["Success"] = "Administrador creado correctamente.";
            return RedirectToAction("Index", "Admin");
        }

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            returnUrl = SanitizeReturnUrl(returnUrl);
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.DebugMode = IsDebugLoginEnabled();
            return View(new LoginVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM vm, string returnUrl)
        {
            returnUrl = SanitizeReturnUrl(returnUrl);
            ViewBag.ReturnUrl = returnUrl;

            if (vm == null)
                vm = new LoginVM();

            var debugMode = IsDebugLoginEnabled();
            ViewBag.DebugMode = debugMode;
            var debugSteps = debugMode ? new System.Collections.Generic.List<string>() : null;
            AddLoginDebugStep(debugSteps, "Inicio de POST /Account/Login.");

            vm.Correo = NormalizeEmail(vm.Correo);
            ModelState.Remove("Correo");
            TryValidateModel(vm);
            AddLoginDebugStep(debugSteps, "Correo normalizado: " + vm.Correo + ".");

            if (!ModelState.IsValid)
            {
                AddLoginDebugStep(debugSteps, "ModelState invlido. Se devuelve la vista con errores.");
                ViewBag.DebugLoginTrace = debugSteps;
                return View(vm);
            }

            var auth = db.UsuarioAuth.FirstOrDefault(x =>
                x.Activo &&
                x.Correo != null &&
                x.Correo.Trim().ToLower() == vm.Correo);
            AddLoginDebugStep(debugSteps, auth == null
                ? "No se encontró usuario activo con ese correo."
                : string.Format("Usuario encontrado. Rol={0}, Activo={1}.", auth.Rol, auth.Activo));

            if (!PasswordMatches(auth, vm.Password))
            {
                AddLoginDebugStep(debugSteps, "PasswordMatches devolvió false.");
                ModelState.AddModelError("", "Credenciales inválidas.");
                ViewBag.DebugLoginTrace = debugSteps;
                return View(vm);
            }

            AddLoginDebugStep(debugSteps, "PasswordMatches devolvió true. Firmando usuario.");
            SignInUser(auth.Correo, auth.Rol, vm.RememberMe);


            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                AddLoginDebugStep(debugSteps, "Redirección a returnUrl local: " + returnUrl + ".");
                return Redirect(returnUrl);
            }

            AddLoginDebugStep(debugSteps, "Redirección por defecto a Medicamentos/Index.");
            return RedirectToAction("Index", "Medicamentos");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
