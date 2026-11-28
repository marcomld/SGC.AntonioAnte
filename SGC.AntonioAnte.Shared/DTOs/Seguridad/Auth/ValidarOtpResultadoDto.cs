using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Seguridad.Auth
{
    public class ValidarOtpResultadoDto
    {
        public bool Exitoso { get; set; }

        public string Mensaje { get; set; } = string.Empty;

        public string? ResetToken { get; set; }

        public int ExpiraEnSegundos { get; set; }
    }
}
