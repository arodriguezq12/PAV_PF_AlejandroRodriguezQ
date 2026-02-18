using System.ComponentModel.DataAnnotations;

namespace PAV_PF_AlejandroRodriguezQ.Models.ViewModels
{
    public class MedicamentoFormVM
    {
        [Required]
        [StringLength(30)]
        [Display(Name = "Código")]
        public string MedicamentoCodigo { get; set; }

        [Required]
        [StringLength(120)]
        public string Nombre { get; set; }

        [Required]
        [Range(typeof(decimal), "0.01", "9999999")]
        public decimal Precio { get; set; }

        [Required]
        [Display(Name = "Módulo")]
        public string ModuloCodigo { get; set; }

        [Required]
        [Display(Name = "Género")]
        public int GeneroId { get; set; }

        public bool Activo { get; set; }
    }
}
