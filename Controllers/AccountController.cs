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
            if (hasAdmin && !(User?.Identity?.IsAuthenticated == true && User.IsInRole("Admin")))
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
            if (hasAdmin && !(User?.Identity?.IsAuthenticated == true && User.IsInRole("Admin")))
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
            if (!(User?.Identity?.IsAuthenticated == true))
                FormsAuthentication.SetAuthCookie(vm.Correo, false);

            TempData["Success"] = "Administrador creado correctamente.";
            return RedirectToAction("Index", "Admin");
        }

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM vm, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            vm.Correo = NormalizeEmail(vm.Correo);

            // El binder valida antes de entrar al action. Si el correo trae espacios,
            // puede quedar inválido aunque al normalizar sí sea correcto.
            ModelState.Remove(nameof(vm.Correo));
            TryValidateModel(vm);

            if (!ModelState.IsValid) return View(vm);

            var auth = db.UsuarioAuth.FirstOrDefault(x => x.Correo.ToLower() == vm.Correo && x.Activo);

            if (auth == null || !Crypto.VerifyHashedPassword(auth.PasswordHash, vm.Password))
            {
                ModelState.AddModelError("", "Credenciales inválidas.");
                return View(vm);
            }

            SignInUser(auth.Correo, auth.Rol, vm.RememberMe);


            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

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
