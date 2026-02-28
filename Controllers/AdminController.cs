using System.Linq;
using System.Net;
using System.Web.Mvc;
using PAV_PF_AlejandroRodriguezQ.Models;

namespace PAV_PF_AlejandroRodriguezQ.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly FarmaDualEntities1 db = new FarmaDualEntities1();

        public ActionResult Index()
        {
            var users = db.UsuarioAuth
                .OrderBy(u => u.Correo)
                .ToList();

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Promote(int id)
        {
            var user = db.UsuarioAuth.FirstOrDefault(u => u.UsuarioAuthId == id);
            if (user == null) return HttpNotFound();

            user.Rol = "Admin";
            db.SaveChanges();

            TempData["Success"] = "Usuario promovido a administrador.";
            return RedirectToAction("Index");
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
