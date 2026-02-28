using System.Linq;
using System.Web.Mvc;
using PAV_PF_AlejandroRodriguezQ.Models;

namespace PAV_PF_AlejandroRodriguezQ.Controllers
{
    public class ProfileController : Controller
    {
        private readonly FarmaDualEntities1 db = new FarmaDualEntities1();

        [ChildActionOnly]
        public ActionResult UserNav()
        {
            var displayName = User?.Identity?.Name;

            if (User?.Identity?.IsAuthenticated == true)
            {
                try
                {
                    var profile = db.UserProfile.FirstOrDefault(u => u.Correo == User.Identity.Name);
                    if (profile != null && !string.IsNullOrWhiteSpace(profile.NombreCompleto))
                        displayName = profile.NombreCompleto;
                }
                catch
                {
                    // ignore
                }
            }

            return PartialView("_UserDisplayNav", displayName);
        }

        // Removed UserHeader usage; keep only UserNav for navbar rendering.

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
