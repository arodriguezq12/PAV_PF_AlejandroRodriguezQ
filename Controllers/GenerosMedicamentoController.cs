using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PAV_PF_AlejandroRodriguezQ.Models;

namespace PAV_PF_AlejandroRodriguezQ.Controllers
{
    [Authorize]
    public class GenerosMedicamentoController : Controller
    {
        private readonly FarmaDualEntities1 db = new FarmaDualEntities1();

        public ActionResult Index() => View(db.GeneroMedicamento.OrderBy(x => x.Nombre).ToList());

        public ActionResult Create() => View(new GeneroMedicamento { Activo = true });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Nombre,Descripcion,Activo")] GeneroMedicamento model)
        {
            if (!ModelState.IsValid) return View(model);
            db.GeneroMedicamento.Add(model);
            db.SaveChanges();
            TempData["Success"] = "Tipo de medicamento creado.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int? id)
        {
            if (!id.HasValue) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var item = db.GeneroMedicamento.Find(id.Value);
            if (item == null) return HttpNotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "GeneroId,Nombre,Descripcion,Activo")] GeneroMedicamento model)
        {
            if (!ModelState.IsValid) return View(model);
            var entity = db.GeneroMedicamento.Find(model.GeneroId);
            if (entity == null) return HttpNotFound();
            entity.Nombre = model.Nombre;
            entity.Descripcion = model.Descripcion;
            entity.Activo = model.Activo;
            db.SaveChanges();
            TempData["Success"] = "Tipo de medicamento actualizado.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deactivate(int id)
        {
            var item = db.GeneroMedicamento.Find(id);
            if (item == null) return HttpNotFound();
            item.Activo = false;
            db.SaveChanges();
            TempData["Success"] = "Tipo de medicamento desactivado.";
            return RedirectToAction("Index");
        }
    }
}
