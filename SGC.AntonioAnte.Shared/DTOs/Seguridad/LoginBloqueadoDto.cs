using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Seguridad
{
    public class LoginBloqueadoDto
    {
        public bool CuentaBloqueada { get; set; } = true;
        public int SegundosRestantes { get; set; }
        public string MensajeError { get; set; } = string.Empty;
    }
}
