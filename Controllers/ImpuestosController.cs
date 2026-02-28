using System.Linq;
using System.Web.Mvc;
using PAV_PF_AlejandroRodriguezQ.Models;

namespace PAV_PF_AlejandroRodriguezQ.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ImpuestosController : Controller
    {
        private readonly FarmaDualEntities1 db = new FarmaDualEntities1();

        public ActionResult Index()
        {
            var impuestos = db.Impuesto.OrderByDescending(i => i.VigenteDesde).ToList();
            return View(impuestos);
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
