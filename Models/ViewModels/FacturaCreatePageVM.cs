using System.Collections.Generic;
using System.Web.Mvc;

namespace PAV_PF_AlejandroRodriguezQ.Models.ViewModels
{
    public class FacturaCreatePageVM
    {
        public FacturaCreateVM Form { get; set; }
        public SelectList Medicamentos { get; set; }
        public IEnumerable<FacturaLineaResumenVM> PreviewLineas { get; set; }
        public decimal PreviewSubtotal { get; set; }
        public decimal PreviewTotalImpuestos { get; set; }
        public decimal PreviewTotal { get; set; }
    }

    public class FacturaLineaResumenVM
    {
        public string MedicamentoCodigo { get; set; }
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubtotalLinea { get; set; }
    }
}
