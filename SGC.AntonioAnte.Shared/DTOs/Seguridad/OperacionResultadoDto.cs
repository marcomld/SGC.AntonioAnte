using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Seguridad
{
    public class OperacionResultadoDto
    {
        public bool Exitoso { get; set; } = false;
        public string Mensaje { get; set; } = string.Empty;
    }
}
