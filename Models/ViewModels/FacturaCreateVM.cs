using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PAV_PF_AlejandroRodriguezQ.Models.ViewModels
{
    public class FacturaCreateVM
    {
        [Display(Name = "Identificación")]
        public string IdentificacionUsuario { get; set; }

        [Display(Name = "Exento de impuestos")]
        public bool EsExento { get; set; }

        public List<FacturaLineaVM> Lineas { get; set; } = new List<FacturaLineaVM>();
    }

    public class FacturaLineaVM
    {
        [Display(Name = "Medicamento")]
        public string MedicamentoCodigo { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }
    }
}
