using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Seguridad.Auth
{
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "El token de acceso expirado es obligatorio.")]
        public string AccessToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "El token de refresco es obligatorio.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
