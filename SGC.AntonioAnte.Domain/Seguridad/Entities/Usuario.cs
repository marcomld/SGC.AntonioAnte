using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGC.AntonioAnte.Domain.Seguridad.Entities
{
    public class Usuario : IdentityUser<Guid>
    {
        public string Identificacion { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;

        // 🔹 Nuevo: Llave foránea hacia Departamento (Nullable por si es SuperAdmin)
        public Guid? DepartamentoId { get; set; }
        public virtual Departamento? Departamento { get; set; } // Propiedad de Navegación

        public bool EstadoActivo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // 🔹 Nuevo: Relación inversa (Un usuario tiene muchas auditorías)
        public virtual ICollection<Auditoria> Auditorias { get; set; } = new List<Auditoria>();
    }
}
