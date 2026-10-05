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
        public Guid? Id { get; set; }
        public object? Datos { get; set; }

        // 🔹 2. Métodos de Fábrica para C# (Sintaxis limpia)
        public static OperacionResultadoDto Exito(string mensaje = "", Guid? id = null, object? datos = null)
            => new() { Exitoso = true, Mensaje = mensaje, Id = id, Datos = datos };

        public static OperacionResultadoDto Fallo(string mensaje)
            => new() { Exitoso = false, Mensaje = mensaje };
    }
}
