using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Seguridad.Auth
{
    public class ValidarOtpDto
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no es válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código es obligatorio.")]
        [RegularExpression(
            @"^[0-9]{6}$",
            ErrorMessage = "El código debe contener 6 dígitos.")]
        public string Codigo { get; set; } = string.Empty;
    }
}
