using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Shared.DTOs.Seguridad.Usuarios
{
    public class UsuarioResponseDto
    {
        public Guid Id { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        // 🔹 Llave y Nombre para la UI
        public Guid? DepartamentoId { get; set; }
        public string NombreDepartamento { get; set; } = string.Empty;
        public bool EstadoActivo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
