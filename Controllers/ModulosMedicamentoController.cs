using System.Linq;
using System.Net;
using System.Web.Mvc;
using PAV_PF_AlejandroRodriguezQ.Models;

namespace PAV_PF_AlejandroRodriguezQ.Controllers
{
    [Authorize]
    public class ModulosMedicamentoController : Controller
    {
        private readonly FarmaDualEntities1 db = new FarmaDualEntities1();

        public ActionResult Index() => View(db.ModuloMedicamento.OrderBy(x => x.Nombre).ToList());

        public ActionResult Create() => View(new ModuloMedicamento { Activo = true });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ModuloCodigo,Nombre,Descripcion,Activo")] ModuloMedicamento model)
        {
            if (db.ModuloMedicamento.Any(x => x.ModuloCodigo == model.ModuloCodigo))
                ModelState.AddModelError("ModuloCodigo", "Código ya existe.");

            if (!ModelState.IsValid) return View(model);
            db.ModuloMedicamento.Add(model);
            db.SaveChanges();
            TempData["Success"] = "Módulo creado.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var item = db.ModuloMedicamento.Find(id);
            if (item == null) return HttpNotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ModuloCodigo,Nombre,Descripcion,Activo")] ModuloMedicamento model)
        {
            if (!ModelState.IsValid) return View(model);
            var entity = db.ModuloMedicamento.Find(model.ModuloCodigo);
            if (entity == null) return HttpNotFound();
            entity.Nombre = model.Nombre;
            entity.Descripcion = model.Descripcion;
            entity.Activo = model.Activo;
            db.SaveChanges();
            TempData["Success"] = "Módulo actualizado.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deactivate(string id)
        {
            var item = db.ModuloMedicamento.Find(id);
            if (item == null) return HttpNotFound();
            item.Activo = false;
            db.SaveChanges();
            TempData["Success"] = "Módulo desactivado.";
            return RedirectToAction("Index");
        }
    }
}
