using System.ComponentModel.DataAnnotations;

namespace FarmaDual.Models.ViewModels
{
    public class LoginVM
    {
        [Required, EmailAddress]
        public string Correo { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
