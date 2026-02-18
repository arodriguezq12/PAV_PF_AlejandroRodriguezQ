using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PAV_PF_AlejandroRodriguezQ.Models;
using PAV_PF_AlejandroRodriguezQ.Models.ViewModels;

namespace PAV_PF_AlejandroRodriguezQ.Controllers
{
    [Authorize]
    public class FacturasController : Controller
    {
        private readonly FarmaDualEntities1 db = new FarmaDualEntities1();

        public ActionResult Index(string numeroFactura, string identificacionUsuario, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var query = db.Factura.AsQueryable();

            if (!string.IsNullOrWhiteSpace(numeroFactura))
                query = query.Where(x => x.NumeroFactura.Contains(numeroFactura));

            if (!string.IsNullOrWhiteSpace(identificacionUsuario))
                query = query.Where(x => x.IdentificacionUsuario.Contains(identificacionUsuario));

            if (fechaDesde.HasValue)
                query = query.Where(x => x.Fecha >= fechaDesde.Value);

            if (fechaHasta.HasValue)
            {
                var end = fechaHasta.Value.Date.AddDays(1);
                query = query.Where(x => x.Fecha < end);
            }

            ViewBag.NumeroFactura = numeroFactura;
            ViewBag.IdentificacionUsuario = identificacionUsuario;
            ViewBag.FechaDesde = fechaDesde?.ToString("yyyy-MM-dd");
            ViewBag.FechaHasta = fechaHasta?.ToString("yyyy-MM-dd");

            return View(query.OrderByDescending(x => x.Fecha).ToList());
        }

        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var factura = db.Factura.FirstOrDefault(x => x.FacturaId == id.Value);
            if (factura == null)
                return HttpNotFound();

