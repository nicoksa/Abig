// Models/ViewModels/RegisterViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Abig2025.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [Display(Name = "Nombre")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        [Display(Name = "Apellido")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirmar contraseña es requerido")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmPassword { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "Debes aceptar los términos y condiciones")]
        public bool AcceptTerms { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "Debes aceptar la política de privacidad")]
        public bool AcceptPrivacy { get; set; }
    }
}
