using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PAV_PF_AlejandroRodriguezQ.Models;
using PAV_PF_AlejandroRodriguezQ.Models.ViewModels;

namespace PAV_PF_AlejandroRodriguezQ.Controllers
{
    [Authorize]
    public class MedicamentosController : Controller
    {
        private readonly FarmaDualEntities1 db = new FarmaDualEntities1();

        public ActionResult Index()
        {
            var medicamentos = db.Medicamento
                .Include(m => m.ModuloMedicamento)
                .Include(m => m.GeneroMedicamento)
                .OrderBy(m => m.Nombre)
                .ToList();

            return View(medicamentos);
        }

        public ActionResult Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var medicamento = db.Medicamento
                .Include(m => m.ModuloMedicamento)
                .Include(m => m.GeneroMedicamento)
                .FirstOrDefault(m => m.MedicamentoCodigo == id);

            if (medicamento == null)
            {
                return HttpNotFound();
            }

            return View(medicamento);
        }

        public ActionResult Create()
        {
            CargarCombos();
            return View(new MedicamentoFormVM { Activo = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MedicamentoFormVM vm)
        {
            if (db.Medicamento.Any(m => m.MedicamentoCodigo == vm.MedicamentoCodigo))
            {
                ModelState.AddModelError(nameof(vm.MedicamentoCodigo), "Ese código de medicamento ya existe.");
            }

            ValidarPrecio(vm);

            if (!ModelState.IsValid)
            {
                CargarCombos(vm.ModuloCodigo, vm.GeneroId);
                return View(vm);
            }

            var medicamento = new Medicamento
            {
                MedicamentoCodigo = vm.MedicamentoCodigo,
                Nombre = vm.Nombre,
                Precio = vm.Precio,
                ModuloCodigo = vm.ModuloCodigo,
                GeneroId = vm.GeneroId,
                Activo = vm.Activo,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = null
            };

            db.Medicamento.Add(medicamento);
            db.SaveChanges();

            TempData["Success"] = "Medicamento creado correctamente.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var medicamento = db.Medicamento.Find(id);
            if (medicamento == null)
            {
                return HttpNotFound();
            }

            var vm = new MedicamentoFormVM
            {
                MedicamentoCodigo = medicamento.MedicamentoCodigo,
                Nombre = medicamento.Nombre,
                Precio = medicamento.Precio,
                ModuloCodigo = medicamento.ModuloCodigo,
                GeneroId = medicamento.GeneroId,
                Activo = medicamento.Activo
            };

            CargarCombos(vm.ModuloCodigo, vm.GeneroId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MedicamentoFormVM vm)
        {
            ValidarPrecio(vm);

            if (!ModelState.IsValid)
            {
                CargarCombos(vm.ModuloCodigo, vm.GeneroId);
                return View(vm);
            }

            var medicamento = db.Medicamento.Find(vm.MedicamentoCodigo);
            if (medicamento == null)
            {
                return HttpNotFound();
            }

            medicamento.Nombre = vm.Nombre;
            medicamento.Precio = vm.Precio;
            medicamento.ModuloCodigo = vm.ModuloCodigo;
            medicamento.GeneroId = vm.GeneroId;
            medicamento.Activo = vm.Activo;
            medicamento.FechaActualizacion = DateTime.Now;

            db.SaveChanges();

            TempData["Success"] = "Medicamento actualizado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deactivate(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var medicamento = db.Medicamento.Find(id);
            if (medicamento == null)
            {
                return HttpNotFound();
            }

            medicamento.Activo = false;
            medicamento.FechaActualizacion = DateTime.Now;
            db.SaveChanges();

            TempData["Success"] = "Medicamento desactivado correctamente.";
            return RedirectToAction("Index");
        }

        private void CargarCombos(string moduloSeleccionado = null, int? generoSeleccionado = null)
        {
            ViewBag.ModuloCodigo = new SelectList(
                db.ModuloMedicamento.Where(x => x.Activo).OrderBy(x => x.Nombre).ToList(),
                "ModuloCodigo",
                "Nombre",
                moduloSeleccionado
            );

            ViewBag.GeneroId = new SelectList(
                db.GeneroMedicamento.Where(x => x.Activo).OrderBy(x => x.Nombre).ToList(),
                "GeneroId",
                "Nombre",
                generoSeleccionado
            );
        }

        private void ValidarPrecio(MedicamentoFormVM vm)
        {
            if (vm.Precio <= 0)
            {
                ModelState.AddModelError(nameof(vm.Precio), "El precio debe ser mayor a 0.");
            }
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
