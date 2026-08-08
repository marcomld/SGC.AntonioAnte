using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Common
{
    public class OperacionResultadoDto
    {
        // 🔹 1. Estructura para Serialización JSON (OBLIGATORIA)
        public bool Exitoso { get; set; } = false;
        public string Mensaje { get; set; } = string.Empty;

        // 🔹 2. Métodos de Fábrica para C# (Sintaxis limpia de 1 línea)
        public static OperacionResultadoDto Exito(string mensaje = "")
            => new() { Exitoso = true, Mensaje = mensaje };

        public static OperacionResultadoDto Fallo(string mensaje)
            => new() { Exitoso = false, Mensaje = mensaje };
    }
}
