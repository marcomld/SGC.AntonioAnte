using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Seguridad
{
    public class LoginDto
    {
        public string Identificacion { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
