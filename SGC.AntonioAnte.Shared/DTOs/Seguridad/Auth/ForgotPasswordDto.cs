using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Seguridad.Auth
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "El correo electrónico institucional es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato de correo electrónico no es válido.")]
        public string Email { get; set; } = string.Empty;
    }
}
