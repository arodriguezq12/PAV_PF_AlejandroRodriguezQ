using FarmaDual.Models.ViewModels;
using System;
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

            return RedirectToAction("Index", "Home");
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

            if (!ModelState.IsValid) return View(vm);

            var auth = db.UsuarioAuth.FirstOrDefault(x => x.Correo == vm.Correo && x.Activo);

            if (auth == null || !Crypto.VerifyHashedPassword(auth.PasswordHash, vm.Password))
            {
                ModelState.AddModelError("", "Credenciales inválidas.");
                return View(vm);
            }

            var role = auth.Rol ?? "Cliente";

            var ticket = new FormsAuthenticationTicket(
                1,
                vm.Correo,                      // Name => User.Identity.Name
                DateTime.Now,
                DateTime.Now.AddMinutes(30),
                vm.RememberMe,
                role                            // UserData => rol
            );

            string encTicket = FormsAuthentication.Encrypt(ticket);

            var cookie = new System.Web.HttpCookie(FormsAuthentication.FormsCookieName, encTicket)
            {
                HttpOnly = true,
                Secure = Request.IsSecureConnection
            };

            if (vm.RememberMe)
                cookie.Expires = ticket.Expiration;

            Response.Cookies.Add(cookie);


            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}
