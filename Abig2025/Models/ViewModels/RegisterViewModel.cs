using System.ComponentModel.DataAnnotations;

namespace Abig2025.Models.ViewModels
{
    public class RegisterViewModel
    {
        // Honeypot para bots
        public string? Honeypot { get; set; } = string.Empty;

       // [Required(ErrorMessage = "El nombre es obligatorio")]
        //[Display(Name = "Nombre")]
       // [MaxLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string FirstName { get; set; } = string.Empty;

       // [Required(ErrorMessage = "El apellido es obligatorio")]
        //[Display(Name = "Apellido")]
        //[MaxLength(100, ErrorMessage = "El apellido no puede tener más de 100 caracteres")]
        public string LastName { get; set; } = string.Empty;

        //[Required(ErrorMessage = "El email es obligatorio")]
        //[EmailAddress(ErrorMessage = "El formato del email no es válido")]
        //[Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        //[Required(ErrorMessage = "La contraseña es obligatoria")]
        //[DataType(DataType.Password)]
        //[Display(Name = "Contraseña")]
        //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
          //  ErrorMessage = "La contraseña debe tener al menos 8 caracteres, incluir mayúsculas, minúsculas, números y caracteres especiales")]
        public string Password { get; set; } = string.Empty;

       // [Required(ErrorMessage = "Confirmar contraseña es obligatorio")]
        //[DataType(DataType.Password)]
        //[Display(Name = "Confirmar Contraseña")]
        //[Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Información adicional (opcional) - pueden ser null
        //[Display(Name = "DNI")]
        //[MaxLength(20)]
        public string? Dni { get; set; }

        //[Display(Name = "Teléfono")]
        //[MaxLength(20)]
        public string? Phone { get; set; }

        //[Display(Name = "Dirección")]
        //[MaxLength(255)]
        public string? Address { get; set; }

        //[Display(Name = "Ciudad")]
        //[MaxLength(100)]
        public string? City { get; set; }

        //[Display(Name = "Provincia")]
        //[MaxLength(100)]
        public string? Province { get; set; }

        //[Display(Name = "País")]
        //[MaxLength(100)]
        public string? Country { get; set; }

        //[Display(Name = "Fecha de Nacimiento")]
        //[DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        //[Required(ErrorMessage = "Debes aceptar los términos y condiciones")]
        //[Display(Name = "Términos y Condiciones")]
        //[Range(typeof(bool), "true", "true", ErrorMessage = "Debes aceptar los términos y condiciones")]
        public bool AcceptTerms { get; set; } 

        //[Required(ErrorMessage = "Debes aceptar la política de privacidad")]
        //[Display(Name = "Política de Privacidad")]
        //[Range(typeof(bool), "true", "true", ErrorMessage = "Debes aceptar la política de privacidad")]
        public bool AcceptPrivacy { get; set; }
    }
}