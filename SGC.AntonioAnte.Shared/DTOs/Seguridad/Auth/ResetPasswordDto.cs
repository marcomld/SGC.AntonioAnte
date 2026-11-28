using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Seguridad.Auth
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El token de autorización es obligatorio.")]
        public string ResetToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        public string NuevaPassword { get; set; } = string.Empty;
    }
}
