using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Seguridad
{
    public class LoginResultadoDto
    {
        public bool Exitoso { get; set; } = false;
        public TokenResponseDto? Tokens { get; set; }
        public bool CuentaBloqueada { get; set; } = false;
        public int SegundosRestantes { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
