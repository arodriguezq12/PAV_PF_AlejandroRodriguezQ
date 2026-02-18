using System.ComponentModel.DataAnnotations;

namespace FarmaDual.Models.ViewModels
{
    public class RegisterVM
    {
        [Required, EmailAddress]
        public string Correo { get; set; }

        [Required, DataType(DataType.Password), MinLength(6)]
        public string Password { get; set; }

        [Required, DataType(DataType.Password), Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required, StringLength(50)]
        public string Identificacion { get; set; }

        [Required, StringLength(150)]
        public string NombreCompleto { get; set; }

        [Required]
        [RegularExpression("^[MF]$", ErrorMessage = "Use M o F.")]
        public string Genero { get; set; }

        [Required]
        public int TipoTarjetaId { get; set; }

        [Required]
        [RegularExpression(@"^\d{4}-\d{4}-\d{4}-\d{4}$", ErrorMessage = "Formato inválido: 0000-0000-0000-0000")]
        public string NumeroTarjeta { get; set; }
    }
}