            var vm = new FacturaDetailsVM
            {
                Factura = factura,
                Detalles = db.FacturaDetalle
                    .Include(d => d.Medicamento)
                    .Where(d => d.FacturaId == factura.FacturaId)
                    .ToList(),
                Impuestos = db.FacturaImpuesto
                    .Include(i => i.Impuesto)
                    .Where(i => i.FacturaId == factura.FacturaId)
                    .ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var form = new FacturaCreateVM();
            for (var i = 0; i < 3; i++) form.Lineas.Add(new FacturaLineaVM { Cantidad = 1 });

            var vm = BuildCreatePageVm(form, null, 0m, 0m, 0m);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FacturaCreatePageVM page)
        {
            var form = page?.Form ?? new FacturaCreateVM();
            var erroresPreview = new List<string>();
            var preview = CalcularResumen(form, erroresPreview, out decimal subtotal, out decimal impuestos, out decimal total);

            foreach (var err in erroresPreview) ModelState.AddModelError("", err);

            if (!ModelState.IsValid)
            {
                if (form.Lineas == null || form.Lineas.Count == 0)
                    form.Lineas = new List<FacturaLineaVM> { new FacturaLineaVM { Cantidad = 1 } };

                return View(BuildCreatePageVm(form, preview, subtotal, impuestos, total));
            }

            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    var userId = db.UserProfile
                        .Where(u => u.Correo == User.Identity.Name)
                        .Select(u => u.UserId)
                        .FirstOrDefault();

                    var factura = new Factura
                    {
                        NumeroFactura = $"FAC-{DateTime.Now:yyyyMMddHHmmssfff}",
                        Fecha = DateTime.Now,
                        UserId = userId,
                        IdentificacionUsuario = form.IdentificacionUsuario,
                        EsExento = form.EsExento,
                        Subtotal = subtotal,
                        TotalImpuestos = impuestos,
                        Total = total
                    };

                    db.Factura.Add(factura);
                    db.SaveChanges();

                    foreach (var linea in preview)
                    {
                        db.FacturaDetalle.Add(new FacturaDetalle
                        {
                            FacturaId = factura.FacturaId,
                            MedicamentoCodigo = linea.MedicamentoCodigo,
                            Cantidad = linea.Cantidad,
                            PrecioUnitario = linea.PrecioUnitario,
                            SubtotalLinea = linea.SubtotalLinea
                        });
                    }

                    if (!form.EsExento && impuestos > 0)
                    {
                        var iva = db.Impuesto
                            .Where(i => i.Activo)
                            .OrderByDescending(i => i.VigenteDesde)
                            .FirstOrDefault(i => i.Codigo.Contains("IVA") || i.Nombre.Contains("IVA"))
                                  ?? db.Impuesto.Where(i => i.Activo).OrderByDescending(i => i.VigenteDesde).FirstOrDefault();

                        if (iva != null)
                        {
                            db.FacturaImpuesto.Add(new FacturaImpuesto
                            {
                                FacturaId = factura.FacturaId,
                                ImpuestoId = iva.ImpuestoId,
                                PorcentajeAplicado = iva.Porcentaje,
                                MontoImpuesto = impuestos
                            });
                        }
                    }

                    db.SaveChanges();
                    tx.Commit();

                    TempData["Success"] = "Factura creada correctamente.";
                    return RedirectToAction("Details", new { id = factura.FacturaId });
                }
                catch (Exception)
                {
                    tx.Rollback();
                    TempData["Error"] = "Ocurrió un error al guardar la factura.";
                    return View(BuildCreatePageVm(form, preview, subtotal, impuestos, total));
                }
            }
        }

        private List<FacturaLineaResumenVM> CalcularResumen(FacturaCreateVM form, List<string> errores, out decimal subtotal, out decimal totalImpuestos, out decimal total)
        {
            subtotal = 0m;
            totalImpuestos = 0m;
            total = 0m;

            var resumen = new List<FacturaLineaResumenVM>();
            var lineas = form?.Lineas ?? new List<FacturaLineaVM>();

            var candidatas = lineas
                .Where(l => !string.IsNullOrWhiteSpace(l.MedicamentoCodigo))
                .ToList();

            if (!candidatas.Any())
            {
                errores.Add("Debe ingresar al menos una línea con medicamento.");
                return resumen;
            }

            foreach (var linea in candidatas)
            {
                if (linea.Cantidad <= 0)
                {
                    errores.Add("Todas las cantidades deben ser mayores a cero.");
                    continue;
                }

                var medicamento = db.Medicamento.FirstOrDefault(m => m.MedicamentoCodigo == linea.MedicamentoCodigo && m.Activo);
                if (medicamento == null)
                {
                    errores.Add($"Medicamento inválido o inactivo: {linea.MedicamentoCodigo}");
                    continue;
                }

                if (medicamento.Precio < 0)
                {
                    errores.Add($"El medicamento {medicamento.Nombre} tiene precio inválido.");
                    continue;
                }

                var subtotalLinea = medicamento.Precio * linea.Cantidad;
                subtotal += subtotalLinea;

                resumen.Add(new FacturaLineaResumenVM
                {
                    MedicamentoCodigo = medicamento.MedicamentoCodigo,
                    Nombre = medicamento.Nombre,
                    Cantidad = linea.Cantidad,
                    PrecioUnitario = medicamento.Precio,
                    SubtotalLinea = subtotalLinea
                });
            }

            if (errores.Any())
                return resumen;

            decimal tasa = 0m;
            if (!form.EsExento)
            {
                var iva = db.Impuesto
                    .Where(i => i.Activo)
                    .OrderByDescending(i => i.VigenteDesde)
                    .FirstOrDefault(i => i.Codigo.Contains("IVA") || i.Nombre.Contains("IVA"))
                          ?? db.Impuesto.Where(i => i.Activo).OrderByDescending(i => i.VigenteDesde).FirstOrDefault();

                if (iva != null)
                {
                    tasa = iva.Porcentaje > 1m ? iva.Porcentaje / 100m : iva.Porcentaje;
                }
            }

            totalImpuestos = Math.Round(subtotal * tasa, 2);
            total = Math.Round(subtotal + totalImpuestos, 2);
            subtotal = Math.Round(subtotal, 2);

            return resumen;
        }

        private FacturaCreatePageVM BuildCreatePageVm(FacturaCreateVM form, IEnumerable<FacturaLineaResumenVM> preview, decimal subtotal, decimal impuestos, decimal total)
        {
            var medicamentosActivos = db.Medicamento
                .Where(m => m.Activo)
                .OrderBy(m => m.Nombre)
                .Select(m => new
                {
                    m.MedicamentoCodigo,
                    Texto = m.MedicamentoCodigo + " - " + m.Nombre + " (₡" + m.Precio + ")"
                })
                .ToList();

            return new FacturaCreatePageVM
            {
                Form = form,
                Medicamentos = new SelectList(medicamentosActivos, "MedicamentoCodigo", "Texto"),
                PreviewLineas = preview ?? new List<FacturaLineaResumenVM>(),
                PreviewSubtotal = subtotal,
                PreviewTotalImpuestos = impuestos,
                PreviewTotal = total
            };
        }
    }
}
