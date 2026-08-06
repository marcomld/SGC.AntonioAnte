using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Domain.Seguridad.Entities
{
    public class Auditoria
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? UsuarioId { get; set; }
        public virtual Usuario? Usuario { get; set; } // 🔹 Nuevo: Propiedad de Navegación

        public string Accion { get; set; } = string.Empty; // Ej: "LOGIN", "CREATE", "UPDATE"
        public string Entidad { get; set; } = string.Empty; // Ej: "Usuario", "Predio"
        public string? EntidadId { get; set; }
        public string DireccionIp { get; set; } = string.Empty;
        public string Navegador { get; set; } = string.Empty;
        public string? DatosAdicionales { get; set; } // Ideal para guardar un JSON con los cambios
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
