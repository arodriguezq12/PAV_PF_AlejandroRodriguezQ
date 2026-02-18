using System.Collections.Generic;
using PAV_PF_AlejandroRodriguezQ.Models;

namespace PAV_PF_AlejandroRodriguezQ.Models.ViewModels
{
    public class FacturaDetailsVM
    {
        public Factura Factura { get; set; }
        public IEnumerable<FacturaDetalle> Detalles { get; set; }
        public IEnumerable<FacturaImpuesto> Impuestos { get; set; }
    }
}
